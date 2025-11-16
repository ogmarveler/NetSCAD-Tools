using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Axis.Scad.Models;
using NetScad.Axis.Scad.Utility;
using NetScad.Core.Interfaces;
using NetScad.Core.Material;
using NetScad.Core.Measurements;
using NetScad.Core.Models;
using NetScad.Core.Primitives;
using NetScad.Core.Utility;
using NetScad.Designer.Repositories;
using NetScad.Designer.Utility;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetScad.Core.Measurements.Colors;
using static NetScad.Core.Measurements.Conversion;
using static NetScad.Core.Measurements.Selector;
using static NetScad.Core.Utility.WrapInModule;

namespace NetScad.UI.ViewModels
{
    public class ScadObjectViewModel : ValidatableBase
    {
        private ObservableCollection<SolidDimensions> _solidDimensions;
        private ObservableCollection<GeneratedModule> _axesModulesList;
        private ObservableCollection<ModuleDimensions> _moduleDimensions;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsDifferences;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsUnions;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsIntersections;
        private AxisDimensions? _axisDimensions = new();
        private List<string>? _axesList = [];
        private SqliteConnection? _dbConnection;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private double _lengthMM = 0;
        private double _widthMM = 0;
        private double _heightMM = 0;
        private double _thicknessMM = 0;
        private FilamentType _selectedFilament = FilamentType.Other;
        private UnitSystem _selectedUnit = UnitSystem.Metric;
        private UnitSystem _selectedAxisUnit = UnitSystem.Metric;
        private GeneratedModule? _selectedAxis = new();
        private string? _selectedAxisValue;
        private string _objectAxisDisplay = string.Empty;
        private string _objectAxisUnitDisplay = string.Empty;
        private bool _unitHasChanged;
        private bool _isMetric = true;
        private bool _isImperial = false;
        private bool _isAxisMetric = true;
        private bool _isAxisImperial = false;
        private bool _axesSelectEnabled = true;
        private bool _appendObject = false;
        public int _decimalPlaces;
        public int? _axisId = null;
        public int? _solidDimensionsId;
        private readonly string _objectFilePath;
        private List<ScrewSize>? _screwSizes;
        private ScrewSize? _selectedScrewSize;
        private string _selectedScrewProperty = "Screw Thread";
        private OperationType _selectedOperationType = OperationType.Union;
        private bool _unionButton;
        private bool _differenceButton;
        private bool _intersectionButton;
        private bool _saveFileButton;
        private double _radiusMM = 0;
        private double _radius1MM = 0;
        private double _radius2MM = 0;
        private double _cylinderHeightMM = 0;
        private bool _isCubeSelected = true;
        private bool _isRoundCubeSelected = false;
        private bool _isCylinderSelected = false;
        private bool _isSurfaceSelected = false;
        private bool _isRoundSurfaceSelected = false;
        private bool _axisStored = false;
        private List<ServerRack>? _serverRackSizes;
        private ServerRack? _selectedServerRack;
        private string _selectedServerRackWidthType = "InnerWidth";
        private double _xOffsetMM = 0;
        private double _yOffsetMM = 0;
        private double _zOffsetMM = 0;
        private bool _isNoneOperation = true;
        private bool _isUnionOperation = false;
        private bool _isDifferenceOperation = false;
        private bool _isIntersectionOperation = false;
        private double _AxisXPositionMM = 0;
        private double _AxisYPositionMM = 0;
        private double _AxisZPositionMM = 0;
        private double _axisXPositionMM = 0;
        private double _axisYPositionMM = 0;
        private double _axisZPositionMM = 0;
        private double _xRotate = 0;
        private double _yRotate = 0;
        private double _zRotate = 0;
        private string _selectedShapeType = "Cube";
        private bool _removeAxis = false;
        private bool _isPreRendered = false;
        private bool _originalRemoveAxis = false;
        private bool _isFileOpen = false;
        private bool _exportToStl = false;
        private string _originalAxisCall = string.Empty;
        private bool _isModalOpen;
        private string _modalTitle = string.Empty;
        private string _modalContent = string.Empty;
        private string _surfaceFilePath = string.Empty;
        private double _surfaceScaleX = 1;
        private double _surfaceScaleY = 1;
        private double _surfaceScaleZ = 1;
        private bool _surfaceCenter = true;
        private bool _surfaceInvert = false;
        private bool _autoSmoothFile = false;
        private int _surfaceConvexity = 1;


        [UnconditionalSuppressMessage("Trimming", "IL2026")]
        [UnconditionalSuppressMessage("AOT", "IL3050")]
        public ScadObjectViewModel()
        {
            SolidDimensions = [];
            ModuleDimensions = [];
            ModuleDimensionsUnions = [];
            ModuleDimensionsDifferences = [];
            ModuleDimensionsIntersections = [];
            _solidDimensions = [];
            _moduleDimensions = [];
            _axesModulesList = [];
            _moduleDimensionsUnions = [];
            _moduleDimensionsDifferences = [];
            _moduleDimensionsIntersections = [];
            DbConnection = App.Services!.GetRequiredService<SqliteConnection>(); // Get the DbConnection from the DI container
            _ = ClearObjectAsync();
            _ = GetAxesList();
            AxisStored = false;
            _decimalPlaces = Designer.Repositories.SolidDimensions.OpenSCAD_DecimalPlaces;
            FilamentTypes = [.. Enum.GetValues<FilamentType>()];
            UnitSystemValues = [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
            SelectedUnitValue = UnitSystem.Metric;
            SelectedAxisValue = AxesList?.FirstOrDefault() ?? string.Empty;
            _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == SelectedAxisValue);
            ScrewSizes = new ScrewSizeService().ScrewSizes;
            OperationTypes = [.. Enum.GetValues<OperationType>()];
            _objectFilePath = App.Services!.GetRequiredService<IScadPathProvider>().ScadPath;
            ServerRackSizes = [.. Enumerable.Range(1, 12).Select(ServerRackDimensions.GetByRackUnits).OfType<ServerRack>()]; // OfType automatically filters nulls AND casts
        }

        /**** Outer Dimensions DataGrid ****/
        public async Task GetDimensionsPartAsync()
        {
            await AxisDimensionsExtensions.CreateTable(DbConnection!); // Ensure AxisDimensions table exists first
            await SolidDimensionsExtensions.CreateTable(DbConnection!); // Ensure SolidDimensions table exists
            await ModuleDimensionsExtensions.CreateTable(DbConnection!); // Ensure ModuleDimensions table exists

            // Get records from database with both Axis and Module joins
            var records = await new SolidDimensions().GetByNameWithAxisAndModuleAsync(DbConnection!, Name); // Gets SolidDimensions with related AxisDimensions AND ModuleDimensions
            var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(DbConnection!, Name);

            // Update ObservableCollections
            ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
            SolidDimensions = new ObservableCollection<SolidDimensions>(records);

            // Update differences and unions buttons
            IsCubeSelected = _isCubeSelected;
            IsRoundCubeSelected = _isRoundCubeSelected;
            IsCylinderSelected = _isCylinderSelected;
        }

        // Clear all input fields
        public Task ClearInputsAsync()
        {
            Name = string.IsNullOrEmpty(Name) ? string.Empty : Name; // Needs to remain since object process can have multiple components
            Description = string.Empty;  // Needed for making sure that parts added don't have the same description
            LengthMM = 0;
            WidthMM = 0;
            HeightMM = 0;
            ThicknessMM = 0;
            RadiusMM = 0;
            Radius1MM = 0;
            Radius2MM = 0;
            CylinderHeightMM = 0;
            SelectedServerRack = null;
            SelectedSolidType = "Select Solid";
            SelectedServerRackWidthType = string.Empty;
            SelectedScrewProperty = string.Empty;
            SelectedScrewSize = null;
            SelectedFilament = FilamentType.Other;
            SelectedOperationType = OperationType.Union;
            SurfaceFilePath = string.Empty;
            AutoSmoothFile = false;
            SurfaceCenter = false;
            SurfaceConvexity = 1;
            SurfaceInvert = false;
            SurfaceScaleX = 0;
            SurfaceScaleY = 0;
            SurfaceScaleZ = 0;
            XOffsetMM = 0;
            YOffsetMM = 0;
            ZOffsetMM = 0;
            XRotate = 0;
            YRotate = 0;
            ZRotate = 0;
            return Task.CompletedTask;
        }

        // Clear all object fields
        public async Task ClearObjectAsync()
        {
            Name = string.Empty;
            Description = string.Empty;
            await ClearInputsAsync(); // Clear input fields
            ModuleDimensions = [];
            SolidDimensions = [];
            SelectedOperationType = OperationType.Union;
            SelectedUnitValue = UnitSystem.Metric;
            AppendObject = false;
            IsNoneOperation = true;
            IsUnionOperation = false;
            IsDifferenceOperation = false;
            DifferenceButton = false;
            UnionButton = false;
            SaveFileButton = false;
            IsCubeSelected = false;
            IsRoundCubeSelected = false;
            IsCylinderSelected = false;
            IsSurfaceSelected = false;
            IsPreRendered = false;
            ScrewSizes = _screwSizes;
            ServerRackSizes = _serverRackSizes;
            RemoveAxis = false;
            _originalRemoveAxis = false;
            await GetDimensionsPartAsync(); // Refresh the DataGrid
        }

        // Create object and save to database
        public async Task<int> CreateObjectAsync()
        {
            // Make sure a solid type has been selected
            /*** TO DO: Add message box for warning ***/
            switch (SelectedSolidType)
            {
                case "Cube":
                    IsCubeSelected = true;
                    break;
                case "Round Cube":
                    IsRoundCubeSelected = true;
                    break;
                case "Cylinder":
                    IsCylinderSelected = true;
                    break;
                case "Surface":
                    IsSurfaceSelected = true;
                    break;
                default: return 0;
            }
            int? id = null;  // Object for returning the row id

            if (_axisId is null) // New axis being applied
                await CreateAxisAsync(); // Create or get AxisDimensions and return its Id

            var newObject = new SolidDimensions  // Create new SolidDimensions instance
            {
                Name = Name,
                Description = Description,
                Material = SelectedFilament.ToString(),
                OperationType = SelectedOperationType.ToString(),
                SolidType = SelectedSolidType,
                Length_MM = LengthMM,
                Width_MM = WidthMM,
                Height_MM = HeightMM,
                Thickness_MM = ThicknessMM,
                Radius_MM = RadiusMM,
                Radius1_MM = Radius1MM,
                Radius2_MM = Radius2MM,
                CylinderHeight_MM = CylinderHeightMM,
                XOffset_MM = XOffsetMM,
                YOffset_MM = YOffsetMM,
                ZOffset_MM = ZOffsetMM,
                XRotate = XRotate,
                YRotate = YRotate,
                ZRotate = ZRotate,
                CreatedAt = DateTime.UtcNow,
                AxisDimensionsId = _axisId
            };

            newObject.OSCADMethod = await GenerateOSCADAsync(oDim: newObject); // Get the OSCAD method

            // Determine if new row being added is appending the current object or is a new object
            if (AppendObject)
            {
                await newObject.UpsertAsync(DbConnection!); /* Save to database, add to object */
            }
            else { await newObject.UpsertAsync(DbConnection!); /* Save to database, new object, overwrite existing object */ }

            if (AxesSelectEnabled)
            {
                var axisUsed = string.Empty;
                if (SolidDimensions.Count > 0)
                    axisUsed = SolidDimensions.SingleOrDefault()?.AxisOSCADMethod;
                else
                    AxesSelectEnabled = false; // Disable axis selection after it has been created, to use within the same object
                ObjectAxisDisplay = StringFunctions.FormatAxisDisplay(axisUsed); // Format for display
            }

            id = newObject.Id;

            AppendObject = true; // Set to true since after inserting new row, appending to existing set, or updating, object may be appended.
            switch (SelectedOperationType)
            {
                case OperationType.Union: await CreateDifferenceModuleAsync(); break; // Update Union modules for real-time updates through difference module
                case OperationType.Difference: await CreateDifferenceModuleAsync(); break; // Update Difference modules for real-time updates
                case OperationType.Intersection: await CreateIntersectionModuleAsync(); break; // Update Intersection modules for real-time updates
            }

            await ClearInputsAsync(); // After new part added, make sure that description is cleared out
            await GetDimensionsPartAsync(); // Refresh datagrids
            return id ?? 0;
        }

        public async Task UpdateAxisTranslateAsync()
        {
            if (!AxisStored) return;  // No axis has been applied yet

            /** Need to find the original scad statement before updating variables **/
            if (_originalRemoveAxis)
                _originalAxisCall = $"// translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}";
            else
                _originalAxisCall = $"translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}";

            _axisDimensions = new AxisDimensions
            {
                Theme = _selectedAxis?.Theme!,
                OSCADMethod = _selectedAxis?.CallingMethod!,
                Unit = _selectedAxis?.Unit!,
                MinX = _selectedAxis!.MinX,
                MaxX = _selectedAxis!.MaxX,
                MinY = _selectedAxis!.MinY,
                MaxY = _selectedAxis!.MaxY,
                MinZ = _selectedAxis!.MinZ,
                MaxZ = _selectedAxis!.MaxZ,
                CreatedAt = DateTime.UtcNow,
            };
            _axisDimensions.OSCADMethod = $"{_axisDimensions.IncludeMethod} {_selectedAxis.CallingMethod}";
            _axisId = await _axisDimensions.UpsertAsync(DbConnection!); // Save to database

            if (_selectedAxisUnit == UnitSystem.Imperial) // Need to convert since logic is textboxes are display variables
            {
                // Convert offsets to metric for OpenSCAD
                _AxisXPositionMM = Math.Round(InchesToMillimeter(_axisXPositionMM), _decimalPlaces); // new X adjustment
                _AxisYPositionMM = Math.Round(InchesToMillimeter(_axisYPositionMM), _decimalPlaces); // new Y adjustment
                _AxisZPositionMM = Math.Round(InchesToMillimeter(_axisZPositionMM), _decimalPlaces); // new Z adjustment
            }
            else
            {
                _AxisXPositionMM = _axisXPositionMM;
                _AxisYPositionMM = _axisYPositionMM;
                _AxisZPositionMM = _axisZPositionMM;
            }

            var filePath = Path.Combine(_objectFilePath, "Solids", "object.scad");  // Where the main output is stored
            if (!File.Exists(filePath)) return;
            // Match with CreateAxisAsync logic
            var wrappedAxisCall = string.Empty;
            if (RemoveAxis)
            {
                wrappedAxisCall = $"// translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}"; // Wrap axis call in translate module, this is the search string for replacement
                _originalRemoveAxis = true;
            }
            else
            {
                // Update axis call with new translate values
                wrappedAxisCall = $"translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}"; // Wrap axis call in translate module, this is the search string for replacement
                _originalRemoveAxis = false;
            }

            _ = UpdateFIle.ChangeContentBlockFile(oldCodeBlock: _originalAxisCall, newCodeBlock: wrappedAxisCall, filePath: filePath);  // Append updated axis call to object.scad file
        }

        public async Task CreateAxisAsync()
        {
            _axisDimensions = new AxisDimensions
            {
                Theme = _selectedAxis?.Theme!,
                OSCADMethod = _selectedAxis?.CallingMethod!,
                Unit = _selectedAxis?.Unit!,
                MinX = _selectedAxis!.MinX,
                MaxX = _selectedAxis!.MaxX,
                MinY = _selectedAxis!.MinY,
                MaxY = _selectedAxis!.MaxY,
                MinZ = _selectedAxis!.MinZ,
                MaxZ = _selectedAxis!.MaxZ,
                CreatedAt = DateTime.UtcNow,
            };
            _axisDimensions.OSCADMethod = $"{_axisDimensions.IncludeMethod} {_selectedAxis.CallingMethod}";
            _axisId = await _axisDimensions.UpsertAsync(DbConnection!); // Save to database

            if (_axisId != null)  // If successful, write to object.scad file
            {
                if (_selectedAxisUnit == UnitSystem.Imperial) // Need to convert since logic is based on temporary oDim variable
                {
                    // Convert offsets to metric for OpenSCAD
                    _AxisXPositionMM = Math.Round(InchesToMillimeter(_axisXPositionMM), _decimalPlaces);
                    _AxisYPositionMM = Math.Round(InchesToMillimeter(_axisYPositionMM), _decimalPlaces);
                    _AxisZPositionMM = Math.Round(InchesToMillimeter(_axisZPositionMM), _decimalPlaces);
                }
                else
                {
                    _AxisXPositionMM = _axisXPositionMM;
                    _AxisYPositionMM = _axisYPositionMM;
                    _AxisZPositionMM = _axisZPositionMM;
                }

                // Match with UpdateAxisTranslateAsync logic
                var wrappedAxisCall = $"translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}"; // Wrap axis call in translate module, this is the search string for replacement
                // Put Scad object file together
                var sb = new StringBuilder();
                sb.AppendLine("// Custom axis");
                sb.AppendLine(_axisDimensions?.IncludeMethod); // Include axis
                sb.AppendLine(wrappedAxisCall); // Use axis
                sb.AppendLine();
                await Output.WriteToSCAD(content: sb.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", "object.scad"), overWrite: true, cancellationToken: new CancellationToken());
                AxisStored = true;
                RemoveAxis = false;
                AxesSelectEnabled = false;
                await GetDimensionsPartAsync();
                ObjectAxisDisplay = StringFunctions.FormatAxisDisplay(_axisDimensions?.OSCADMethod); // Format for display
                ObjectAxisUnitDisplay = _axisDimensions?.Unit == "mm" ? "Metric (mm)" : "Imperial (in)";
                AxisXPositionMM = _axisDimensions!.MinX;  // Set to axis value, Min X
                AxisYPositionMM = _axisDimensions!.MinY;  // Set to axis value, Min Y
                AxisZPositionMM = _axisDimensions!.MinZ;  // Set to axis value, Min Z
                SelectedUnitValue = _axisDimensions!.Unit == "mm" ? UnitSystem.Metric : UnitSystem.Imperial; // Set object unit to axis unit as default unit system
            }
        }

        public async Task<string> GenerateOSCADAsync(SolidDimensions? oDim = null)
        {
            if (IsCubeSelected || IsRoundCubeSelected || IsSurfaceSelected || IsRoundSurfaceSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim!.Length_MM = Math.Round(InchesToMillimeter(oDim.Length_MM), _decimalPlaces);
                    oDim.Width_MM = Math.Round(InchesToMillimeter(oDim.Width_MM), _decimalPlaces);
                    oDim.Height_MM = Math.Round(InchesToMillimeter(oDim.Height_MM), _decimalPlaces);
                    oDim.Thickness_MM = Math.Round(InchesToMillimeter(oDim.Thickness_MM), _decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), _decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), _decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), _decimalPlaces);
                }

                if (IsRoundCubeSelected)
                {
                    // Based on width and height (if applicable)
                    oDim!.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), _decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), _decimalPlaces);

                    // Generate a rounded cube with x,y offset for rounding - OpenSCAD code
                    var roundedCubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim!.Length_MM }, { "size_y", oDim!.Width_MM }, { "size_z", oDim!.Height_MM }, { "round_r", oDim.Round_r_MM }, { "round_h", oDim.Round_h_MM }, { "resolution", oDim.Resolution }
                    };
                    var roundedCube = OScad3D.RoundedCube.ToScadObject(roundedCubeParams);
                    var rotated = await GetRotateAsync(roundedCube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = await GetTranslateAsync(rotated, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower()).Trim();
                }
                else if (IsCubeSelected)
                {
                    // Generate a cube - OpenSCAD code
                    var cubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim!.Length_MM }, { "size_y", oDim!.Width_MM }, { "size_z", oDim!.Height_MM },
                    };
                    var cube = OScad3D.Cube.ToScadObject(cubeParams);
                    var rotated = await GetRotateAsync(cube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = await GetTranslateAsync(rotated, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower()).Trim();
                }
                else if (IsSurfaceSelected)
                {
                    var surfaceParams = new Dictionary<string, object>
                    {
                        { "file", $"{_surfaceFilePath.Replace("\\", "/")}" },
                        { "scaleX", _surfaceScaleX },
                        { "scaleY", _surfaceScaleY },
                        { "scaleZ", _surfaceScaleZ },
                        { "invert", _surfaceInvert },
                        { "center", _surfaceCenter },
                        { "convexity", _surfaceConvexity }
                    };
                    var surface = OScad3D.Surface.ToScadObject(surfaceParams);
                    var rotated = await GetRotateAsync(surface, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = await GetTranslateAsync(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower()).Trim();
                }
                else if (IsRoundSurfaceSelected)
                {
                    // Based on width and height (if applicable)
                    oDim!.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), _decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), _decimalPlaces);

                    var surfaceParams = new Dictionary<string, object>
                    {
                        { "file", $"{_surfaceFilePath.Replace("\\", "/")}" },
                        { "scaleX", _surfaceScaleX },
                        { "scaleY", _surfaceScaleY },
                        { "scaleZ", _surfaceScaleZ },
                        { "invert", _surfaceInvert },
                        { "center", _surfaceCenter },
                        { "convexity", _surfaceConvexity },
                        { "round_r", oDim.Round_r_MM },
                        { "round_h", oDim.Round_h_MM },
                        { "resolution", oDim.Resolution },
                    };
                    var roundSurface = OScad3D.RoundedSurface.ToScadObject(surfaceParams);
                    var rotated = await GetRotateAsync(roundSurface, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = await GetTranslateAsync(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower()).Trim();
                }
            }
            else if (IsCylinderSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert cylinder dimensions to metric for OpenSCAD
                    oDim!.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), _decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), _decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), _decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), _decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), _decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), _decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), _decimalPlaces);
                }

                var cylParams = new Dictionary<string, object> 
                { 
                    { "r", oDim!.Radius_MM },
                    { "r1", oDim!.Radius1_MM },
                    { "r2", oDim!.Radius2_MM },
                    { "h", oDim!.CylinderHeight_MM }, 
                    { "resolution", 180 } 
                };
                var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
                var rotated = await GetRotateAsync(cylinder, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                var translate = await GetTranslateAsync(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM);
                return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower()).Trim();
            }
            
            return string.Empty;
        }

        // Make the object's position changeable if this function is called separately. Translate of a translate.
        // Use case for shifting an entire set of child objects as part of an IScadObject
        public Task<IScadObject> GetTranslateAsync(IScadObject scadObject, double XOffset_MM, double YOffset_MM, double ZOffset_MM)
        {
            if (IsCubeSelected || IsRoundCubeSelected || IsSurfaceSelected || IsRoundSurfaceSelected)
            {
                var oDim = new SolidDimensions  // Create new SolidDimensions instance for getting RoundRadius and RoundHeight
                {
                    Length_MM = LengthMM,
                    Width_MM = WidthMM,
                    Height_MM = HeightMM,
                    Thickness_MM = ThicknessMM,
                };

                if (_selectedUnit == UnitSystem.Imperial) // Need to convert since logic is based on temporary oDim variable
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim.Length_MM = Math.Round(InchesToMillimeter(oDim.Length_MM), _decimalPlaces);
                    oDim.Width_MM = Math.Round(InchesToMillimeter(oDim.Width_MM), _decimalPlaces);
                    oDim.Height_MM = Math.Round(InchesToMillimeter(oDim.Height_MM), _decimalPlaces);
                    oDim.Thickness_MM = Math.Round(InchesToMillimeter(oDim.Thickness_MM), _decimalPlaces);
                    // Convert offsets to metric for OpenSCAD
                    XOffset_MM = Math.Round(InchesToMillimeter(_xOffsetMM), _decimalPlaces);
                    YOffset_MM = Math.Round(InchesToMillimeter(_yOffsetMM), _decimalPlaces);
                    ZOffset_MM = Math.Round(InchesToMillimeter(_zOffsetMM), _decimalPlaces);
                }

                if (IsRoundCubeSelected || IsRoundSurfaceSelected)  // Adjust for Minkowski offset
                {
                    // Based on width and height (if applicable)
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), _decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), _decimalPlaces);

                    switch (SelectedOperationType)
                    {
                        case OperationType.Union:
                            XOffset_MM += oDim.Round_r_MM;
                            YOffset_MM += oDim.Round_r_MM;
                            ZOffset_MM += -oDim.Round_h_MM;
                            break;
                        case OperationType.Difference:
                            XOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            YOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            ZOffset_MM += -oDim.Round_h_MM + oDim.Thickness_MM;
                            break;
                        case OperationType.Intersection:
                            XOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            YOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            ZOffset_MM += -oDim.Round_h_MM + oDim.Thickness_MM;
                            break;
                    }
                }
                else if (IsCubeSelected || IsSurfaceSelected)  // For regular cubes, if thickness has been added, then factor this into offsets
                {
                    switch (SelectedOperationType)
                    {
                        case OperationType.Difference:
                            XOffset_MM += oDim.Thickness_MM;
                            YOffset_MM += oDim.Thickness_MM;
                            ZOffset_MM += oDim.Thickness_MM;
                            break;
                        case OperationType.Intersection:
                            XOffset_MM += oDim.Thickness_MM;
                            YOffset_MM += oDim.Thickness_MM;
                            ZOffset_MM += oDim.Thickness_MM;
                            break;
                    }
                }
            }
            else if (IsCylinderSelected)
            {
                var oDim = new SolidDimensions  // Create new SolidDimensions instance for getting RoundRadius and RoundHeight
                {
                    Radius_MM = RadiusMM,
                    Radius1_MM = Radius1MM,
                    Radius2_MM = Radius2MM,
                    CylinderHeight_MM = CylinderHeightMM,
                };

                if (_selectedUnit == UnitSystem.Imperial) // Need to convert since logic is based on temporary oDim variable
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), _decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), _decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), _decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), _decimalPlaces);

                    // Convert offsets to metric for OpenSCAD
                    XOffset_MM = Math.Round(InchesToMillimeter(_xOffsetMM), _decimalPlaces);
                    YOffset_MM = Math.Round(InchesToMillimeter(_yOffsetMM), _decimalPlaces);
                    ZOffset_MM = Math.Round(InchesToMillimeter(_zOffsetMM), _decimalPlaces);
                }
            }

            var translateParams = new Dictionary<string, object>
            {
                { "x", XOffset_MM },
                { "y", YOffset_MM },
                { "z", ZOffset_MM },
                { "children", new IScadObject[] { scadObject } }
            };
            var translate = OScadTransform.Translate.ToScadObject(translateParams);
            return Task.FromResult(translate);
        }

        public Task<IScadObject> GetRotateAsync(IScadObject scadObject, double xRotate, double yRotate, double zRotate)
        {
            // Only apply rotation if any rotation value is non-zero
            if (xRotate == 0 && yRotate == 0 && zRotate == 0)
                return Task.FromResult(scadObject);

            var rotateParams = new Dictionary<string, object>
            {
                { "ax", xRotate },
                { "ay", yRotate },
                { "az", zRotate },
                { "children", new IScadObject[] { scadObject } }
            };
            var rotate = OScadModify.Rotate.ToScadObject(rotateParams);
            return Task.FromResult(rotate);
        }

        public async Task PartsToScadFilesAsync()
        {
            // Get latest data from DB
            await GetDimensionsPartAsync();
            /*** Parts scad file ***/
            var sbpart = new StringBuilder();
            var fileName = string.Empty;
            ModuleDimensions moduleUpdate;

            var objUDim = ModuleDimensionsUnions.Where(x => x.SolidType == "Object");
            var objDDim = ModuleDimensionsDifferences.Where(x => x.SolidType == "Object");
            var objIDim = ModuleDimensionsIntersections.Where(x => x.SolidType == "Object");
            fileName = $"{Name.Replace(" ", "_").Trim().ToLower()}.scad"; /*_{Description.Replace(" ", "_").Trim().ToLower()}_cube*/
            var moduleIncludeMethod = $"include <{fileName}>;";
            // Update ModuleDimensions OSCADMethod in DB
            moduleUpdate = new ModuleDimensions
            {
                ObjectDescription = Description,
                ObjectName = Name,
                SolidType = "Object",
                IncludeMethod = moduleIncludeMethod,
                CreatedAt = DateTime.UtcNow
            };
            await moduleUpdate.UpdateIncludeMethodByNameDescriptionSolidTypeAsync(DbConnection!);

            // Parts file creation
            sbpart.AppendLine($"//Use in main file: {moduleIncludeMethod}");
            sbpart.AppendLine();

            // Put in Scad solids file
            foreach (SolidDimensions module in SolidDimensions) // Get child objects in difference or union modules
            {
                sbpart.AppendLine($"// {module.Name} - Solid Type: {module.SolidType}, Description: {module.Description}, Operation Type: {module.OperationType}");
                sbpart.AppendLine(module.OSCADMethod);
                sbpart.AppendLine();
            }

            foreach (ModuleDimensions module in ModuleDimensions.Where(m => m.SolidType == "Object")) // Get difference or union rows for cubes
            {
                sbpart.AppendLine($"// {module.ObjectName} - Type: {module.ModuleType}");
                sbpart.AppendLine(module.OSCADMethod);
                sbpart.AppendLine();
            }

            // If difference functions present, then call those, likely unions are child objects
            if (objDDim.Any())
            {
                foreach (ModuleDimensions module in objDDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (objUDim.Any())
            {
                foreach (ModuleDimensions module in objUDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (objIDim.Any())
            {
                foreach (ModuleDimensions module in objIDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }

            // Write to part file with name of object
            await Output.WriteToSCAD(content: sbpart.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", fileName), overWrite: true, cancellationToken: new CancellationToken());
            // Refresh the DataGrid
            await GetDimensionsPartAsync(); // Refresh the DataGrid
        }

        // Deletes the selected item from any DataGrid (Cube, Cylinder, or Module)
        public async Task DeleteSelectedItemAsync(object? selectedItem)
        {
            if (selectedItem == null) return;

            try
            {
                switch (selectedItem)
                {
                    case SolidDimensions solid:
                        await solid.DeleteAsync(DbConnection!);
                        SolidDimensions.Remove(solid);
                        break;

                    case ModuleDimensions module:
                        await module.DeleteAsync(DbConnection!);
                        ModuleDimensions.Remove(module);
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown item type: {selectedItem.GetType().Name}");
                        return;
                }

                // Refresh the DataGrids and files after successful deletion
                await PartsToScadFilesAsync();  // Only update parts file
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting item: {ex.Message}");
            }
        }

        public async Task ObjectToScadFilesAsync()
        {
            // Get any additional updates to parts
            await PartsToScadFilesAsync();

            // Put Scad object file together
            var sb = new StringBuilder();
            sb.AppendLine("// Solid components used in main object");
            if (ModuleDimensions.Any())
            {
                foreach (string includeMethod in ModuleDimensions.Select(y => y.IncludeMethod).Distinct().ToList())
                {
                    sb.AppendLine(includeMethod);  // Include parts
                }
            }
            sb.AppendLine(); // Calling methods below

            /* Generic difference() { union() { } } wrapper
            This means that all modules will be bundled into the union section
            This has to do with Add/Subtract logic of components that are a difference from a complete set
            The custom object designer is intended to create components, and then allow a user to determine the order of adds and subtracts within the entire object.
            Example: a cylinder with a raised boss and a hole through it is added to the object, not subtracted from it
            Example: a cylinder with a raised boss can be interpreted either way, so manual adjustments will be necessary if different logic intended
            This has to do with subtractions from unions possibly subtracting the raised boss from a cylinder that was intended to be added after a previous cube difference.
            Functional programming logic is accounted for at a base level, but for user specifics, manual adjustments will be needed */

            sb.AppendLine($"difference() {{");
            sb.AppendLine($"   union() {{");

            // If difference functions present, then call those, likely unions are child objects
            if (ModuleDimensionsDifferences.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsDifferences)
                {
                    sb.Append($"        "); // Formatting
                    sb.AppendLine(module.Name);
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (ModuleDimensionsUnions.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsUnions)
                {
                    sb.Append($"        "); // Formatting
                    sb.AppendLine(module.Name);
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (ModuleDimensionsIntersections.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsIntersections)
                {
                    sb.Append($"        "); // Formatting
                    sb.AppendLine(module.Name);
                }
            }

            sb.AppendLine($"    }}");  // Union close bracket
            sb.AppendLine($"}}"); // Difference close bracket

            // Write the call methods to the main object.scad file
            var filePath = Path.Combine(_objectFilePath, "Solids", "object.scad");
            await Output.AppendToSCAD(content: sb.ToString(), filePath: filePath, cancellationToken: new CancellationToken());

            // Open the file in whatever the user has designated as the SCAD IDE associated with opening .scad files
            // Handle the case where the file could not be opened
            _isFileOpen = await ScadFileOperations.OpenScadFileAsync(filePath, allowDuplicates: false);

            if (ExportToStl) // If export to STL is enabled, do so after creating object.scad
                await ExportToSTLAsync();
        }

        // Simplified version using ScadFileOperations
        public async Task ExportToSTLAsync()
        {
            // Remove axis first before exporting
            var tempRemoveAxis = _removeAxis;
            RemoveAxis = true;
            await UpdateAxisTranslateAsync();

            // Then export to STL
            var scadFile = Path.Combine(_objectFilePath, "Solids", "object.scad");
            var stlFile = Path.Combine(_objectFilePath, "Solids", "object.stl");
            await ScadFileOperations.ExportToStlAsync(scadFile, stlFile);

            // Then restore axis if needed
            RemoveAxis = tempRemoveAxis;
            await UpdateAxisTranslateAsync();

            // Set ExportToStl back to false
            ExportToStl = false;
        }

        // Add this property to combine all module dimensions
        public ObservableCollection<ModuleDimensions> AllModuleDimensions
        {
            get
            {
                var combined = new ObservableCollection<ModuleDimensions>();
                foreach (var item in ModuleDimensionsUnions)
                    combined.Add(item);
                foreach (var item in ModuleDimensionsDifferences)
                    combined.Add(item);
                foreach (var item in ModuleDimensionsIntersections)
                    combined.Add(item);
                return combined;
            }
        }

        /**** Unit Conversion ****/
        public async Task ConvertInputs(int decimalPlaces)
        {
            if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged)
            {
                await ConvertInputsImperial(decimalPlaces);
            }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged)
            {
                await ConvertInputsMetric(decimalPlaces);
            }
            IsImperial = SelectedUnitValue != UnitSystem.Metric;
            IsMetric = SelectedUnitValue == UnitSystem.Metric;
        }

        private Task ConvertInputsImperial(int decimalPlaces)
        {
            // Convert from metric unit system to imperial (mm to inches)
            LengthMM = Math.Round(MillimeterToInches(_lengthMM), decimalPlaces);
            WidthMM = Math.Round(MillimeterToInches(_widthMM), decimalPlaces);
            HeightMM = Math.Round(MillimeterToInches(_heightMM), decimalPlaces);
            ThicknessMM = Math.Round(MillimeterToInches(_thicknessMM), decimalPlaces);
            RadiusMM = Math.Round(MillimeterToInches(_radiusMM), decimalPlaces);
            Radius1MM = Math.Round(MillimeterToInches(_radius1MM), decimalPlaces);
            Radius2MM = Math.Round(MillimeterToInches(_radius2MM), decimalPlaces);
            CylinderHeightMM = Math.Round(MillimeterToInches(_cylinderHeightMM), decimalPlaces);
            XOffsetMM = Math.Round(MillimeterToInches(_xOffsetMM), decimalPlaces);
            YOffsetMM = Math.Round(MillimeterToInches(_yOffsetMM), decimalPlaces);
            ZOffsetMM = Math.Round(MillimeterToInches(_zOffsetMM), decimalPlaces);
            UnitHasChanged = false;
            return Task.CompletedTask;
        }

        private Task ConvertInputsMetric(int decimalPlaces)
        {
            // Convert from imperial unit system to metric (inches to mm)
            LengthMM = Math.Round(InchesToMillimeter(_lengthMM), decimalPlaces);
            WidthMM = Math.Round(InchesToMillimeter(_widthMM), decimalPlaces);
            HeightMM = Math.Round(InchesToMillimeter(_heightMM), decimalPlaces);
            ThicknessMM = Math.Round(InchesToMillimeter(_thicknessMM), decimalPlaces);
            RadiusMM = Math.Round(InchesToMillimeter(_radiusMM), decimalPlaces);
            Radius1MM = Math.Round(MillimeterToInches(_radius1MM), decimalPlaces);
            Radius2MM = Math.Round(MillimeterToInches(_radius2MM), decimalPlaces);
            CylinderHeightMM = Math.Round(InchesToMillimeter(_cylinderHeightMM), decimalPlaces);
            XOffsetMM = Math.Round(InchesToMillimeter(_xOffsetMM), decimalPlaces);
            YOffsetMM = Math.Round(InchesToMillimeter(_yOffsetMM), decimalPlaces);
            ZOffsetMM = Math.Round(InchesToMillimeter(_zOffsetMM), decimalPlaces);
            UnitHasChanged = false;
            return Task.CompletedTask;
        }

        /**** Axes List DataGrid ****/
        public Task GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            _axesModulesList = parser.AxesModulesList(filePath);

            // Filter and select based on AXIS unit system (not general unit system)
            var filteredAxes = SelectedAxisUnitValue switch
            {
                UnitSystem.Metric => _axesModulesList.Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_MM_")).Select(x => x.CallingMethod!).ToList(),
                UnitSystem.Imperial => _axesModulesList.Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_Inch_")).Select(x => x.CallingMethod!).ToList(),
                _ => _axesModulesList.Where(x => x.CallingMethod != null).Select(x => x.CallingMethod!).ToList()
            };

            // Add "Select Axis" as the first item if no axis is stored
            if (!AxisStored)
            {
                filteredAxes.Insert(0, "Select Axis");
            }

            AxesList = [.. filteredAxes];

            // Update selected axis if current selection is no longer valid
            if (!AxesList.Contains(SelectedAxisValue!))
            {
                SelectedAxisValue = AxisStored ? AxesList.FirstOrDefault() : "Select Axis";
                _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == SelectedAxisValue);
            }

            return Task.CompletedTask;
        }

        /* Create a union module from all "Union" parts. This assumes that the user will treat a collection of parts as a single part.
        The CreateDifferenceModuleAsync method will handle a single part, and then subtract multiple parts from it.
        Cubes and cylinders are handled separately but combined in unions and differences. This is because the Db properties are different.
        Manual editing within OpenSCAD is likely needed to place cylinders within cubes to the user's satisfaction.
        The custom object builder is designed to create the parts, and then union or subtract them as needed.
        Once all the parts are created, the user can then manually edit the OpenSCAD file to position them as needed.
        Union should be called first, then Difference. OpenSCAD logic is difference() { union() { A,B,C }, D, E, F } */
        public async Task CreateUnionModuleAsync()
        {
            // Get all objects marked as "Union"
            var objects = SolidDimensions
                .Where(o => o.OperationType == "Union") // Filter for Union operation type
                .OrderBy(c => c.SolidType.ToLower() == "cube" ? 0 : c.SolidType.ToLower() == "cylinder" ? 1 : c.SolidType.ToLower() == "surface" ? 2 : 3)  // Cube (0), Cylinder (1), Surface (2), Minkowski (3)
                .ThenBy(c => c.Volume_IN3)  // Ascending volume within each type
                .ToList();
            var addMethods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

            var solidType = "Object";
            var unionModule = new ModuleDimensions
            {
                ModuleType = "Union",
                ObjectName = Name,
                ObjectDescription = Description,
                SolidType = solidType,
                OSCADMethod = ToUnionModule(addMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                CreatedAt = DateTime.UtcNow,
            };
            // Build call method and store in Db
            unionModule.Name = ExtractModuleCallMethod(unionModule.OSCADMethod).ToLower();
            var moduleId = await unionModule.UpsertAsync(DbConnection!);
            await GetDimensionsPartAsync(); // Refresh the datagrids
            // Update all solid objects with the new ModuleDimensionsId
            var solidIds = objects.Select(o => o.Id);
            await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
            await PartsToScadFilesAsync();  // Only update parts file
        }

        public async Task CreateDifferenceModuleAsync()
        {
            await CreateUnionModuleAsync(); // Update union module before applying differences
            // Get all objects marked as "Difference"
            var objects = SolidDimensions
                .Where(o => o.OperationType == "Difference")
                .OrderBy(c => c.SolidType.ToLower() == "cube" ? 0 : c.SolidType.ToLower() == "cylinder" ? 1 : c.SolidType.ToLower() == "surface" ? 2 : 3)  // Cube (0), Cylinder (1), Surface (2), Minkowski (3)
                .ThenBy(c => c.Volume_IN3)  // Ascending volume within each type
                .ToList();
            ModuleDimensions? baseObj;
            baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.ObjectName == Name);

            if (baseObj != null)
            {
                var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                var subtractMethods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                var solidType = "Object";
                var differenceModule = new ModuleDimensions
                {
                    ModuleType = "Difference",
                    ObjectName = Name,
                    ObjectDescription = Description,
                    SolidType = solidType,
                    OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                // get calling method for differenceModule
                differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                var moduleId = await differenceModule.UpsertAsync(DbConnection!);
                await GetDimensionsPartAsync(); // Refresh the datagrids
                // Update all solid objects with the new ModuleDimensionsId
                var solidIds = objects.Select(o => o.Id);
                await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                await PartsToScadFilesAsync();  // Only update parts file
            }
            else
                Console.WriteLine("No Unions available as base object");
        }

        public async Task CreateIntersectionModuleAsync()
        {
            // Get all objects marked as "Intersection"
            var objects = SolidDimensions.Where(o => o.OperationType == "Intersection").ToList();
            ModuleDimensions? baseObj;
            baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.ObjectName == Name);

            if (baseObj != null)
            {
                var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                var intersectMethods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                var solidType = "Object";
                var intersectionModule = new ModuleDimensions
                {
                    ModuleType = "Intersection",
                    ObjectName = Name,
                    ObjectDescription = Description,
                    SolidType = solidType,
                    OSCADMethod = ToIntersectionModule(baseCallMethod, intersectMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                // get calling method for intersectionModule
                intersectionModule.Name = ExtractModuleCallMethod(intersectionModule.OSCADMethod).ToLower();
                var moduleId = await intersectionModule.UpsertAsync(DbConnection!);
                await GetDimensionsPartAsync(); // Refresh the datagrids
                // Update all solid objects with the new ModuleDimensionsId
                var solidIds = objects.Select(o => o.Id);
                await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                await PartsToScadFilesAsync();  // Only update parts file
            }
            else
                Console.WriteLine("No Unions available as base object");
        }

        private static string ExtractModuleCallMethod(string moduleDefinition)
        {
            if (string.IsNullOrWhiteSpace(moduleDefinition))
                return string.Empty;

            // Remove "module " prefix
            var withoutModuleKeyword = moduleDefinition.Trim();
            if (withoutModuleKeyword.StartsWith("module ", StringComparison.OrdinalIgnoreCase))
            {
                withoutModuleKeyword = withoutModuleKeyword["module ".Length..].Trim();
            }

            // Find the closing parenthesis after the module name
            var closingParenIndex = withoutModuleKeyword.IndexOf(')');
            if (closingParenIndex == -1)
                return string.Empty;

            // Extract everything up to and including the closing parenthesis
            var callMethod = withoutModuleKeyword[..(closingParenIndex + 1)];

            // Add semicolon if not already present
            if (!callMethod.EndsWith(';'))
            {
                callMethod += ';';
            }

            return callMethod.Trim();
        }

        private Task UpdateScrewRadiusFromSelection()
        {
            if (!IsCylinderSelected)
                return Task.CompletedTask;
            var screwData = SelectedScrewSize;

            double radiusValue = SelectedScrewProperty switch
            {
                "Screw Thread" => screwData!.ScrewRadius,
                "Screw Head" => screwData!.ScrewHeadRadius,
                "Threaded Insert" => screwData!.ThreadedInsertRadius,
                "Clearance Hole" => screwData!.ClearanceHoleRadius,
                _ => 0
            };
       
            RadiusMM = SelectedUnitValue == UnitSystem.Imperial ? Math.Round(MillimeterToInches(radiusValue), _decimalPlaces) : radiusValue;
            return Task.CompletedTask;
        }

        // Add this method to update dimensions when server rack is selected
        private Task UpdateServerRackDimensionsFromSelection()
        {
            if (!IsCubeSelected && !IsRoundCubeSelected)
                return Task.CompletedTask;

            // Update width if a width type is selected
            if (!string.IsNullOrEmpty(SelectedServerRackWidthType))
            {
                var rackData = ServerRackDimensions.GetAll().FirstOrDefault(); // Independent of Rack Height
                if (rackData == null)
                    return Task.CompletedTask;
                double widthValue = SelectedServerRackWidthType switch
                {
                    "Inner Mount" => SelectedUnitValue == UnitSystem.Metric
                        ? rackData.InnerWidthMm
                        : rackData.InnerWidthInches,
                    "Outer Mount" => SelectedUnitValue == UnitSystem.Metric
                        ? rackData.OuterWidthMm
                        : rackData.OuterWidthInches,
                    _ => 0
                };

                WidthMM = Math.Round(widthValue, _decimalPlaces);
            }

            // Update height if server rack is selected
            if (SelectedServerRack != null)
            {
                var rackData = SelectedServerRack;

                // Update Height based on unit system
                HeightMM = SelectedUnitValue == UnitSystem.Metric
                    ? rackData.HeightMm
                    : Math.Round(rackData.HeightInches, _decimalPlaces);
            }

            return Task.CompletedTask;
        }

        private Task UpdateViewButtons()
        {
            var mDim = ModuleDimensions.Where(o => o.SolidType == "Object");
            // If there are any parts to save in the file, or modules to save in the object file that are cube objects
            switch (mDim.Any())
            {
                case true:
                    SaveFileButton = true;
                    // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                    switch (SolidDimensions.Where(o => o.OperationType == "Difference").Any())
                    {
                        case true:
                            DifferenceButton = true;
                            break;
                        case false:
                            DifferenceButton = false;
                            break;
                    }
                    break;
                case false:
                    SaveFileButton = false;
                    break;
            }
            // If there is at least one add item in the SolidDimensions table
            switch (SolidDimensions.Where(o => o.OperationType == "Union").Any() || SolidDimensions.Where(o => o.OperationType == "Union").Any())
            {
                case true:
                    UnionButton = true;
                    break;
                case false:
                    UnionButton = false;
                    break;
            }
            // If there is at least one add item in the SolidDimensions table
            switch (SolidDimensions.Where(o => o.OperationType == "Intersection").Any() || SolidDimensions.Where(o => o.OperationType == "Intersection").Any())
            {
                case true:
                    IntersectionButton = true;
                    break;
                case false:
                    IntersectionButton = false;
                    break;
            }
            return Task.CompletedTask;
        }

        public Task LoadPngDimensions(string filePath)
        {
            var (width, height, depth, _filePath) = ImageHelper.GetPngDimensions(filePath, AutoSmoothFile);
            // Image width and height are equivalent to width and length axis by default, so have to rotate for user to interpret width and height as the same context
            XRotate = 90;
            ZRotate = 90;

            if (_selectedUnit == UnitSystem.Imperial)
            {
                depth = Math.Round(MillimeterToInches(depth), _decimalPlaces);
                width = Math.Round(MillimeterToInches(width), _decimalPlaces);
                height = Math.Round(MillimeterToInches(height), _decimalPlaces);
            }

            // Set textboxes for UI
            LengthMM = depth * SurfaceScaleX;
            WidthMM = width * SurfaceScaleY;
            HeightMM = height * SurfaceScaleZ;
            SurfaceFilePath = _filePath; // return the file path if it has changed in smoothing

            // OpenSCAD interprets depth of the image differently based on invert being true or not, to align with 0,0,0 axes
            if (_surfaceInvert)
                XOffsetMM = LengthMM;  // 100mm OpenSCAD default

            return Task.CompletedTask;
        }

        // Method to show OSCAD methods
        public Task ShowOSCADMethods(ModuleDimensions module)
        {
            var solids = SolidDimensions.Where(s => s.ModuleDimensionsId == module.Id).ToList();
            if (solids.Any())
            {
                ModalTitle = $"OSCAD Methods for {module.Name}";
                ModalContent = string.Join("\n\n", solids.Select(s => s.OSCADMethod));
                IsModalOpen = true;
            }
            return Task.CompletedTask;
        }
        // Add this method to the ScadObjectViewModel class

        /// <summary>
        /// Updates the module color in the database and regenerates SCAD files with the new color.
        /// </summary>
        /// <param name="moduleId">The ID of the module to update</param>
        /// <param name="color">The selected OpenScad color</param>
        public async Task UpdateModuleColorAsync(int moduleId, OpenScadColor color)
        {
            try
            {
                // Find the module in the collection
                var module = ModuleDimensions.FirstOrDefault(m => m.Id == moduleId);
                if (module == null) return;

                // Update the module's color property
                module.ModuleColor = color.ToString();

                // Update in database
                await module.UpdateColorAsync(DbConnection!, color.ToString());

                // Regenerate the SCAD files with the new color
                await RegenerateModuleWithColorAsync(module, color);

                // Refresh the parts file
                await PartsToScadFilesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating module color: {ex.Message}");
            }
        }

        /// <summary>
        /// Regenerates the module's OSCAD method with the specified color.
        /// </summary>
        private async Task RegenerateModuleWithColorAsync(ModuleDimensions module, OpenScadColor color)
        {
            // Get all solids for this module
            var solids = SolidDimensions.Where(s => s.ModuleDimensionsId == module.Id).ToList();

            if (!solids.Any()) return;

            // Regenerate the module with color wrapper
            var moduleCallMethods = solids.Select(s => ExtractModuleCallMethod(s.OSCADMethod)).ToList();

            // Apply color based on module type
            string updatedOSCADMethod = module.ModuleType switch
            {
                "Union" => ToUnionModuleWithColor(moduleCallMethods, module.ObjectName, module.ObjectDescription ?? string.Empty, module.SolidType, _isPreRendered, color),

                "Difference" => ToDifferenceModuleWithColor(ExtractModuleCallMethod(ModuleDimensions.FirstOrDefault(m => m.ModuleType == "Union" && m.ObjectName == module.ObjectName)?.OSCADMethod ?? string.Empty),
                                                            moduleCallMethods, module.ObjectName, module.ObjectDescription ?? string.Empty, module.SolidType, _isPreRendered, color),

                "Intersection" => ToIntersectionModuleWithColor(ExtractModuleCallMethod(ModuleDimensions.FirstOrDefault(m => m.ModuleType == "Union" && m.ObjectName == module.ObjectName)?.OSCADMethod ?? string.Empty),
                                                                moduleCallMethods, module.ObjectName, module.ObjectDescription ?? string.Empty,
                                                                module.SolidType, _isPreRendered, color),
                _ => module.OSCADMethod
            };

            // Update the module's OSCAD method
            module.OSCADMethod = updatedOSCADMethod.ToLower();
            await module.UpsertAsync(DbConnection!);
        }

        /*** Public Variables ***/
        public bool IsNoneOperation
        {
            get => _isNoneOperation;
            set
            {
                this.RaiseAndSetIfChanged(ref _isNoneOperation, value);
                if (value)
                {
                    _isUnionOperation = false;
                    _isIntersectionOperation = false;
                    _isDifferenceOperation = false;
                    this.RaisePropertyChanged(nameof(IsUnionOperation));
                    this.RaisePropertyChanged(nameof(IsIntersectionOperation));
                    this.RaisePropertyChanged(nameof(IsDifferenceOperation));
                }
            }
        }
        public bool IsUnionOperation
        {
            get => _isUnionOperation;
            set
            {
                this.RaiseAndSetIfChanged(ref _isUnionOperation, value);
                if (value)
                {
                    _isNoneOperation = false;
                    _isDifferenceOperation = false;
                    _isIntersectionOperation = false;
                    this.RaisePropertyChanged(nameof(IsNoneOperation));
                    this.RaisePropertyChanged(nameof(IsIntersectionOperation));
                    this.RaisePropertyChanged(nameof(IsDifferenceOperation));
                }
            }
        }
        public bool IsDifferenceOperation
        {
            get => _isDifferenceOperation;
            set
            {
                this.RaiseAndSetIfChanged(ref _isDifferenceOperation, value);
                if (value)
                {
                    _isNoneOperation = false;
                    _isUnionOperation = false;
                    _isIntersectionOperation = false;
                    this.RaisePropertyChanged(nameof(IsNoneOperation));
                    this.RaisePropertyChanged(nameof(IsUnionOperation));
                    this.RaisePropertyChanged(nameof(IsIntersectionOperation));
                }
            }
        }
        public bool IsIntersectionOperation
        {
            get => _isIntersectionOperation;
            set
            {
                this.RaiseAndSetIfChanged(ref _isIntersectionOperation, value);
                if (value)
                {
                    _isNoneOperation = false;
                    _isUnionOperation = false;
                    _isDifferenceOperation = false;
                    this.RaisePropertyChanged(nameof(IsNoneOperation));
                    this.RaisePropertyChanged(nameof(IsUnionOperation));
                    this.RaisePropertyChanged(nameof(IsDifferenceOperation));
                }
            }
        }
        // Helper property to get the selected operation as a string
        public string SelectedBooleanOperation
        {
            get
            {
                if (IsUnionOperation) return "Union";
                if (IsDifferenceOperation) return "Difference";
                if (IsIntersectionOperation) return "Intersection";
                return "None";
            }
        }

        public string SurfaceFilePath { get => _surfaceFilePath; set => this.RaiseAndSetIfChanged(ref _surfaceFilePath, value); }
        public bool SurfaceCenter { get => _surfaceCenter; set => this.RaiseAndSetIfChanged(ref _surfaceCenter, value); }
        public bool AutoSmoothFile { get => _autoSmoothFile; set => this.RaiseAndSetIfChanged(ref _autoSmoothFile, value); }
        public int SurfaceConvexity { get => _surfaceConvexity; set => this.RaiseAndSetIfChanged(ref _surfaceConvexity, value); }
        public bool SurfaceInvert
        {
            get => _surfaceInvert;
            set
            {
                this.RaiseAndSetIfChanged(ref _surfaceInvert, value);

                if (_isSurfaceSelected)
                {
                    if (_surfaceInvert)
                    {
                        XOffsetMM = LengthMM;  // 100mm OpenSCAD default
                    }
                    else if (XOffsetMM == LengthMM)  // If user is toggling Invert Surface or not
                    {
                        XOffsetMM = 0.0;
                    }
                }
            }
        }
        public double SurfaceScaleX 
        { 
            get => _surfaceScaleX;
            set
            {
                if (value > 0.0)
                {
                    this.RaiseAndSetIfChanged(ref _surfaceScaleX, value);
                    if (_isSurfaceSelected)
                    {
                        LengthMM = _lengthMM * _surfaceScaleX;
                        if (_surfaceInvert)
                        {
                            XOffsetMM = LengthMM;  // 100mm OpenSCAD default
                        }
                    }
                }
            }
        }
        public double SurfaceScaleY 
        {
            get => _surfaceScaleY; 
            set
            {
                if (value > 0.0)
                {
                    this.RaiseAndSetIfChanged(ref _surfaceScaleY, value);
                    if (_isSurfaceSelected)
                        WidthMM = _widthMM * _surfaceScaleY;
                }
            }
        }
        public double SurfaceScaleZ
        {
            get => _surfaceScaleZ;
            set
            {
                if (value > 0.0)
                {
                    this.RaiseAndSetIfChanged(ref _surfaceScaleZ, value);
                    if (_isSurfaceSelected)
                        HeightMM = _heightMM * _surfaceScaleZ;
                }
            }
        }
        public double AxisXPositionMM { get => _axisXPositionMM; set { this.RaiseAndSetIfChanged(ref _axisXPositionMM, value); if (AxisStored) _ = UpdateAxisTranslateAsync(); } }
        public double AxisYPositionMM { get => _axisYPositionMM; set { this.RaiseAndSetIfChanged(ref _axisYPositionMM, value); if (AxisStored) _ = UpdateAxisTranslateAsync(); } }
        public double AxisZPositionMM { get => _axisZPositionMM; set { this.RaiseAndSetIfChanged(ref _axisZPositionMM, value); if (AxisStored) _ = UpdateAxisTranslateAsync(); } }
        public bool UnitHasChanged { get => _unitHasChanged; set => this.RaiseAndSetIfChanged(ref _unitHasChanged, value); }
        public bool IsMetric { get => _isMetric; set => this.RaiseAndSetIfChanged(ref _isMetric, value); }
        public bool IsImperial { get => _isImperial; set => this.RaiseAndSetIfChanged(ref _isImperial, value); }
        public bool IsAxisMetric { get => _isAxisMetric; set => this.RaiseAndSetIfChanged(ref _isAxisMetric, value); }
        public bool IsAxisImperial { get => _isAxisImperial; set => this.RaiseAndSetIfChanged(ref _isAxisImperial, value); }
        public bool IsPreRendered { get => _isPreRendered; set => this.RaiseAndSetIfChanged(ref _isPreRendered, value); }
        public bool RemoveAxis { get => _removeAxis; set => this.RaiseAndSetIfChanged(ref _removeAxis, value); }
        public bool ExportToStl
        {
            get => _exportToStl;
            set
            {
                this.RaiseAndSetIfChanged(ref _exportToStl, value);
                if (_exportToStl)
                    _ = ObjectToScadFilesAsync();
            }
        }
        public bool AxesSelectEnabled
        {
            get => _axesSelectEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _axesSelectEnabled, value);
                if (_axesSelectEnabled)
                {
                    SelectedAxisValue = "Select Axis"; // Reset selection when enabled
                    _selectedAxis = null; // Clear selected axis
                }
            }
        }
        public bool AxisStored { get => _axisStored; set => this.RaiseAndSetIfChanged(ref _axisStored, value); }
        public bool AppendObject { get => _appendObject; set => this.RaiseAndSetIfChanged(ref _appendObject, value); }
        public bool UnionButton { get => _unionButton; set => this.RaiseAndSetIfChanged(ref _unionButton, value); }
        public bool IntersectionButton { get => _intersectionButton; set => this.RaiseAndSetIfChanged(ref _intersectionButton, value); }
        public bool DifferenceButton { get => _differenceButton; set => this.RaiseAndSetIfChanged(ref _differenceButton, value); }
        public bool SaveFileButton { get => _saveFileButton; set => this.RaiseAndSetIfChanged(ref _saveFileButton, value); }
        public List<FilamentType> FilamentTypes { get; }
        public List<UnitSystem> UnitSystemValues { get; }
        public List<string>? AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }
        public List<OperationType> OperationTypes { get; }
        public OperationType SelectedOperationType { get => _selectedOperationType; set => this.RaiseAndSetIfChanged(ref _selectedOperationType, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsUnions { get => _moduleDimensionsUnions; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsUnions, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsIntersections { get => _moduleDimensionsIntersections; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsIntersections, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsDifferences { get => _moduleDimensionsDifferences; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsDifferences, value); }
        public ObservableCollection<SolidDimensions> SolidDimensions { get => _solidDimensions; set => this.RaiseAndSetIfChanged(ref _solidDimensions, value); }
        public SqliteConnection? DbConnection { get => _dbConnection; set => this.RaiseAndSetIfChanged(ref _dbConnection, value); }
        public string Name
        {
            get => _name;
            set
            {
                System.Text.RegularExpressions.Regex.Replace(value.Trim().ToLower(), @"[^a-zA-Z0-9_]", "_"); // Sanitize name to be suitable for OpenSCAD module naming
                this.RaiseAndSetIfChanged(ref _name, value);
            }
        }
        public string ObjectAxisDisplay { get => _objectAxisDisplay; set => this.RaiseAndSetIfChanged(ref _objectAxisDisplay, value); }
        public string ObjectAxisUnitDisplay { get => _objectAxisUnitDisplay; set => this.RaiseAndSetIfChanged(ref _objectAxisUnitDisplay, value); }
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
        public double LengthMM { get => _lengthMM; set => this.RaiseAndSetIfChanged(ref _lengthMM, value); }
        public double WidthMM { get => _widthMM; set => this.RaiseAndSetIfChanged(ref _widthMM, value); }
        public double HeightMM { get => _heightMM; set => this.RaiseAndSetIfChanged(ref _heightMM, value); }
        public double ThicknessMM { get => _thicknessMM; set => this.RaiseAndSetIfChanged(ref _thicknessMM, value); }
        public FilamentType SelectedFilament { get => _selectedFilament; set => this.RaiseAndSetIfChanged(ref _selectedFilament, value); }
        public double XOffsetMM { get => _xOffsetMM; set => this.RaiseAndSetIfChanged(ref _xOffsetMM, value); }
        public double YOffsetMM { get => _yOffsetMM; set => this.RaiseAndSetIfChanged(ref _yOffsetMM, value); }
        public double ZOffsetMM { get => _zOffsetMM; set => this.RaiseAndSetIfChanged(ref _zOffsetMM, value); }
        public double XRotate { get => _xRotate; set => this.RaiseAndSetIfChanged(ref _xRotate, value); }
        public double YRotate { get => _yRotate; set => this.RaiseAndSetIfChanged(ref _yRotate, value); }
        public double ZRotate { get => _zRotate; set => this.RaiseAndSetIfChanged(ref _zRotate, value); }
        public double RadiusMM { get => _radiusMM; set => this.RaiseAndSetIfChanged(ref _radiusMM, value); }
        public double Radius1MM { get => _radius1MM; set => this.RaiseAndSetIfChanged(ref _radius1MM, value); }
        public double Radius2MM { get => _radius2MM; set => this.RaiseAndSetIfChanged(ref _radius2MM, value); }
        public double CylinderHeightMM { get => _cylinderHeightMM; set => this.RaiseAndSetIfChanged(ref _cylinderHeightMM, value); }
        public bool IsModalOpen { get => _isModalOpen; set => this.RaiseAndSetIfChanged(ref _isModalOpen, value); }
        public string ModalTitle { get => _modalTitle; set => this.RaiseAndSetIfChanged(ref _modalTitle, value); }
        public string ModalContent { get => _modalContent; set => this.RaiseAndSetIfChanged(ref _modalContent, value); }
        public bool IsCubeSelected
        {
            get => _isCubeSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCubeSelected, value);
                if (_isCubeSelected)
                {
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                   _ = UpdateServerRackDimensionsFromSelection(); // For cubes and round cubes, update server rack dimensions
                   _ = UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }
        public bool IsRoundCubeSelected
        {
            get => _isRoundCubeSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRoundCubeSelected, value);
                if (_isRoundCubeSelected)
                {
                    _isCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    _ = UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }
        public bool IsCylinderSelected
        {
            get => _isCylinderSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCylinderSelected, value);
                if (_isCylinderSelected)
                {
                    _isRoundCubeSelected = false;
                    _isCubeSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    _ = UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }
        public bool IsSurfaceSelected
        {
            get => _isSurfaceSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSurfaceSelected, value);
                if (_isSurfaceSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isRoundSurfaceSelected = false;
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    _ = UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }
        public bool IsRoundSurfaceSelected
        {
            get => _isRoundSurfaceSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRoundSurfaceSelected, value);
                if (_isRoundSurfaceSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    _ = UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }
        public ScrewSize? SelectedScrewSize
        {
            get => _selectedScrewSize;
            set
            {
                if (value == null) return;
                this.RaiseAndSetIfChanged(ref _selectedScrewSize, value);
                SelectedScrewProperty ??= ScrewProperties!.FirstOrDefault(s => s == "Screw Thread")!;
                if (_isCylinderSelected)
                {
                    _ = UpdateScrewRadiusFromSelection();
                }
            }
        }
        public string SelectedScrewProperty
        {
            get => _selectedScrewProperty;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                this.RaiseAndSetIfChanged(ref _selectedScrewProperty, value);
                if (_isCylinderSelected)
                {
                    _ = UpdateScrewRadiusFromSelection();
                }
            }
        }
        public List<ScrewSize>? ScrewSizes { get => _screwSizes; set => this.RaiseAndSetIfChanged(ref _screwSizes, value); }
        public List<string> ScrewProperties { get; } =
        [
            "Screw Thread",
            "Screw Head",
            "Threaded Insert",
            "Clearance Hole",
        ];
        public ServerRack? SelectedServerRack
        {
            get => _selectedServerRack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedServerRack, value);
                if (_isCubeSelected || _isRoundCubeSelected)
                {
                    _ = UpdateServerRackDimensionsFromSelection();
                }
            }
        }
        public string SelectedServerRackWidthType
        {
            get => _selectedServerRackWidthType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedServerRackWidthType, value);
                if (_isCubeSelected || _isRoundCubeSelected)
                {
                    _ = UpdateServerRackDimensionsFromSelection();
                }
            }
        }
        public List<ServerRack>? ServerRackSizes { get => _serverRackSizes; set => this.RaiseAndSetIfChanged(ref _serverRackSizes, value); }
        public List<string> ServerRackWidthTypes { get; } =
        [
            "Inner Mount",
            "Outer Mount",
        ];
        public ObservableCollection<ModuleDimensions> ModuleDimensions
        {
            get => _moduleDimensions;
            set
            {
                this.RaiseAndSetIfChanged(ref _moduleDimensions, value);
                ModuleDimensionsUnions = new ObservableCollection<ModuleDimensions>(_moduleDimensions.Where(m => m.ModuleType == "Union"));
                ModuleDimensionsDifferences = new ObservableCollection<ModuleDimensions>(_moduleDimensions.Where(m => m.ModuleType == "Difference"));
                ModuleDimensionsIntersections = new ObservableCollection<ModuleDimensions>(_moduleDimensions.Where(m => m.ModuleType == "Intersection"));
                this.RaisePropertyChanged(nameof(AllModuleDimensions)); // Add this line
            }
        }
        public UnitSystem SelectedUnitValue
        {
            get => _selectedUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedUnit, value);
                UnitHasChanged = true;
                _ = ConvertInputs(_decimalPlaces);
            }
        }
        public UnitSystem SelectedAxisUnitValue
        {
            get => _selectedAxisUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAxisUnit, value);
                IsAxisImperial = _selectedAxisUnit != UnitSystem.Metric;
                IsAxisMetric = _selectedAxisUnit == UnitSystem.Metric;
                _ = GetAxesList(); // Refresh axes list when axis unit changes
            }
        }
        public string? SelectedAxisValue
        {
            get => _selectedAxisValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAxisValue, value);
                _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == value);
                AxisXPositionMM = _selectedAxis != null ? _selectedAxis.MinX : 0;
                AxisYPositionMM = _selectedAxis != null ? _selectedAxis.MinY : 0;
                AxisZPositionMM = _selectedAxis != null ? _selectedAxis.MinZ : 0;
                if (AxisStored)
                    _ = UpdateAxisTranslateAsync();
                else
                    _ = CreateAxisAsync();
            }
        }
        public List<string> SolidTypes { get; } = ["Cube", "Round Cube", "Cylinder", "Surface"];
        public string SelectedSolidType
        {
            get => _selectedShapeType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedShapeType, value);

                // Update the individual shape selection properties based on the combo box selection
                switch (value)
                {
                    case "Cube":
                        IsCubeSelected = true;
                        break;
                    case "Round Cube":
                        IsRoundCubeSelected = true;
                        break;
                    case "Cylinder":
                        IsCylinderSelected = true;
                        break;
                    case "Surface":
                        IsSurfaceSelected = true;
                        break;
                }
            }
        }
    }
}
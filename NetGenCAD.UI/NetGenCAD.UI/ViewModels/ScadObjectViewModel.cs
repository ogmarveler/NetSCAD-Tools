using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Core.Interfaces;
using NetGenCAD.Core.Material;
using NetGenCAD.Core.Measurements;
using NetGenCAD.Core.Models;
using NetGenCAD.Core.Primitives;
using NetGenCAD.Core.Utility;
using NetGenCAD.Designer.Repositories;
using NetGenCAD.Designer.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
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
using static NetGenCAD.Core.Measurements.Colors;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;
using static NetGenCAD.Core.Utility.WrapInModule;

namespace NetGenCAD.UI.ViewModels
{
    public class ScadObjectViewModel : ValidatableBase
    {
        private ObservableCollection<SolidDimensions> _solidDimensions;
        private ObservableCollection<GeneratedModule> _axesModulesList;
        private ObservableCollection<ModuleDimensions> _moduleDimensions;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsDifferences;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsUnions;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsIntersections;
        private ObservableCollection<ModuleDimensions> _layeredModuleDimensions = new();
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
        private double _AxisXPositionMM = 0;
        private double _AxisYPositionMM = 0;
        private double _AxisZPositionMM = 0;
        private double _axisXPositionMM = 0;
        private double _axisYPositionMM = 0;
        private double _axisZPositionMM = 0;
        private double _xRotate = 0;
        private double _yRotate = 0;
        private double _zRotate = 0;
        private double _xMirror = 0;
        private double _yMirror = 0;
        private double _zMirror = 0;
        private double _XMirror = 0;
        private double _YMirror = 0;
        private double _ZMirror = 0;
        private string _originalMirrorCall = string.Empty;
        private string _selectedShapeType = "Cube";
        private bool _removeAxis = false;
        private bool _isPreRendered = false;
        private bool _originalRemoveAxis = false;
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
        private OpenScadColor[] _openScadColors = Enum.GetValues<OpenScadColor>();
        private OpenScadColor _selectedOpenScadColor = OpenScadColor.Silver;
        private static readonly System.Text.RegularExpressions.Regex NameSanitizerRegex = new System.Text.RegularExpressions.Regex(@"[^a-z0-9]+", System.Text.RegularExpressions.RegexOptions.Compiled);
        private int _layerIntValue = 0;
        private double _alphaIntValue = 1;
        private bool _isSphereSelected = false;
        private bool _isRoundCylinderSelected = false;


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
            ClearObject();
            GetAxesList();
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

        /**** Dimensions DataGrids ****/
        public async void GetDimensionsParts()
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
            IsRoundCylinderSelected = _isRoundCylinderSelected;
            IsSphereSelected = _isSphereSelected;
            IsSurfaceSelected = _isSurfaceSelected;
        }

        // Clear all input fields
        public void ClearInputs()
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
            LayerIntValue = 0;
            AlphaIntValue = 1;
            SelectedOpenSCADColor = OpenScadColor.Silver;
        }

        // Clear all object fields
        public void ClearObject()
        {
            Name = string.Empty;
            Description = string.Empty;
            ClearInputs(); // Clear input fields
            ModuleDimensions = [];
            SolidDimensions = [];
            SelectedOperationType = OperationType.Union;
            SelectedUnitValue = UnitSystem.Metric;
            AppendObject = false;
            DifferenceButton = false;
            UnionButton = false;
            SaveFileButton = false;
            IsCubeSelected = false;
            IsRoundCubeSelected = false;
            IsCylinderSelected = false;
            IsSurfaceSelected = false;
            IsRoundSurfaceSelected = false;
            IsSphereSelected = false;
            IsRoundCylinderSelected = false;
            IsPreRendered = false;
            ScrewSizes = _screwSizes;
            ServerRackSizes = _serverRackSizes;
            RemoveAxis = false;
            _originalRemoveAxis = false;
            XMirror = 0;
            YMirror = 0;
            ZMirror = 0;
            GetDimensionsParts(); // Refresh the DataGrid
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
                case "Round Cylinder":
                    IsRoundCylinderSelected = true;
                    break;
                case "Sphere":
                    IsSphereSelected = true;
                    break;
                case "Surface":
                    IsSurfaceSelected = true;
                    break;
                default: return 0;
            }
            int? id = null;  // Object for returning the row id

            if (_axisId is null) // New axis being applied
                CreateAxis(); // Create or get AxisDimensions and return its Id

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
                AxisDimensionsId = _axisId,
                SurfaceCenter = SurfaceCenter ? 1 : 0,
                SurfaceInvert = SurfaceInvert ? 1 : 0,
                SurfaceFilePath = SurfaceFilePath,
                ModuleColor = SelectedOpenSCADColor.ToString(),
                Layer = LayerIntValue,
                Alpha = AlphaIntValue,
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
                case OperationType.Union: CreateDifferenceModule(); break; // Update Union modules for real-time updates through difference module
                case OperationType.Difference: CreateDifferenceModule(); break; // Update Difference modules for real-time updates
                case OperationType.Intersection: CreateIntersectionModule(); break; // Update Intersection modules for real-time updates
            }

            ClearInputs(); // After new part added, make sure that description is cleared out
            GetDimensionsParts(); // Refresh datagrids
            return id ?? 0;
        }

        public async void UpdateAxisTranslate()
        {
            if (!AxisStored) return;  // No axis has been applied yet

            /** Need to find the original scad statement before updating variables **/
            if (_originalRemoveAxis)
                _originalAxisCall = $"// translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}";
            else
                _originalAxisCall = $"translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}";
            try
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

                UpdateFIle.ChangeContentBlockFile(oldCodeBlock: _originalAxisCall, newCodeBlock: wrappedAxisCall, filePath: filePath);  // Append updated axis call to object.scad file
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating axis translate in object.scad file: " + ex.Message);
            }
        }

        public async void CreateAxis()
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
                GetDimensionsParts();
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
                    var rotated = await GetRotate(roundedCube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = GetTranslate(rotated, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM).Result;
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
                else if (IsCubeSelected)
                {
                    // Generate a cube - OpenSCAD code
                    var cubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim!.Length_MM }, { "size_y", oDim!.Width_MM }, { "size_z", oDim!.Height_MM },
                    };
                    var cube = OScad3D.Cube.ToScadObject(cubeParams);

                    var rotated = await GetRotate(cube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                    var translate = GetTranslate(rotated, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM).Result;
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
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
                    var rotated = await GetRotate(surface, oDim!.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = GetTranslate(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM).Result;
                    return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
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
                    var rotated = await GetRotate(roundSurface, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                    var translate = GetTranslate(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM).Result;
                    return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
            }
            else if (IsCylinderSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
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
                    { "resolution", 360 }
                };
                var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
                var rotated = await GetRotate(cylinder, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = GetTranslate(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM).Result;
                return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }
            else if (IsRoundCylinderSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    oDim!.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), _decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), _decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), _decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), _decimalPlaces);
                    oDim.Round_r_MM = Math.Round(InchesToMillimeter(oDim.Round_r_MM), _decimalPlaces);
                    oDim.Round_h_MM = Math.Round(InchesToMillimeter(oDim.Round_h_MM), _decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), _decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), _decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), _decimalPlaces);
                }

                // Based on radius (if applicable)
                oDim!.Round_r_MM = Math.Round(oDim.Round_r_MM > 0 ? oDim.Round_r_MM : RoundFromWidth(oDim.CylinderHeight_MM), _decimalPlaces);
                oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), _decimalPlaces);

                var roundedCylParams = new Dictionary<string, object>
                {
                    { "r", oDim!.Radius_MM },
                    { "r1", oDim!.Radius1_MM },
                    { "r2", oDim!.Radius2_MM },
                    { "h", oDim!.CylinderHeight_MM },
                    { "round_r", oDim.Round_r_MM },
                    { "round_h", oDim.Round_h_MM },
                    { "resolution", oDim.Resolution }
                };
                var roundedCylinder = OScad3D.RoundedCylinder.ToScadObject(roundedCylParams);
                var rotated = await GetRotate(roundedCylinder, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = GetTranslate(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM).Result;
                return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }
            else if (IsSphereSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert sphere dimensions to metric for OpenSCAD
                    oDim!.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), _decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), _decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), _decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), _decimalPlaces);
                }

                var sphereParams = new Dictionary<string, object>
                {
                    { "r", oDim!.Radius_MM },
                    { "resolution", 360 }
                };
                var sphere = OScad3D.Sphere.ToScadObject(sphereParams);
                var rotated = await GetRotate(sphere, oDim.XRotate, oDim.YRotate, oDim.ZRotate);  // Add rotation
                var translate = GetTranslate(rotated, oDim!.XOffset_MM, oDim!.YOffset_MM, oDim!.ZOffset_MM).Result;
                return ToModule(translate.OSCADMethod, oDim!.Name, oDim!.Description!, oDim!.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }

            return string.Empty;
        }

        // Make the object's position changeable if this function is called separately. Translate of a translate.
        // Use case for shifting an entire set of child objects as part of an IScadObject
        public Task<IScadObject> GetTranslate(IScadObject scadObject, double XOffset_MM, double YOffset_MM, double ZOffset_MM)
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
                            ZOffset_MM += oDim.Round_h_MM;
                            break;
                        case OperationType.Difference:
                            XOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            YOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            ZOffset_MM += -oDim.Round_h_MM + oDim.Thickness_MM;
                            break;
                        case OperationType.Intersection:
                            XOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            YOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                            ZOffset_MM += oDim.Round_h_MM + oDim.Thickness_MM;
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
            else if (IsCylinderSelected || IsSphereSelected || IsRoundCylinderSelected)
            {
                var oDim = new SolidDimensions
                {
                    Radius_MM = RadiusMM,
                    Radius1_MM = Radius1MM,
                    Radius2_MM = Radius2MM,
                    CylinderHeight_MM = CylinderHeightMM,
                };

                if (_selectedUnit == UnitSystem.Imperial)
                {
                    oDim.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), _decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), _decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), _decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), _decimalPlaces);

                    XOffset_MM = Math.Round(InchesToMillimeter(_xOffsetMM), _decimalPlaces);
                    YOffset_MM = Math.Round(InchesToMillimeter(_yOffsetMM), _decimalPlaces);
                    ZOffset_MM = Math.Round(InchesToMillimeter(_zOffsetMM), _decimalPlaces);
                }

                if (IsRoundCylinderSelected)  // Adjust for Minkowski offset
                {
                    // Based on width and height (if applicable)
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.CylinderHeight_MM), _decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), _decimalPlaces);
                    // The added inflation from minkowski on a cylinder is added based on height, before rotation.
                    ZOffset_MM += oDim.Round_r_MM;
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

        public Task<IScadObject> GetRotate(IScadObject scadObject, double xRotate, double yRotate, double zRotate)
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
            GetDimensionsParts();
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
            GetDimensionsParts(); // Refresh the DataGrid
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

        public async void UpdateObjectMirror()
        {
            _originalMirrorCall = $"mirror([{_XMirror}, {_YMirror}, {_ZMirror}]) ";
            // Update new mirror values
            _XMirror = XMirror;
            _YMirror = YMirror;
            _ZMirror = ZMirror;
            try
            {
                var filePath = Path.Combine(_objectFilePath, "Solids", "object.scad");  // Where the main output is stored
                if (!File.Exists(filePath)) return;

                var wrappedMirrorCall = string.Empty;
                wrappedMirrorCall = $"mirror([{XMirror}, {YMirror}, {ZMirror}]) "; // Wrap axis call in mirror module, this is the search string for replacement
                UpdateFIle.ChangeContentBlockFile(oldCodeBlock: _originalMirrorCall, newCodeBlock: wrappedMirrorCall, filePath: filePath);  // Append updated axis call to object.scad file
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating mirror in object.scad file: " + ex.Message);
            }
        }

        public async Task ObjectToScadFilesAsync()
        {
            // Get any additional updates to parts
            await PartsToScadFilesAsync();

            // Clear current object, reapply axis
            CreateAxis();

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
            sb.AppendLine($"difference() {{");
            sb.AppendLine($"    mirror([{XMirror}, {YMirror}, {ZMirror}]) ");
            sb.AppendLine($"    union() {{");

            // Get the maximum layer value
            int maxLayer = ModuleDimensions.Max(m => m.Layer);
            // Iterate through each layer from 0 to maxLayer
            for (int currentLayer = 0; currentLayer <= maxLayer; currentLayer++)
            {
                // Get all modules for the current layer
                var modulesForLayer = ModuleDimensions
                    .Where(m => m.Layer == currentLayer)
                    .ToList();

                // Intersections take precedence over differences and unions
                var _ModuleDimensionsIntersections = modulesForLayer
                    .Where(m => m.ModuleType == OperationType.Intersection.ToString())
                    .ToList();

                var _ModuleDimensionsDifferences = modulesForLayer
                    .Where(m => m.ModuleType == OperationType.Difference.ToString())
                    .ToList();

                var _ModuleDimensionsUnions = modulesForLayer
                    .Where(m => m.ModuleType == OperationType.Union.ToString())
                    .ToList();

                if (_ModuleDimensionsIntersections.Any())
                {
                    foreach (ModuleDimensions module in _ModuleDimensionsIntersections)
                    {
                        sb.Append($"        "); // Formatting
                        sb.AppendLine(module.Name);
                    }
                }

                else if (_ModuleDimensionsDifferences.Any())
                {
                    foreach (ModuleDimensions module in _ModuleDimensionsDifferences)
                    {
                        sb.Append($"        "); // Formatting
                        sb.AppendLine(module.Name);
                    }
                }
                else if (_ModuleDimensionsUnions.Any())
                {
                    foreach (ModuleDimensions module in _ModuleDimensionsUnions)
                    {
                        sb.Append($"        "); // Formatting
                        sb.AppendLine(module.Name);
                    }
                }
            }

            sb.AppendLine($"    }}");  // Union close bracket
            sb.AppendLine($"}}"); // Difference close bracket

            // Write the call methods to the main object.scad file
            var filePath = Path.Combine(_objectFilePath, "Solids", "object.scad");
            await Output.AppendToSCAD(content: sb.ToString(), filePath: filePath, cancellationToken: new CancellationToken());

            // Open the file in whatever the user has designated as the SCAD IDE associated with opening .scad files
            // Handle the case where the file could not be opened
            await ScadFileOperations.OpenScadFileAsync(filePath, allowDuplicates: false);

            if (ExportToStl) // If export to STL is enabled, do so after creating object.scad
                ExportToSTLAsync();
        }

        // Simplified version using ScadFileOperations
        public async void ExportToSTLAsync()
        {
            // Remove axis first before exporting
            var tempRemoveAxis = _removeAxis;
            RemoveAxis = true;
            UpdateAxisTranslate();

            // Then export to STL
            var scadFile = Path.Combine(_objectFilePath, "Solids", "object.scad");
            var stlFile = Path.Combine(_objectFilePath, "Solids", "object.stl");
            await ScadFileOperations.ExportToStlAsync(scadFile, stlFile);

            // Then restore axis if needed
            RemoveAxis = tempRemoveAxis;
            UpdateAxisTranslate();

            // Set ExportToStl back to false
            ExportToStl = false;
        }

        // ✅ Cache the combined collection
        private ObservableCollection<ModuleDimensions>? _cachedAllModuleDimensions;

        public ObservableCollection<ModuleDimensions> AllModuleDimensions
        {
            get
            {
                if (_cachedAllModuleDimensions == null)
                {
                    RefreshAllModuleDimensions();
                }
                return _cachedAllModuleDimensions!;
            }
        }

        private void RefreshAllModuleDimensions()
        {
            _cachedAllModuleDimensions = new ObservableCollection<ModuleDimensions>();
            foreach (var item in ModuleDimensionsUnions)
                _cachedAllModuleDimensions.Add(item);
            foreach (var item in ModuleDimensionsDifferences)
                _cachedAllModuleDimensions.Add(item);
            foreach (var item in ModuleDimensionsIntersections)
                _cachedAllModuleDimensions.Add(item);
        }

        /**** Unit Conversion ****/
        public void ConvertInputs(int decimalPlaces)
        {
            if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged)
            {
                ConvertInputsImperial(decimalPlaces);
            }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged)
            {
                ConvertInputsMetric(decimalPlaces);
            }
            IsImperial = SelectedUnitValue != UnitSystem.Metric;
            IsMetric = SelectedUnitValue == UnitSystem.Metric;
        }

        private void ConvertInputsImperial(int decimalPlaces)
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
        }

        private void ConvertInputsMetric(int decimalPlaces)
        {
            // Convert from imperial unit system to metric (inches to mm)
            LengthMM = Math.Round(InchesToMillimeter(_lengthMM), decimalPlaces);
            WidthMM = Math.Round(InchesToMillimeter(_widthMM), decimalPlaces);
            HeightMM = Math.Round(InchesToMillimeter(_heightMM), decimalPlaces);
            ThicknessMM = Math.Round(InchesToMillimeter(_thicknessMM), decimalPlaces);
            RadiusMM = Math.Round(InchesToMillimeter(_radiusMM), decimalPlaces);
            Radius1MM = Math.Round(InchesToMillimeter(_radius1MM), decimalPlaces);
            Radius2MM = Math.Round(InchesToMillimeter(_radius2MM), decimalPlaces);
            CylinderHeightMM = Math.Round(InchesToMillimeter(_cylinderHeightMM), decimalPlaces);
            XOffsetMM = Math.Round(InchesToMillimeter(_xOffsetMM), decimalPlaces);
            YOffsetMM = Math.Round(InchesToMillimeter(_yOffsetMM), decimalPlaces);
            ZOffsetMM = Math.Round(InchesToMillimeter(_zOffsetMM), decimalPlaces);
            UnitHasChanged = false;
        }

        /**** Axes List DataGrid ****/
        public void GetAxesList()
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
        }

        public async void CreateUnionModule()
        {
            var objects = SolidDimensions               // Minkowski objects rendered last, simpler shapes first
                .Where(o => o.OperationType == "Union")
                .OrderBy(c => c.SolidType.ToLower() == "cube" ? 0
                          : c.SolidType.ToLower() == "cylinder" ? 1
                          : c.SolidType.ToLower() == "sphere" ? 2
                          : c.SolidType.ToLower() == "roundcube" ? 3
                          : c.SolidType.ToLower() == "roundcylinder" ? 4
                          : 5)
                .ThenBy(c => c.Volume_IN3)
                .ToList();


            if (!objects.Any())
                return; // No objects to process

            // Get unique layers and create a module for each layer
            var layers = objects.Select(o => o.Layer).Distinct().OrderBy(l => l).ToList();

            foreach (var layer in layers)
            {
                // Get objects for this specific layer
                var layerObjects = objects.Where(o => o.Layer == layer).ToList();
                var addMethods = layerObjects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                var solidType = $"L{layer}";
                var unionModule = new ModuleDimensions
                {
                    ModuleType = "Union",
                    ObjectName = Name,
                    ObjectDescription = Description,
                    SolidType = "Object",
                    OSCADMethod = ToUnionModule(addMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                    CreatedAt = DateTime.UtcNow,
                    Layer = layer, // Set the layer for this module
                };

                // Build call method and store in Db
                unionModule.Name = ExtractModuleCallMethod(unionModule.OSCADMethod).ToLower();
                var moduleId = await unionModule.UpsertAsync(DbConnection!);

                // Update all solid objects for this layer with the new ModuleDimensionsId
                var solidIds = layerObjects.Select(o => o.Id);
                await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
            }

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        public async void CreateDifferenceModule()
        {
            CreateUnionModule();
            var objects = SolidDimensions           // Minkowski objects rendered last, simpler shapes first
                .Where(o => o.OperationType == "Difference")
                .OrderBy(c => c.SolidType.ToLower() == "cube" ? 0
                          : c.SolidType.ToLower() == "cylinder" ? 1
                          : c.SolidType.ToLower() == "sphere" ? 2
                          : c.SolidType.ToLower() == "roundcube" ? 3
                          : c.SolidType.ToLower() == "roundcylinder" ? 4
                          : 5)
                .ThenBy(c => c.Volume_IN3)
                .ToList();

            if (!objects.Any())
                return; // No objects to process

            // Get unique layers and create a module for each layer
            var layers = objects.Select(o => o.Layer).Distinct().OrderBy(l => l).ToList();

            foreach (var layer in layers)
            {
                // Get objects for this specific layer
                var layerObjects = objects.Where(o => o.Layer == layer).ToList();

                // Find the base union module for this layer
                ModuleDimensions? baseObj = ModuleDimensions.FirstOrDefault(o =>
                    o.ModuleType == "Union" &&
                    o.ObjectName == Name &&
                    o.Layer == layer);

                if (baseObj != null)
                {
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = layerObjects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                    var solidType = $"L{layer}";
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        ObjectName = Name,
                        ObjectDescription = Description,
                        SolidType = "Object",
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        Layer = layer, // Set the layer for this module
                    };

                    // Get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                    var moduleId = await differenceModule.UpsertAsync(DbConnection!);

                    // Update all solid objects for this layer with the new ModuleDimensionsId
                    var solidIds = layerObjects.Select(o => o.Id);
                    await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                }
                else
                {
                    Console.WriteLine($"No Union available as base object for layer {layer}");
                }
            }

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        public async void CreateIntersectionModule()
        {
            // Get all objects marked as "Intersection"
            var objects = SolidDimensions.Where(o => o.OperationType == "Intersection").ToList();

            if (!objects.Any())
                return; // No objects to process

            // Get unique layers and create a module for each layer
            var layers = objects.Select(o => o.Layer).Distinct().OrderBy(l => l).ToList();

            foreach (var layer in layers)
            {
                // Get objects for this specific layer
                var layerObjects = objects.Where(o => o.Layer == layer).ToList();

                // Find the base union module for this layer
                ModuleDimensions? baseObj = ModuleDimensions.FirstOrDefault(o =>
                    o.ModuleType == "Union" &&
                    o.ObjectName == Name &&
                    o.Layer == layer);

                if (baseObj != null)
                {
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var intersectMethods = layerObjects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                    var solidType = $"L{layer}";
                    var intersectionModule = new ModuleDimensions
                    {
                        ModuleType = "Intersection",
                        ObjectName = Name,
                        ObjectDescription = Description,
                        SolidType = "Object",
                        OSCADMethod = ToIntersectionModule(baseCallMethod, intersectMethods, Name, string.Empty, solidType, _isPreRendered).ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        Layer = layer, // Set the layer for this module
                    };

                    // Get calling method for intersectionModule
                    intersectionModule.Name = ExtractModuleCallMethod(intersectionModule.OSCADMethod).ToLower();
                    var moduleId = await intersectionModule.UpsertAsync(DbConnection!);

                    // Update all solid objects for this layer with the new ModuleDimensionsId
                    var solidIds = layerObjects.Select(o => o.Id);
                    await DbConnection!.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                }
                else
                {
                    Console.WriteLine($"No Union available as base object for layer {layer}");
                }
            }

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        private void UpdateScrewRadiusFromSelection()
        {
            if (IsCylinderSelected || IsRoundCylinderSelected)
            {
                var screwData = SelectedScrewSize;
                double radiusValue = SelectedScrewProperty switch
                {
                    "Screw Thread" => screwData!.ScrewRadius,
                    "Screw Head" => screwData!.ScrewHeadRadius,
                    "Threaded Insert" => screwData!.ThreadedInsertRadius,
                    "Clearance Hole" => screwData!.ClearanceHoleRadius,
                    _ => 0
                };
                RadiusMM = SelectedUnitValue == UnitSystem.Imperial
                    ? Math.Round(MillimeterToInches(radiusValue), _decimalPlaces)
                    : radiusValue;
            }
        }

        // Add this method to update dimensions when server rack is selected
        private void UpdateServerRackDimensionsFromSelection()
        {
            if (!IsCubeSelected && !IsRoundCubeSelected)
                return;

            // Update width if a width type is selected
            if (!string.IsNullOrEmpty(SelectedServerRackWidthType))
            {
                var rackData = ServerRackDimensions.GetAll().FirstOrDefault(); // Independent of Rack Height
                if (rackData == null)
                    return;
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
        }

        private void UpdateViewButtons()
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
        }

        public void LoadPngDimensions(string filePath)
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
            if (_surfaceInvert && XOffsetMM == 0)
                XOffsetMM = LengthMM;  // 100mm OpenSCAD default
        }

        // Method to show OSCAD methods
        public void ShowOSCADMethods(ModuleDimensions module)
        {
            var solids = SolidDimensions.Where(s => s.ModuleDimensionsId == module.Id).ToList();
            if (solids.Any())
            {
                ModalTitle = $"OSCAD Methods for {module.Name}";

                // Build modal content with Module Name (call method) at the top
                var sb = new StringBuilder();
                sb.AppendLine($"Module Name (Call Method): {module.Name}");
                sb.AppendLine();
                sb.AppendLine("Solids:");
                sb.AppendLine(new string('-', 50));
                sb.Append(string.Join("\n\n", solids.Select(s => s.OSCADMethod)));

                ModalContent = sb.ToString();
                IsModalOpen = true;
            }
        }

        /// <summary>
        /// Updates the solid color in the database and regenerates with the new color.
        /// </summary>
        /// <param name="solidId">The ID of the solid to update</param>
        /// <param name="color">The selected OpenScad color</param>
        public async Task UpdateSolidColorAsync(int solidId, OpenScadColor color)
        {
            try
            {
                // Find the solid in the collection
                var solid = SolidDimensions.FirstOrDefault(s => s.Id == solidId);
                if (solid == null) return;

                // Update the solid's color property
                solid.ModuleColor = color.ToString();

                // Update in database
                await solid.UpdateColorAsync(DbConnection!, color.ToString());

                // Regenerate the SCAD files with the new color
                await RegenerateSolidWithColorAsync(solid, color);

                // Refresh the parts file
                await PartsToScadFilesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating solid color: {ex.Message}");
            }
        }

        /// <summary>
        /// Regenerates the solid's OSCAD method with the specified color.
        /// Replaces any existing color wrapper with the new one (does not append).
        /// </summary>
        private async Task RegenerateSolidWithColorAsync(SolidDimensions solid, OpenScadColor color)
        {
            if (solid == null) return;

            var moduleContent = solid.OSCADMethod;

            // Find the opening brace after the module declaration
            int openingBraceIndex = moduleContent.IndexOf('{');
            int closingBraceIndex = moduleContent.LastIndexOf('}');

            if (openingBraceIndex == -1 || closingBraceIndex == -1 || closingBraceIndex <= openingBraceIndex)
            {
                // Invalid format, just wrap it
                solid.OSCADMethod = $"color(\"{color.ToString().ToLower()}\") {{ {moduleContent} }}";
                await solid.UpdateAsync(DbConnection!);
                return;
            }

            // Extract the module header (module name and parameters)
            string moduleHeader = moduleContent[..openingBraceIndex].Trim();

            // Extract the inner content (everything between the braces)
            string innerContent = moduleContent[(openingBraceIndex + 1)..closingBraceIndex].Trim();

            // Remove any existing color() wrappers to avoid nesting
            innerContent = StripColorWrappers(innerContent);

            // Build the updated module with color wrapper
            var sb = new StringBuilder();
            sb.AppendLine($"{moduleHeader} {{");
            sb.AppendLine($"    color(\"{color.ToString().ToLower()}\") {{");
            sb.AppendLine($"        {innerContent}");
            sb.AppendLine($"    }}");
            sb.AppendLine($"}}");

            // Update the solid's OSCAD method to the module definition with color
            solid.OSCADMethod = sb.ToString();
            await solid.UpdateAsync(DbConnection!);
        }

        /// <summary>
        /// Removes nested color() wrappers to prevent accumulation when updating colors.
        /// </summary>
        private static string StripColorWrappers(string content)
        {
            var result = content.Trim();

            // Repeatedly strip color() wrappers until none remain
            while (result.StartsWith("color(", StringComparison.OrdinalIgnoreCase))
            {
                // Find the opening brace of the color() function
                int colorOpenParen = result.IndexOf('(');
                int colorCloseParen = result.IndexOf(')');

                if (colorOpenParen == -1 || colorCloseParen == -1)
                    break;

                // Find the opening brace of the color block
                int blockOpenBrace = result.IndexOf('{', colorCloseParen);
                int blockCloseBrace = FindMatchingCloseBrace(result, blockOpenBrace);

                if (blockOpenBrace == -1 || blockCloseBrace == -1)
                    break;

                // Extract content between the braces
                result = result[(blockOpenBrace + 1)..blockCloseBrace].Trim();
            }

            return result;
        }

        /// <summary>
        /// Finds the matching closing brace for an opening brace at a given index.
        /// </summary>
        private static int FindMatchingCloseBrace(string text, int openBraceIndex)
        {
            if (openBraceIndex == -1 || text[openBraceIndex] != '{')
                return -1;

            int depth = 0;
            for (int i = openBraceIndex; i < text.Length; i++)
            {
                if (text[i] == '{')
                    depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }

            return -1; // No matching closing brace found
        }

        /*** Public Variables ***/
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
        public int LayerIntValue { get => _layerIntValue; set => this.RaiseAndSetIfChanged(ref _layerIntValue, value); }
        public double AlphaIntValue { get => _alphaIntValue; set => this.RaiseAndSetIfChanged(ref _alphaIntValue, value); }
        public double AxisXPositionMM { get => _axisXPositionMM; set { this.RaiseAndSetIfChanged(ref _axisXPositionMM, value); if (AxisStored) UpdateAxisTranslate(); } }
        public double AxisYPositionMM { get => _axisYPositionMM; set { this.RaiseAndSetIfChanged(ref _axisYPositionMM, value); if (AxisStored) UpdateAxisTranslate(); } }
        public double AxisZPositionMM { get => _axisZPositionMM; set { this.RaiseAndSetIfChanged(ref _axisZPositionMM, value); if (AxisStored) UpdateAxisTranslate(); } }
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
        public OpenScadColor[] OpenSCADColors { get => _openScadColors; set => this.RaiseAndSetIfChanged(ref _openScadColors, value); }
        public OpenScadColor SelectedOpenSCADColor { get => _selectedOpenScadColor; set => this.RaiseAndSetIfChanged(ref _selectedOpenScadColor, value); }
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
        public ObservableCollection<ModuleDimensions> LayeredModuleDimensions { get => _layeredModuleDimensions; set => this.RaiseAndSetIfChanged(ref _layeredModuleDimensions, value); }

        public SqliteConnection? DbConnection { get => _dbConnection; set => this.RaiseAndSetIfChanged(ref _dbConnection, value); }

        public string Name
        {
            get => _name;
            set
            {
                var sanitized = NameSanitizerRegex.Replace(value.Trim().ToLower(), "_");
                this.RaiseAndSetIfChanged(ref _name, sanitized);  // ← Use sanitized value
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
        public double XMirror { get => _xMirror; set { this.RaiseAndSetIfChanged(ref _xMirror, value); UpdateObjectMirror(); } }
        public double YMirror { get => _yMirror; set { this.RaiseAndSetIfChanged(ref _yMirror, value); UpdateObjectMirror(); } }
        public double ZMirror { get => _zMirror; set { this.RaiseAndSetIfChanged(ref _zMirror, value); UpdateObjectMirror(); } }
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
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    UpdateServerRackDimensionsFromSelection(); // For cubes and round cubes, update server rack dimensions
                    UpdateViewButtons(); // Generic update of view buttons based on selection
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
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    UpdateViewButtons(); // Generic update of view buttons based on selection
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
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    UpdateViewButtons(); // Generic update of view buttons based on selection
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
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    UpdateViewButtons(); // Generic update of view buttons based on selection
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
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }

        public bool IsSphereSelected
        {
            get => _isSphereSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSphereSelected, value);
                if (_isSphereSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    _isRoundCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    UpdateViewButtons(); // Generic update of view buttons based on selection
                }
            }
        }

        public bool IsRoundCylinderSelected
        {
            get => _isRoundCylinderSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRoundCylinderSelected, value);
                if (_isRoundCylinderSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    _isSphereSelected = false;
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    UpdateViewButtons();
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
                if (_isCylinderSelected || _isRoundCylinderSelected)
                {
                    UpdateScrewRadiusFromSelection();  // ← No await needed
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
                if (_isCylinderSelected || _isRoundCylinderSelected)
                {
                    UpdateScrewRadiusFromSelection();
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
                    UpdateServerRackDimensionsFromSelection();
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
                    UpdateServerRackDimensionsFromSelection();
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

                _cachedAllModuleDimensions = null;  // ← Invalidate cache
                this.RaisePropertyChanged(nameof(AllModuleDimensions));
            }
        }
        public UnitSystem SelectedUnitValue
        {
            get => _selectedUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedUnit, value);
                UnitHasChanged = true;
                ConvertInputs(_decimalPlaces);
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
                GetAxesList(); // Refresh axes list when axis unit changes
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
                    UpdateAxisTranslate();
                else if (_selectedAxis != null)
                    CreateAxis();
            }
        }
        public List<string> SolidTypes { get; } = ["Cube", "Round Cube", "Cylinder", "Round Cylinder", "Sphere", "Surface"];
        public string SelectedSolidType
        {
            get => _selectedShapeType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedShapeType, value);
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
                    case "Round Cylinder":
                        IsRoundCylinderSelected = true;
                        break;
                    case "Sphere":
                        IsSphereSelected = true;
                        break;
                    case "Surface":
                        IsSurfaceSelected = true;
                        break;
                }
            }
        }
    }
}
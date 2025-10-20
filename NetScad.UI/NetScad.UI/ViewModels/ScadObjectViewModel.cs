using Avalonia.Animation;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Axis.Scad.Models;
using NetScad.Axis.Scad.Utility;
using NetScad.Axis.SCAD.Objects;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetScad.Core.Measurements.Conversion;
using static NetScad.Core.Measurements.Selector;
using static NetScad.Core.Utility.WrapInModule;

namespace NetScad.UI.ViewModels
{
    public class ScadObjectViewModel : ValidatableBase
    {
        private ObservableCollection<OuterDimensions> _outerDimensions;
        private ObservableCollection<GeneratedModule>? _axesModulesList;
        private ObservableCollection<ModuleDimensions> _moduleDimensions;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsDifferences;
        private ObservableCollection<ModuleDimensions> _moduleDimensionsUnions;
        private ObservableCollection<CylinderDimensions> _cylinderDimensions;
        private AxisDimensions _axisDimensions;
        private List<string>? _axesList;
        private SqliteConnection _dbConnection;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private double _lengthMM = 0;
        private double _widthMM = 0;
        private double _heightMM = 0;
        private double _thicknessMM = 0;
        private double _lengthIN = 0;
        private double _widthIN = 0;
        private double _heightIN = 0;
        private double _thicknessIN = 0;
        private FilamentType _selectedFilament = FilamentType.Other;
        private UnitSystem _selectedUnit = UnitSystem.Metric;
        private GeneratedModule? _selectedAxis;
        private string? _selectedAxisValue;
        private string _objectAxisDisplay = string.Empty;
        private bool _unitHasChanged;
        private bool _isMetric = true;
        private bool _isImperial = false;
        private bool _axesSelectEnabled = true;
        private bool _appendObject = false;
        public int _decimalPlaces;
        public int? _axisId = null;
        public int? _outerDimensionsId;
        private string _objectFilePath;
        private List<ScrewSize>? _screwSizes;
        private ScrewSize? _selectedScrewSize;
        private string _selectedScrewProperty = "ScrewRadius";
        private OperationType _selectedOperationType = OperationType.Add;
        private bool _unionButton;
        private bool _differenceButton;
        private bool _saveFileButton;
        private double _radiusMM = 0;
        private double _radius1MM = 0;
        private double _radius2MM = 0;
        private double _cylinderHeightMM;
        private double _radiusIN = 0;
        private double _radius1IN = 0;
        private double _radius2IN = 0;
        private double _cylinderHeightIN = 0;
        private bool _isCubeSelected = true;
        private bool _isRoundCubeSelected = false;
        private bool _isCylinderSelected = false;
        private bool _axisStored = false;
        private List<ServerRack>? _serverRackSizes;
        private ServerRack? _selectedServerRack;
        private string _selectedServerRackWidthType = "InnerWidth";
        private double _xOffsetMM = 0;
        private double _yOffsetMM = 0;
        private double _zOffsetMM = 0;
        private double _xOffsetIN = 0;
        private double _yOffsetIN = 0;
        private double _zOffsetIN = 0;
        private bool _isNoneOperation = true;
        private bool _isUnionOperation = false;
        private bool _isDifferenceOperation = false;

        public ScadObjectViewModel()
        {
            OuterDimensions = new ObservableCollection<OuterDimensions>();
            ModuleDimensions = new ObservableCollection<ModuleDimensions>();
            CylinderDimensions = new ObservableCollection<CylinderDimensions>();
            ModuleDimensionsUnions = ModuleDimensionsUnions.Any() ? ModuleDimensionsUnions : new ObservableCollection<ModuleDimensions>();
            ModuleDimensionsDifferences = ModuleDimensionsDifferences.Any() ? ModuleDimensionsDifferences : new ObservableCollection<ModuleDimensions>();
            DbConnection = App.Host.Services.GetRequiredService<SqliteConnection>(); // Get the DbConnection from the DI container
            //_ = GetDimensionsPartAsync();
            _ = ClearObjectAsync();
            _decimalPlaces = Designer.Repositories.OuterDimensions.OpenSCAD_DecimalPlaces;
            FilamentTypes = Enum.GetValues<FilamentType>().ToList();
            UnitSystemValues = Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>().ToList();
            SelectedUnitValue = UnitSystem.Metric;
            ScrewSizes = new ScrewSizeService().ScrewSizes;
            OperationTypes = Enum.GetValues<OperationType>().ToList();
            _objectFilePath = App.Host.Services.GetRequiredService<IScadPathProvider>().ScadPath;
            ServerRackSizes = Enumerable.Range(1, 12).Select(ServerRackDimensions.GetByRackUnits).Where(r => r != null).ToList();
        }
        public bool IsNoneOperation
        {
            get => _isNoneOperation;
            set
            {
                this.RaiseAndSetIfChanged(ref _isNoneOperation, value);
                if (value)
                {
                    _isUnionOperation = false;
                    _isDifferenceOperation = false;
                    this.RaisePropertyChanged(nameof(IsUnionOperation));
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
                    this.RaisePropertyChanged(nameof(IsNoneOperation));
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
                    this.RaisePropertyChanged(nameof(IsNoneOperation));
                    this.RaisePropertyChanged(nameof(IsUnionOperation));
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
                return "None";
            }
        }

        /**** Outer Dimensions DataGrid ****/
        public async Task GetDimensionsPartAsync()
        {
            await AxisDimensionsExtensions.CreateTable(DbConnection); // Ensure AxisDimensions table exists first
            await OuterDimensionsExtensions.CreateTable(DbConnection); // Ensure OuterDimensions table exists
            await ModuleDimensionsExtensions.CreateTable(DbConnection); // Ensure ModuleDimensions table exists
            await CylinderDimensionsExtensions.CreateTable(DbConnection); // Ensure CylinderDimensions table exists
            // Get records from database
            var records = await new OuterDimensions().GetByNameWithAxisAsync(DbConnection, Name); // Gets OuterDimensions with related AxisDimensions
            var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(DbConnection, Name);
            var cylinderRecords = await new CylinderDimensions().GetByNameWithAxisAsync(DbConnection, Name);
            // Update ObservableCollections
            ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
            OuterDimensions = new ObservableCollection<OuterDimensions>(records);
            CylinderDimensions = new ObservableCollection<CylinderDimensions>(cylinderRecords);
            // Update differences and unions buttons
            IsCubeSelected = _isCubeSelected;
            IsRoundCubeSelected = _isRoundCubeSelected;
            IsCylinderSelected = _isCylinderSelected;
        }

        // Clear all input fields
        public async Task ClearInputsAsync()
        {
            Name = string.IsNullOrEmpty(Name) ? string.Empty : Name; // Needs to remain since object process can have multiple components
            Description = string.Empty;  // Needed for making sure that parts added don't have the same description
            LengthMM = 0;
            WidthMM = 0;
            HeightMM = 0;
            ThicknessMM = 0;
            LengthIN = 0;
            WidthIN = 0;
            HeightIN = 0;
            ThicknessIN = 0;
            RadiusMM = 0;
            Radius1MM = 0;
            Radius2MM = 0;
            CylinderHeightMM = 0;
            RadiusIN = 0;
            Radius1IN = 0;
            Radius2IN = 0;
            CylinderHeightIN = 0;
            SelectedServerRack = null;
            SelectedServerRackWidthType = string.Empty;
            SelectedScrewProperty = string.Empty;
            SelectedScrewSize = null;
            SelectedFilament = FilamentType.Other;
            SelectedOperationType = OperationType.Add;
            XOffsetMM = 0;
            YOffsetMM = 0;
            ZOffsetMM = 0;
            XOffsetIN = 0;
            YOffsetIN = 0;
            ZOffsetIN = 0;
        }

        // Clear all object fields
        public async Task ClearObjectAsync()
        {
            Name = string.Empty;
            Description = string.Empty;
            await ClearInputsAsync(); // Clear input fields
            OuterDimensions = new ObservableCollection<OuterDimensions>();
            ModuleDimensions = new ObservableCollection<ModuleDimensions>();
            CylinderDimensions = new ObservableCollection<CylinderDimensions>();
            ObjectAxisDisplay = string.Empty;
            SelectedOperationType = OperationType.Add;
            AxesSelectEnabled = true;
            AxisStored = false;
            SelectedUnitValue = UnitSystem.Metric;
            AppendObject = false;
            IsNoneOperation = true;
            IsUnionOperation = false;
            IsDifferenceOperation = false;
            DifferenceButton = false;
            UnionButton = false;
            SaveFileButton = false;
            IsCubeSelected = true;
            IsRoundCubeSelected = false;
            IsCylinderSelected = false;
            _axisId = AxesSelectEnabled ? null : _axisId;  // If the ability to select an axis is disabled, then use existing Id
            _selectedAxis = AxesSelectEnabled ? null : _selectedAxis;
            ScrewSizes = _screwSizes;
            ServerRackSizes = _serverRackSizes;
            await GetAxesList(); // Refresh axes list when unit changes
            await GetDimensionsPartAsync(); // Refresh the DataGrid
        }

        // Create object and save to database
        public async Task<int> CreateObjectAsync()
        {
            if (_axisId is null) // New axis being applied
                await CreateAxisAsync(); // Create or get AxisDimensions and return its Id

            if (IsCubeSelected || IsRoundCubeSelected)
            {
                var newObject = new OuterDimensions  // Create new OuterDimensions instance
                {
                    Name = Name,
                    Description = Description,
                    Material = SelectedFilament.ToString(),
                    OperationType = SelectedOperationType.ToString(),
                    Length_MM = LengthMM,
                    Width_MM = WidthMM,
                    Height_MM = HeightMM,
                    Thickness_MM = ThicknessMM,
                    XOffset_MM = XOffsetMM,
                    YOffset_MM = YOffsetMM,
                    ZOffset_MM = ZOffsetMM,
                    CreatedAt = DateTime.UtcNow,
                    AxisDimensionsId = _axisId
                };

                newObject.OSCADMethod = await GenerateOSCADAsync(oDim: newObject); // Get the OSCAD method

                // Determine if new row being added is appending the current object or is a new object
                if (AppendObject)
                {
                    await newObject.UpsertAsync(DbConnection); // Save to database, add to object
                }
                else
                {
                    await newObject.UpsertAsync(DbConnection); // Save to database, new object, overwrite existing object
                }

                AppendObject = true; // Set to true since after inserting new row, appending to existing set, or updating, object may be appended.
                await GetDimensionsPartAsync(); // Refresh the DataGrid
                await PartsToScadFilesAsync(); // Refresh object.scad file

                if (AxesSelectEnabled)
                {
                    var axisUsed = string.Empty;
                    if (OuterDimensions.Count > 0)
                        axisUsed = OuterDimensions.SingleOrDefault()?.AxisOSCADMethod;
                    else
                        AxesSelectEnabled = false; // Disable axis selection after it has been created, to use within the same object
                    ObjectAxisDisplay = $"Dimensions: {axisUsed?
                        .Replace("use <Axes/axes.scad>; ", "")
                        .Replace("Get_", "")
                        .Replace("_", " ")
                        .Replace("();", "")
                        .Replace(" Orig ", ", Origin: ")
                        .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                        .Replace("Light ", "")
                        .Replace("Dark ", "")
                        .Replace("MM", "mm")
                        .Replace("Inch", "in")
                        .Replace("x", " x ")}"; // Format for display
                }
                await ClearInputsAsync(); // After new part added, make sure that description is cleared out
                return newObject.Id;
            }
            else if (IsCylinderSelected)
            {
                var newCylinder = new CylinderDimensions  // Create new OuterDimensions instance
                {
                    Name = Name,
                    Description = Description,
                    Material = SelectedFilament.ToString(),
                    OperationType = SelectedOperationType.ToString(),
                    Radius_MM = RadiusMM,
                    Radius1_MM = Radius1MM,
                    Radius2_MM = Radius2MM,
                    Height_MM = CylinderHeightMM,
                    XOffset_MM = XOffsetMM,
                    YOffset_MM = YOffsetMM,
                    ZOffset_MM = ZOffsetMM,
                    CreatedAt = DateTime.UtcNow,
                    AxisDimensionsId = _axisId
                };

                newCylinder.OSCADMethod = await GenerateOSCADAsync(cDim: newCylinder); // Get the OSCAD method

                // Determine if new row being added is appending the current object or is a new object
                if (AppendObject)
                {
                    await newCylinder.UpsertAsync(DbConnection); // Save to database, add to object
                }
                else
                {
                    await newCylinder.UpsertAsync(DbConnection); // Save to database, new object, overwrite existing object
                }

                AppendObject = true; // Set to true since after inserting new row, appending to existing set, or updating, object may be appended.
                await GetDimensionsPartAsync(); // Refresh the DataGrid
                await PartsToScadFilesAsync(); // Refresh object.scad file

                if (AxesSelectEnabled && CylinderDimensions.Count > 0)
                {
                    var axisUsed = CylinderDimensions.SingleOrDefault()?.AxisOSCADMethod;
                    AxesSelectEnabled = false; // Disable axis selection after first object is created, since axis should be reused
                    ObjectAxisDisplay = $"Dimensions: {axisUsed?
                        .Replace("use <Axes/axes.scad>; ", "")
                        .Replace("Get_", "")
                        .Replace("_", " ")
                        .Replace("();", "")
                        .Replace(" Orig ", ", Origin: ")
                        .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                        .Replace("Light ", "")
                        .Replace("Dark ", "")
                        .Replace("MM", "mm")
                        .Replace("Inch", "in")
                        .Replace("x", " x ")}"; // Format for display
                }
                await ClearInputsAsync(); // After new part added, make sure that description is cleared out
                return newCylinder.Id;
            }
            return 0;
        }

        public async Task CreateAxisAsync()
        {
            _axisDimensions = new AxisDimensions
            {
                Theme = _selectedAxis.Theme,
                OSCADMethod = _selectedAxis.CallingMethod,
                Unit = _selectedAxis.Unit,
                MinX = _selectedAxis.MinX,
                MaxX = _selectedAxis.MaxX,
                MinY = _selectedAxis.MinY,
                MaxY = _selectedAxis.MaxY,
                MinZ = _selectedAxis.MinZ,
                MaxZ = _selectedAxis.MaxZ,
                CreatedAt = DateTime.UtcNow,
            };
            _axisDimensions.OSCADMethod = $"{_axisDimensions.IncludeMethod} {_selectedAxis.CallingMethod}";
            _axisId = await _axisDimensions.UpsertAsync(DbConnection); // Save to database

            if (_axisId != null)  // If successful, write to object.scad file
            {
                // Put Scad object file together
                var sb = new StringBuilder();
                sb.AppendLine("// Custom axis");
                sb.AppendLine(_axisDimensions.OSCADMethod); // Use axis
                sb.AppendLine();
                await Output.WriteToSCAD(content: sb.ToString(), filePath: Path.Combine(_objectFilePath, "object.scad"), overWrite: true, cancellationToken: new CancellationToken());
                AxisStored = true;
                AxesSelectEnabled = false;
                await GetDimensionsPartAsync();
                ObjectAxisDisplay = $"Dimensions: {_axisDimensions.OSCADMethod?
                        .Replace("use <Axes/axes.scad>; ", "")
                        .Replace("Get_", "")
                        .Replace("_", " ")
                        .Replace("();", "")
                        .Replace(" Orig ", ", Origin: ")
                        .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                        .Replace("Light ", "")
                        .Replace("Dark ", "")
                        .Replace("MM", "mm")
                        .Replace("Inch", "in")
                        .Replace("x", " x ")}"; // Format for display
            }
        }

        public async Task<string> GenerateOSCADAsync(OuterDimensions? oDim = null, CylinderDimensions? cDim = null)
        {
            if (IsCubeSelected || IsRoundCubeSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim.Length_MM = Math.Round(InchesToMillimeter(oDim.Length_MM), _decimalPlaces);
                    oDim.Width_MM = Math.Round(InchesToMillimeter(oDim.Width_MM), _decimalPlaces);
                    oDim.Height_MM = Math.Round(InchesToMillimeter(oDim.Height_MM), _decimalPlaces);
                    oDim.Thickness_MM = Math.Round(InchesToMillimeter(oDim.Thickness_MM), _decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), _decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), _decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), _decimalPlaces);
                }

                if (IsRoundCubeSelected)
                {
                    // Generate a rounded cube with x,y offset for rounding - OpenSCAD code
                    var roundedCubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim.Length_MM }, { "size_y", oDim.Width_MM }, { "size_z", oDim.Height_MM }, { "round_r", oDim.Round_r_MM }, { "round_h", oDim.Round_h_MM }, { "resolution", oDim.Resolution }
                    };
                    var roundedCube = OScad3D.RoundedCube.ToScadObject(roundedCubeParams);
                    var translate = await GetTranslateAsync(roundedCube, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description, oDim.OperationType, "cube").Trim();
                }
                else if (IsCubeSelected)
                {
                    // Generate a cube - OpenSCAD code
                    var cubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim.Length_MM }, { "size_y", oDim.Width_MM }, { "size_z", oDim.Height_MM },
                    };
                    var cube = OScad3D.Cube.ToScadObject(cubeParams);
                    var translate = await GetTranslateAsync(cube, oDim.XOffset_MM, oDim.YOffset_MM, oDim.ZOffset_MM);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description, oDim.OperationType, "cube").Trim();
                }
            }
            else if (IsCylinderSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert cylinder dimensions to metric for OpenSCAD
                    cDim.Radius_MM = Math.Round(InchesToMillimeter(cDim.Radius_MM), _decimalPlaces);
                    cDim.Radius1_MM = Math.Round(InchesToMillimeter(cDim.Radius1_MM), _decimalPlaces);
                    cDim.Radius2_MM = Math.Round(InchesToMillimeter(cDim.Radius2_MM), _decimalPlaces);
                    cDim.Height_MM = Math.Round(InchesToMillimeter(cDim.Height_MM), _decimalPlaces);
                    cDim.XOffset_MM = Math.Round(InchesToMillimeter(cDim.XOffset_MM), _decimalPlaces);
                    cDim.YOffset_MM = Math.Round(InchesToMillimeter(cDim.YOffset_MM), _decimalPlaces);
                    cDim.ZOffset_MM = Math.Round(InchesToMillimeter(cDim.ZOffset_MM), _decimalPlaces);
                }

                var cylParams = new Dictionary<string, object> { { "r", cDim.Radius_MM }, { "h", cDim.Height_MM }, { "resolution", 180 } };
                var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
                var translate = await GetTranslateAsync(cylinder, cDim.XOffset_MM, cDim.YOffset_MM, cDim.ZOffset_MM);
                return ToModule(translate.OSCADMethod, cDim.Name, cDim.Description, cDim.OperationType, "cylinder").Trim();
            }

            return string.Empty;
        }


        // Make the object's position changeable if this function is called separately. Translate of a translate.
        // Use case for shifting an entire set of child objects as part of an IScadObject
        public async Task<IScadObject> GetTranslateAsync(IScadObject scadObject, double XOffset_MM, double YOffset_MM, double ZOffset_MM)
        {
            var oDim = new OuterDimensions  // Create new OuterDimensions instance for getting RoundRadius and RoundHeight
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

            if (IsRoundCubeSelected)  // Adjust for Minkowski offset
            {
                switch (SelectedOperationType)
                {
                    case OperationType.Add:
                        XOffset_MM += oDim.Round_r_MM;
                        YOffset_MM += oDim.Round_r_MM;
                        ZOffset_MM -= oDim.Round_h_MM;
                        break;
                    case OperationType.Subtract:
                        XOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                        YOffset_MM += oDim.Round_r_MM + oDim.Thickness_MM;
                        ZOffset_MM += -oDim.Round_h_MM + oDim.Thickness_MM;
                        break;
                }
            }
            else if (IsCubeSelected)  // For regular cubes, if thickness has been added, then factor this into offsets
            {
                switch (SelectedOperationType)
                {
                    case OperationType.Subtract:
                        XOffset_MM += oDim.Thickness_MM;
                        YOffset_MM += oDim.Thickness_MM;
                        ZOffset_MM += oDim.Thickness_MM;
                        break;
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
            return translate;
        }

        public async Task PartsToScadFilesAsync()
        {
            /*** Parts scad file ***/
            var sbpart = new StringBuilder();
            var fileName = string.Empty;
            if (IsCubeSelected || IsRoundCubeSelected)
            {
                var cubeUDim = ModuleDimensionsUnions.Where(x => x.SolidType == "Cube");
                var cubeDDim = ModuleDimensionsDifferences.Where(x => x.SolidType == "Cube");
                fileName = $"{Name.Replace(" ", "_").Trim().ToLower()}_{Description.Replace(" ", "_").Trim().ToLower()}_cube.scad";
                var moduleIncludeMethod = $"include <Solids/{fileName}>;";
                // Update ModuleDimensions OSCADMethod in DB
                var moduleUpdate = new ModuleDimensions
                {
                    ObjectDescription = Description,
                    ObjectName = Name,
                    SolidType = "Cube",
                    IncludeMethod = moduleIncludeMethod,
                    CreatedAt = DateTime.UtcNow
                };
                await moduleUpdate.UpdateIncludeMethodByNameDescriptionSolidTypeAsync(DbConnection);

                // Parts file creation
                sbpart.AppendLine($"//Use in main file: {moduleIncludeMethod}");
                sbpart.AppendLine();

                // Put in Scad solids file
                foreach (OuterDimensions module in OuterDimensions) // Get child objects in difference or union modules
                {
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                foreach (ModuleDimensions module in ModuleDimensions.Where(m => m.SolidType == "Cube")) // Get difference or union rows for cubes
                {
                    sbpart.AppendLine($"// {module.ObjectName} - Type: {module.ModuleType}");
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                // If difference functions present, then call those, likely unions are child objects
                if (cubeDDim.Any())
                {
                    foreach (ModuleDimensions module in cubeDDim)
                    {
                        sbpart.AppendLine($"// Calling method to use in your object.scad file");
                        sbpart.AppendLine($"// {module.Name}");
                    }
                }
                // If no difference functions found, then call any unions as they're parent objects
                else if (cubeUDim.Any())
                {
                    foreach (ModuleDimensions module in cubeUDim)
                    {
                        sbpart.AppendLine($"// Calling method to use in your object.scad file");
                        sbpart.AppendLine($"// {module.Name}");
                    }
                }
            }
            else if (IsCylinderSelected)
            {
                var cylUDim = ModuleDimensionsUnions.Where(x => x.SolidType == "Cylinder");
                var cylDDim = ModuleDimensionsDifferences.Where(x => x.SolidType == "Cylinder");
                fileName = $"{Name.Replace(" ", "_").Trim().ToLower()}_{Description.Replace(" ", "_").Trim().ToLower()}_cylinder.scad";
                var moduleIncludeMethod = $"include <Solids/{fileName}>;";
                // Update ModuleDimensions IncludeMethod in DB
                var moduleUpdate = new ModuleDimensions
                {
                    ObjectDescription = Description,
                    ObjectName = Name,
                    SolidType = "Cylinder",
                    IncludeMethod = moduleIncludeMethod,
                    CreatedAt = DateTime.UtcNow
                };
                await moduleUpdate.UpdateIncludeMethodByNameDescriptionSolidTypeAsync(DbConnection);

                // Parts file creation
                sbpart.AppendLine($"//Use in main file: {moduleIncludeMethod}");
                sbpart.AppendLine();

                foreach (CylinderDimensions module in CylinderDimensions) // Get child objects in difference or union modules
                {
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                foreach (ModuleDimensions module in ModuleDimensions.Where(m => m.SolidType == "Cylinder")) // Get difference or union rows for cylinders
                {
                    sbpart.AppendLine($"// {module.ObjectName} - Type: {module.ModuleType}");
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                // If difference functions present, then call those, likely unions are child objects
                if (cylDDim.Any())
                {
                    foreach (ModuleDimensions module in cylDDim)
                    {
                        sbpart.AppendLine($"// Calling method to use in your object.scad file");
                        sbpart.AppendLine($"// {module.Name}");
                    }
                }
                // If no difference functions found, then call any unions as they're parent objects
                else if (cylUDim.Any())
                {
                    foreach (ModuleDimensions module in cylUDim)
                    {
                        sbpart.AppendLine($"// Calling method to use in your object.scad file");
                        sbpart.AppendLine($"// {module.Name}");
                    }
                }
            }

            // Write to part file with name of object
            await Output.WriteToSCAD(content: sbpart.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", fileName), overWrite: true, cancellationToken: new CancellationToken());
            await GetDimensionsPartAsync(); // Refresh the DataGrid
        }

        public async Task ObjectToScadFilesAsync()
        {
            // Get any additional updates to parts
            await PartsToScadFilesAsync();

           // if (_axisId is null) // New axis being applied
                await CreateAxisAsync(); // Create or get AxisDimensions and return its Id

            // Put Scad object file together
            var sb = new StringBuilder();
            sb.AppendLine("// Solid components used in main object");
            if (ModuleDimensions.Any())
            {
                foreach(string includeMethod in ModuleDimensions.Select(y=>y.IncludeMethod).Distinct().ToList())
                {
                    sb.AppendLine(includeMethod);  // Include parts
                }
            }
            sb.AppendLine(); // Calling methods

            // If difference functions present, then call those, likely unions are child objects
            if (ModuleDimensionsDifferences.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsDifferences)
                {
                    sb.AppendLine(module.Name);
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (ModuleDimensionsUnions.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsUnions)
                {
                    sb.AppendLine(module.Name);
                }
            }

            sb.AppendLine();

            // Write the call methods to the main object.scad file
            var filePath = Path.Combine(_objectFilePath, "object.scad");
            await Output.AppendToSCAD(content: sb.ToString(), filePath: filePath, cancellationToken: new CancellationToken());

            // Open the file in whatever the user has designated as the SCAD IDE associated with opening .scad files
            await ScadFileOperations.OpenScadFileAsync(filePath);

            // OR allow duplicates explicitly
            //await ScadFileOperations.OpenScadFileAsync(filePath, allowDuplicates: false);
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
            IsImperial = SelectedUnitValue == UnitSystem.Metric ? false : true;
            IsMetric = SelectedUnitValue == UnitSystem.Metric ? true : false;
        }

        private async Task ConvertInputsImperial(int decimalPlaces)
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

        private async Task ConvertInputsMetric(int decimalPlaces)
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
        }

        /**** Axes List DataGrid ****/
        public async Task GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            _axesModulesList = parser.AxesModulesList(filePath);

            // Filter and select based on unit system
            AxesList = SelectedUnitValue switch
            {
                UnitSystem.Metric => [.. _axesModulesList
                    .Where(x => x.CallingMethod.Contains("_MM_"))
                    .Select(x => x.CallingMethod)],

                UnitSystem.Imperial => [.. _axesModulesList
                    .Where(x => x.CallingMethod.Contains("_Inch_"))
                    .Select(x => x.CallingMethod)],
                _ => _axesModulesList.Select(x => x.CallingMethod).ToList()
            };

            // Update selected axis if current selection is no longer valid
            if (!AxesList.Contains(SelectedAxisValue))
            {
                SelectedAxisValue = AxesList.FirstOrDefault() ?? string.Empty;
                _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == SelectedAxisValue);
            }
        }

        /* Create a union module from all "Add" parts. This assumes that the user will treat a collection of parts as a single part.
        The CreateDifferenceModuleAsync method will handle a single part, and then subtract multiple parts from it.
        Cubes and cylinders are handled separately. This is because the properties are different.
        Manual editing within OpenSCAD is likely needed to place cylinders within cubes to the user's satisfaction.
        The custom object builder is designed to create the parts, and then union or subtract them as needed.
        Once all the parts are created, the user can then manually edit the OpenSCAD file to position them as needed.
        Union should be called first, then Difference. OpenSCAD logic is difference() { union() { A,B,C }, D, E, F } */
        public async Task CreateUnionModuleAsync()
        {
            // Get all objects marked as "Add"
            var cylinders = CylinderDimensions.Where(o => o.OperationType == "Add").ToList();
            var objects = OuterDimensions.Where(o => o.OperationType == "Add").ToList();
            // If user has selected cube, create union of cubes
            if ((IsCubeSelected || IsRoundCubeSelected) && objects.Any())
            {
                // Transform module definitions to call methods
                var methods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                var unionModule = new ModuleDimensions
                {
                    ModuleType = "Union",
                    ObjectName = Name,
                    ObjectDescription = Description,
                    SolidType = "Cube",
                    OSCADMethod = ToUnionModule(methods, Name, string.Empty, "Cube").ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                unionModule.Name = ExtractModuleCallMethod(unionModule.OSCADMethod).ToLower();
                await unionModule.UpsertAsync(DbConnection);
            }
            else if ((IsCylinderSelected) && cylinders.Any())
            {
                var cylinderMethods = cylinders.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                var unionModuleCylinder = new ModuleDimensions
                {
                    ModuleType = "Union",
                    ObjectName = Name,
                    ObjectDescription = Description,
                    SolidType = "Cylinder",
                    OSCADMethod = ToUnionModule(cylinderMethods, Name, string.Empty, "Cylinder").ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                unionModuleCylinder.Name = ExtractModuleCallMethod(unionModuleCylinder.OSCADMethod).ToLower();
                await unionModuleCylinder.UpsertAsync(DbConnection);
            }

            // Refresh ModuleDimensions DataGrid
            var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(DbConnection, Name);
            ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
            await GetDimensionsPartAsync(); // Refresh the DataGrid
            await PartsToScadFilesAsync(); // Refresh object.scad file
        }

        public async Task CreateDifferenceModuleAsync()
        {

            // Get all objects marked as "Subtract"
            var cylinders = CylinderDimensions.Where(o => o.OperationType == "Subtract").ToList();
            var objects = OuterDimensions.Where(o => o.OperationType == "Subtract").ToList();
            ModuleDimensions? baseObj;

            if ((IsCubeSelected || IsRoundCubeSelected) && objects.Any())
            {
                baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.ObjectName == Name && o.SolidType == "Cube");

                if (baseObj != null)
                {
                    // Transform module definition to call method
                    // "module abc_name() { ... }" -> "abc_name();"
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        ObjectName = Name,
                        ObjectDescription = Description,
                        SolidType = baseObj.SolidType,
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, string.Empty, baseObj.SolidType).ToLower(),
                        CreatedAt = DateTime.UtcNow
                    };
                    // get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                    await differenceModule.UpsertAsync(DbConnection);
                    // Refresh ModuleDimensions DataGrid
                    var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(DbConnection, Name);
                    ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
                }
            }
            else if ((IsCylinderSelected) && cylinders.Any())
            {
                baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.ObjectName == Name && o.SolidType == "Cylinder" && o.ObjectDescription == Description);
                if (baseObj != null)
                {
                    // Transform module definition to call method
                    // "module abc_name() { ... }" -> "abc_name();"
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = cylinders.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        ObjectName = Name,
                        ObjectDescription = Description,
                        SolidType = baseObj.SolidType,
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, string.Empty, baseObj.SolidType).ToLower(),
                        CreatedAt = DateTime.UtcNow
                    };
                    // get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                    await differenceModule.UpsertAsync(DbConnection);
                    // Refresh ModuleDimensions DataGrid
                    var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(DbConnection, Name);
                    ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
                    await GetDimensionsPartAsync(); // Refresh the DataGrid
                    await PartsToScadFilesAsync(); // Refresh object.scad file
                }
            }
        }

        private string ExtractModuleCallMethod(string moduleDefinition)
        {
            if (string.IsNullOrWhiteSpace(moduleDefinition))
                return string.Empty;

            // Remove "module " prefix
            var withoutModuleKeyword = moduleDefinition.Trim();
            if (withoutModuleKeyword.StartsWith("module ", StringComparison.OrdinalIgnoreCase))
            {
                withoutModuleKeyword = withoutModuleKeyword.Substring("module ".Length).Trim();
            }

            // Find the closing parenthesis after the module name
            var closingParenIndex = withoutModuleKeyword.IndexOf(')');
            if (closingParenIndex == -1)
                return string.Empty;

            // Extract everything up to and including the closing parenthesis
            var callMethod = withoutModuleKeyword.Substring(0, closingParenIndex + 1);

            // Add semicolon if not already present
            if (!callMethod.EndsWith(";"))
            {
                callMethod += ";";
            }

            return callMethod.Trim();
        }

        private async Task UpdateScrewRadiusFromSelection()
        {
            if (!IsCylinderSelected) return;

            var screwData = SelectedScrewSize;

            double radiusValue = SelectedScrewProperty switch
            {
                "Threaded Portion" => screwData.ScrewRadius,
                "Screw Head" => screwData.ScrewHeadRadius,
                "Threaded Insert" => screwData.ThreadedInsertRadius,
                "Clearance Hole" => screwData.ClearanceHoleRadius,
                _ => 0
            };
            RadiusMM = SelectedUnitValue == UnitSystem.Imperial ? Math.Round(MillimeterToInches(radiusValue), _decimalPlaces) : radiusValue;
        }



        // Add this method to update dimensions when server rack is selected
        private async Task UpdateServerRackDimensionsFromSelection()
        {
            if (!IsCubeSelected && !IsRoundCubeSelected) return;

            // Update width if a width type is selected
            if (!string.IsNullOrEmpty(SelectedServerRackWidthType))
            {
                var rackData = ServerRackDimensions.GetAll().FirstOrDefault(); // Independent of Rack Height
                double widthValue = 0;

                switch (SelectedServerRackWidthType)
                {
                    case "Inner Width":
                        if (SelectedUnitValue == UnitSystem.Metric)
                            widthValue = rackData.InnerWidthMm;
                        else
                            widthValue = rackData.InnerWidthInches;
                        break;

                    case "Outer Width":
                        if (SelectedUnitValue == UnitSystem.Metric)
                            widthValue = rackData.OuterWidthMm;
                        else
                            widthValue = rackData.OuterWidthInches;
                        break;

                    default: widthValue = 0; break;
                }

                WidthMM = Math.Round(widthValue, _decimalPlaces);
            }

            // Update height if server rack is selected
            if (SelectedServerRack != null)
            {
                var rackData = SelectedServerRack;

                // Update Height based on unit system
                if (SelectedUnitValue == UnitSystem.Metric)
                {
                    HeightMM = rackData.HeightMm;
                }
                else // Imperial
                {
                    HeightMM = Math.Round(rackData.HeightInches, _decimalPlaces);
                }
            }
        }
        public bool UnitHasChanged { get => _unitHasChanged; set => this.RaiseAndSetIfChanged(ref _unitHasChanged, value); }
        public bool IsMetric { get => _isMetric; set => this.RaiseAndSetIfChanged(ref _isMetric, value); }
        public bool IsImperial { get => _isImperial; set => this.RaiseAndSetIfChanged(ref _isImperial, value); }
        public bool AxesSelectEnabled { get => _axesSelectEnabled; set => this.RaiseAndSetIfChanged(ref _axesSelectEnabled, value); }
        public bool AxisStored { get => _axisStored; set => this.RaiseAndSetIfChanged(ref _axisStored, value); }
        public bool AppendObject { get => _appendObject; set => this.RaiseAndSetIfChanged(ref _appendObject, value); }
        public bool UnionButton { get => _unionButton; set => this.RaiseAndSetIfChanged(ref _unionButton, value); }
        public bool DifferenceButton { get => _differenceButton; set => this.RaiseAndSetIfChanged(ref _differenceButton, value); }
        public bool SaveFileButton { get => _saveFileButton; set => this.RaiseAndSetIfChanged(ref _saveFileButton, value); }
        public List<FilamentType> FilamentTypes { get; }
        public List<UnitSystem> UnitSystemValues { get; }
        public List<string> AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }
        public List<OperationType> OperationTypes { get; }
        public OperationType SelectedOperationType { get => _selectedOperationType; set => this.RaiseAndSetIfChanged(ref _selectedOperationType, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsUnions { get => _moduleDimensionsUnions; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsUnions, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsDifferences { get => _moduleDimensionsDifferences; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsDifferences, value); }
        public ObservableCollection<CylinderDimensions> CylinderDimensions { get => _cylinderDimensions; set => this.RaiseAndSetIfChanged(ref _cylinderDimensions, value); }
        public SqliteConnection DbConnection { get => _dbConnection; set => this.RaiseAndSetIfChanged(ref _dbConnection, value); }
        public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
        public string ObjectAxisDisplay { get => _objectAxisDisplay; set => this.RaiseAndSetIfChanged(ref _objectAxisDisplay, value); }
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
        public double LengthMM { get => _lengthMM; set => this.RaiseAndSetIfChanged(ref _lengthMM, value); }
        public double WidthMM { get => _widthMM; set => this.RaiseAndSetIfChanged(ref _widthMM, value); }
        public double HeightMM { get => _heightMM; set => this.RaiseAndSetIfChanged(ref _heightMM, value); }
        public double ThicknessMM { get => _thicknessMM; set => this.RaiseAndSetIfChanged(ref _thicknessMM, value); }
        public double LengthIN { get => _lengthIN; set => this.RaiseAndSetIfChanged(ref _lengthIN, value); }
        public double WidthIN { get => _widthIN; set => this.RaiseAndSetIfChanged(ref _widthIN, value); }
        public double HeightIN { get => _heightIN; set => this.RaiseAndSetIfChanged(ref _heightIN, value); }
        public double ThicknessIN { get => _thicknessIN; set => this.RaiseAndSetIfChanged(ref _thicknessIN, value); }
        public FilamentType SelectedFilament { get => _selectedFilament; set => this.RaiseAndSetIfChanged(ref _selectedFilament, value); }
        public double XOffsetMM { get => _xOffsetMM; set => this.RaiseAndSetIfChanged(ref _xOffsetMM, value); }
        public double YOffsetMM { get => _yOffsetMM; set => this.RaiseAndSetIfChanged(ref _yOffsetMM, value); }
        public double ZOffsetMM { get => _zOffsetMM; set => this.RaiseAndSetIfChanged(ref _zOffsetMM, value); }
        public double XOffsetIN { get => _xOffsetIN; set => this.RaiseAndSetIfChanged(ref _xOffsetIN, value); }
        public double YOffsetIN { get => _yOffsetIN; set => this.RaiseAndSetIfChanged(ref _yOffsetIN, value); }
        public double ZOffsetIN { get => _zOffsetIN; set => this.RaiseAndSetIfChanged(ref _zOffsetIN, value); }
        public double RadiusMM { get => _radiusMM; set => this.RaiseAndSetIfChanged(ref _radiusMM, value); }
        public double Radius1MM { get => _radius1MM; set => this.RaiseAndSetIfChanged(ref _radius1MM, value); }
        public double Radius2MM { get => _radius2MM; set => this.RaiseAndSetIfChanged(ref _radius2MM, value); }
        public double CylinderHeightMM { get => _cylinderHeightMM; set => this.RaiseAndSetIfChanged(ref _cylinderHeightMM, value); }
        public double RadiusIN { get => _radiusIN; set => this.RaiseAndSetIfChanged(ref _radiusIN, value); }
        public double Radius1IN { get => _radius1IN; set => this.RaiseAndSetIfChanged(ref _radius1IN, value); }
        public double Radius2IN { get => _radius2IN; set => this.RaiseAndSetIfChanged(ref _radius2IN, value); }
        public double CylinderHeightIN { get => _cylinderHeightIN; set => this.RaiseAndSetIfChanged(ref _cylinderHeightIN, value); }
        public bool IsCubeSelected
        {
            get => _isCubeSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCubeSelected, value);
                if (value)
                {
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));

                    if (_isCubeSelected)
                    {
                        var mDim = ModuleDimensions.Where(o => o.SolidType == "Cube");
                        // If there are any parts to save in the file, or modules to save in the object file that are cube objects
                        switch (mDim.Any())
                        {
                            case true:
                                SaveFileButton = true;
                                // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                                switch (OuterDimensions.Where(o => o.OperationType == "Subtract").Any())
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
                        // If there is at least one add item in the OuterDimensions table
                        switch (OuterDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            case true:
                                UnionButton = true;
                                break;
                            case false:
                                UnionButton = false;
                                break;
                        }
                        _ = UpdateServerRackDimensionsFromSelection();
                    }
                }
            }
        }

        public bool IsRoundCubeSelected
        {
            get => _isRoundCubeSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRoundCubeSelected, value);
                if (value)
                {
                    _isCubeSelected = false;
                    _isCylinderSelected = false;
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));

                    if (_isRoundCubeSelected)
                    {
                        var mDim = ModuleDimensions.Where(o => o.SolidType == "Cube");
                        // If there are any parts to save in the file, or modules to save in the object file that are cube objects
                        switch (mDim.Any())
                        {
                            case true:
                                SaveFileButton = true;
                                // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                                switch (OuterDimensions.Where(o => o.OperationType == "Subtract").Any())
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
                        // If there is at least one add item in the OuterDimensions table
                        switch (OuterDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            case true:
                                UnionButton = true;
                                break;
                            case false:
                                UnionButton = false;
                                break;
                        }
                        _ = UpdateServerRackDimensionsFromSelection();
                    }
                }
            }
        }

        public bool IsCylinderSelected
        {
            get => _isCylinderSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCylinderSelected, value);
                if (value)
                {
                    _isRoundCubeSelected = false;
                    _isCubeSelected = false;
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    
                    if (_isCylinderSelected)
                    {
                        var mDim = ModuleDimensions.Where(o => o.SolidType == "Cylinder");
                        // If there are any parts to save in the file, or modules to save in the object file that are cylinder objects
                        switch (mDim.Any())
                        {
                            case true:
                                SaveFileButton = true;
                                // If there is at least one union row for a cylinder solid to create a difference for, along with at least one subtract item.
                                switch (CylinderDimensions.Where(o => o.OperationType == "Subtract").Any())
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
                        // If there is at least one add item in the CylinderDimensions table
                        switch (CylinderDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            case true:
                                UnionButton = true;
                                break;
                            case false:
                                UnionButton = false;
                                break;
                        }
                    }
                }
            }
        }

        public ScrewSize? SelectedScrewSize
        {
            get => _selectedScrewSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedScrewSize, value);
                if (IsCylinderSelected)
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
                this.RaiseAndSetIfChanged(ref _selectedScrewProperty, value);
                if (IsCylinderSelected)
                {
                    _ = UpdateScrewRadiusFromSelection();
                }
            }
        }
        public List<ScrewSize>? ScrewSizes { get => _screwSizes; set => this.RaiseAndSetIfChanged(ref _screwSizes, value); }
        public List<string> ScrewProperties { get; } = new List<string>
        {
            "Threaded Portion",
            "Screw Head",
            "Threaded Insert",
            "Clearance Hole",
        };
        public ServerRack? SelectedServerRack
        {
            get => _selectedServerRack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedServerRack, value);
                if (IsCubeSelected || IsRoundCubeSelected)
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
                if (IsCubeSelected || IsRoundCubeSelected)
                {
                    _ = UpdateServerRackDimensionsFromSelection();
                }
            }
        }
        public List<ServerRack>? ServerRackSizes { get => _serverRackSizes; set => this.RaiseAndSetIfChanged(ref _serverRackSizes, value); }
        public List<string> ServerRackWidthTypes { get; } = new List<string>
        {
            "Inner Width",
            "Outer Width",
        };

        public ObservableCollection<OuterDimensions> OuterDimensions { get => _outerDimensions; set => this.RaiseAndSetIfChanged(ref _outerDimensions, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensions
        {
            get => _moduleDimensions;
            set
            {
                this.RaiseAndSetIfChanged(ref _moduleDimensions, value);
                ModuleDimensionsUnions = new ObservableCollection<ModuleDimensions>(_moduleDimensions.Where(m => m.ModuleType == "Union"));
                ModuleDimensionsDifferences = new ObservableCollection<ModuleDimensions>(_moduleDimensions.Where(m => m.ModuleType == "Difference"));
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
                if (!_axisStored)
                    _ = GetAxesList(); // Refresh axes list when unit changes
            }
        }

        public string? SelectedAxisValue
        {
            get => _selectedAxisValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAxisValue, value);
                _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == SelectedAxisValue);
            }
        }

    }
}
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Axis.Scad.Models;
using NetScad.Axis.Scad.Utility;
using NetScad.Core.Interfaces;
using NetScad.Core.Material;
using NetScad.Core.Models;
using NetScad.Core.Primitives;
using NetScad.Core.Utility;
using NetScad.Designer.Repositories;
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
        private double _lengthMM;
        private double _widthMM;
        private double _heightMM;
        private double _thicknessMM;
        private double _lengthIN;
        private double _widthIN;
        private double _heightIN;
        private double _thicknessIN;
        private FilamentType _selectedFilament = FilamentType.PLA;
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
        private bool _isScrewSelected = false;
        private List<ScrewSize>? _screwSizes;
        private ScrewSize? _selectedScrewSize;
        private string _selectedScrewProperty = "ScrewRadius";
        private OperationType _selectedOperationType = OperationType.Add;
        private bool _unionButton;
        private bool _differenceButton;
        private bool _saveFileButton;
        private double _radiusMM;
        private double? _radius1MM;
        private double? _radius2MM;
        private double _cylinderHeightMM;
        private double _radiusIN;
        private double? _radius1IN;
        private double? _radius2IN;
        private double _cylinderHeightIN;
        private bool _isCubeSelected = true;
        private bool _isCylinderSelected = false;
        private UnitSystem _baseSelectedUnit;
        private bool _axisStored = false;

        public ScadObjectViewModel()
        {
            // Get the DbConnection from the DI container
            DbConnection = App.Host.Services.GetRequiredService<SqliteConnection>();
            _ = GetDimensionsPart();
            _decimalPlaces = Designer.Repositories.OuterDimensions.OpenSCAD_DecimalPlaces;
            SelectedScrewSize = _selectedScrewSize;
            SelectedScrewProperty = "ScrewRadius";
            // Initialize ModuleDimensions
            ModuleDimensions = new ObservableCollection<ModuleDimensions>();

            // Populate filament types for ComboBox
            FilamentTypes = Enum.GetValues<FilamentType>().ToList();

            // Populate unit system values
            UnitSystemValues = Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>().ToList();
            SelectedUnitValue = UnitSystem.Metric;
            _ = GetAxesList();  // Get existing list of axes generated
            SelectedAxisValue = AxesList.FirstOrDefault();
            _baseSelectedUnit = SelectedUnitValue;
            AxesSelectEnabled = true;
            ObjectAxisDisplay = _objectAxisDisplay;
            AppendObject = _appendObject;
            ScrewSizes = new ScrewSizeService().ScrewSizes;
            // Populate operation types for ComboBox
            OperationTypes = Enum.GetValues<OperationType>().ToList();
            SelectedOperationType = OperationType.Add;
            _objectFilePath = App.Host.Services.GetRequiredService<IScadPathProvider>().ScadPath;
            _selectedScrewSize = _screwSizes?.FirstOrDefault(x => x.Name == "M2");
        }

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
        public UnitSystem SelectedUnitValue
        {
            get => _selectedUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedUnit, value);
                UnitHasChanged = true;
                _ = ConvertInputs(_decimalPlaces);
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

        // Boolean operation properties
        private bool _isNoneOperation = true;
        private bool _isUnionOperation = false;
        private bool _isDifferenceOperation = false;

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
        private async Task GetDimensionsPart()
        {
            // This is safe and efficient since CreateTable uses IF NOT EXISTS
            await AxisDimensionsExtensions.CreateTable(DbConnection); // Ensure AxisDimensions table exists first
            await OuterDimensionsExtensions.CreateTable(DbConnection); // Ensure OuterDimensions table exists
            await ModuleDimensionsExtensions.CreateTable(DbConnection); // Add this line
            await CylinderDimensionsExtensions.CreateTable(DbConnection); // Add this line

            var records = await new OuterDimensions().GetByNameWithAxisAsync(DbConnection, Name); // Gets OuterDimensions with related AxisDimensions
            var moduleRecords = await new ModuleDimensions().GetByOuterDimensionsNameAsync(DbConnection, Name);
            var cylinderRecords = await new CylinderDimensions().GetByNameWithAxisAsync(DbConnection, Name);

            ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
            OuterDimensions = new ObservableCollection<OuterDimensions>(records);
            CylinderDimensions = new ObservableCollection<CylinderDimensions>(cylinderRecords);

            // Update state of buttons showing
            IsCubeSelected = _isCubeSelected;
            IsCylinderSelected = _isCylinderSelected;
            IsScrewSelected = _isScrewSelected;
        }

        // Clear all input fields
        public async Task ClearInputsAsync()
        {
            Name = !UnionButton ? string.Empty : Name;
            Description = string.Empty;
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
            SelectedFilament = FilamentType.PLA;
            SelectedOperationType = OperationType.Add; // Reset to Add
            await GetAxesList();
        }

        // Clear all object fields
        public async Task ClearObjectAsync()
        {
            Name = string.Empty;
            Description = string.Empty;
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
            SelectedFilament = FilamentType.PLA;
            OuterDimensions = new ObservableCollection<OuterDimensions>();
            ModuleDimensions = new ObservableCollection<ModuleDimensions>();
            CylinderDimensions = new ObservableCollection<CylinderDimensions>();
            ObjectAxisDisplay = string.Empty;
            SelectedOperationType = OperationType.Add;
            AxesSelectEnabled = true;
            AxisStored = false;
            SelectedUnitValue = UnitSystem.Metric;
            AppendObject = false;
            SelectedAxisValue = null;
            // Reset boolean operation to None
            IsNoneOperation = true;
            IsUnionOperation = false;
            IsDifferenceOperation = false;
            IsCubeSelected = true;
            IsCylinderSelected = false;
            IsScrewSelected = false;
            _axisId = AxesSelectEnabled ? null : _axisId;  // If the ability to select an axis is disabled, then use existing Id
            _selectedAxis = AxesSelectEnabled ? null : _selectedAxis;
            _baseSelectedUnit = UnitSystem.Metric;
            ScrewSizes = _screwSizes;
            await GetAxesList(); // Refresh axes list when unit changes
            await GetDimensionsPart();
        }

        // Create object and save to database
        public async Task<int> CreateObjectAsync()
        {
            if (_axisId is null) // New axis being applied
                await CreateAxisAsync(); // Create or get AxisDimensions and return its Id

            if (IsCubeSelected)
            {
                var newObject = new OuterDimensions  // Create new OuterDimensions instance
                {
                    Name = Name,
                    Description = Description,
                    Material = SelectedFilament.ToString(),
                    OperationType = SelectedOperationType.ToString(), // Add this line
                    Length_MM = LengthMM,
                    Width_MM = WidthMM,
                    Height_MM = HeightMM,
                    Thickness_MM = ThicknessMM,
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
                await GetDimensionsPart(); // Refresh the DataGrid

                if (AxesSelectEnabled && OuterDimensions.Count > 0)
                {
                    var axisUsed = OuterDimensions.SingleOrDefault()?.AxisOSCADMethod;
                    AxesSelectEnabled = false; // Disable axis selection after first object is created, since axis should be reused
                    ObjectAxisDisplay = $"Dimensions: {axisUsed?
                        .Replace("use <../Axes/axes.scad>; ", "")
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

                return newObject.Id;
            }
            else if (IsCylinderSelected || IsScrewSelected)
            {
                var newCylinder = new CylinderDimensions  // Create new OuterDimensions instance
                {
                    Name = Name,
                    Description = Description,
                    Material = SelectedFilament.ToString(),
                    OperationType = SelectedOperationType.ToString(), // Add this line
                    Radius_MM = RadiusMM,
                    Height_MM = CylinderHeightMM,
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
                await GetDimensionsPart(); // Refresh the DataGrid

                if (AxesSelectEnabled && CylinderDimensions.Count > 0)
                {
                    var axisUsed = CylinderDimensions.SingleOrDefault()?.AxisOSCADMethod;
                    AxesSelectEnabled = false; // Disable axis selection after first object is created, since axis should be reused
                    ObjectAxisDisplay = $"Dimensions: {axisUsed?
                        .Replace("use <../Axes/axes.scad>; ", "")
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
                await Output.WriteToSCAD(content: sb.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", "object.scad"), overWrite: true, cancellationToken: new CancellationToken());
                AxisStored = true;
                AxesSelectEnabled = false;
                await GetDimensionsPart();
                ObjectAxisDisplay = $"Dimensions: {_axisDimensions.OSCADMethod?
                        .Replace("use <../Axes/axes.scad>; ", "")
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
            if (IsCubeSelected)
            {
                if (_selectedUnit == UnitSystem.Imperial)
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim.Length_MM = Math.Round(InchesToMillimeter(oDim.Length_MM), _decimalPlaces);
                    oDim.Width_MM = Math.Round(InchesToMillimeter(oDim.Width_MM), _decimalPlaces);
                    oDim.Height_MM = Math.Round(InchesToMillimeter(oDim.Height_MM), _decimalPlaces);
                    oDim.Thickness_MM = Math.Round(InchesToMillimeter(oDim.Thickness_MM), _decimalPlaces);
                }

                // Generate a rounded cube with x,y offset for rounding - OpenSCAD code
                var roundedCubeParams = new Dictionary<string, object>
                {
                    { "size_x", oDim.Length_MM }, { "size_y", oDim.Width_MM }, { "size_z", oDim.Height_MM }, { "round_r", oDim.Round_r_MM }, { "round_h", oDim.Round_h_MM }, { "resolution", oDim.Resolution }
                };
                var roundedCube = OScad3D.RoundedCube.ToScadObject(roundedCubeParams);
                var transParams = new Dictionary<string, object>();

                // x,y offset for rounding, if subtraction then factor in thickness
                if (oDim.OperationType == "Add")
                    transParams = new Dictionary<string, object> { { "x", oDim.Round_r_MM }, { "y", oDim.Round_r_MM }, { "z", 0.0 }, { "children", new IScadObject[] { roundedCube } } };
                else if (oDim.OperationType == "Subtract")
                    transParams = new Dictionary<string, object> { { "x", oDim.Round_r_MM + oDim.Thickness_MM }, { "y", oDim.Round_r_MM + oDim.Thickness_MM }, { "z", oDim.Thickness_MM }, { "children", new IScadObject[] { roundedCube } } };

                var translate = OScadTransform.Translate.ToScadObject(transParams);
                return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description);
            } 
            else if (IsCylinderSelected || IsScrewSelected)
            {
                var cylParams = new Dictionary<string, object> { { "r", cDim.Radius_MM }, { "h", cDim.Height_MM }, { "resolution", 180 } };
                var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
                return ToModule(cylinder.OSCADMethod, cDim.Name, cDim.Description);
            }

            return string.Empty;
        }

        public async Task ObjectToScadFilesAsync()
        {
            var sbpart = new StringBuilder();
            var fileName = string.Empty;

            if (IsCubeSelected)
            {
                fileName = $"{Name.Replace(" ", "_").Trim().ToLower()}_cube.scad";
                sbpart.AppendLine($"//Use in main file: include <{fileName}>;");
                sbpart.AppendLine();

                // Put in Scad solids file
                foreach (OuterDimensions module in OuterDimensions) // Get child objects in difference or union modules
                {
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                foreach (ModuleDimensions module in ModuleDimensions.Where(m=>m.SolidType == "Cube")) // Get difference or union rows for cubes
                {
                    sbpart.AppendLine($"// {module.OuterDimensionsName} - Type: {module.ModuleType}");
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }
            }
            else if (IsCylinderSelected || IsScrewSelected)
            {
                fileName = $"{Name.Replace(" ", "_").Trim().ToLower()}_cylinder.scad";
                sbpart.AppendLine($"//Use in main file: include <{fileName}>;");
                sbpart.AppendLine();

                foreach (CylinderDimensions module in CylinderDimensions) // Get child objects in difference or union modules
                {
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }

                foreach (ModuleDimensions module in ModuleDimensions.Where(m => m.SolidType == "Cylinder")) // Get difference or union rows for cylinders
                {
                    sbpart.AppendLine($"// {module.OuterDimensionsName} - Type: {module.ModuleType}");
                    sbpart.AppendLine(module.OSCADMethod);
                    sbpart.AppendLine();
                }
            }

            // If difference functions present, then call those, likely unions are child objects
            if (ModuleDimensionsDifferences.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsDifferences)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
            // If no difference functions found, then call any unions as they're parent objects
            else if (ModuleDimensionsUnions.Any())
            {
                foreach (ModuleDimensions module in ModuleDimensionsUnions)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
                // Write to part file with name of object
                await Output.WriteToSCAD(content: sbpart.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", fileName), overWrite: true, cancellationToken: new CancellationToken());

            if (AxisStored)
            {
                // Put Scad object file together
                var sb = new StringBuilder();
                sb.AppendLine("// Solid components used in main object");
                sb.AppendLine($"include <{fileName}>;");  // Include parts
                sb.AppendLine();

                // Call functions at the top level in object.scad, put differences at top level, and then put generic difference() { union() { <difference calling methods> } }
                //sb.AppendLine($"difference() {{");
                //sb.AppendLine($"    union() {{");

                // If difference functions present, then call those, likely unions are child objects
                if (ModuleDimensionsDifferences.Any())
                {
                    foreach (ModuleDimensions module in ModuleDimensionsDifferences)
                    {
                        //sb.Append($"        ");
                        sb.AppendLine(module.Name);
                    }
                }
                // If no difference functions found, then call any unions as they're parent objects
                else if (ModuleDimensionsUnions.Any())
                {
                    foreach (ModuleDimensions module in ModuleDimensionsUnions)
                    {
                        //sb.Append($"        ");
                        sb.AppendLine(module.Name);
                    }
                }

                sb.AppendLine();
               //sb.AppendLine($"    }}"); // Union close
                //sb.AppendLine($"}}");  // Difference close

                // Write the call methods to the main object.scad file
                await Output.AppendToSCAD(content: sb.ToString(), filePath: Path.Combine(_objectFilePath, "Solids", "object.scad"), cancellationToken: new CancellationToken());
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
            CylinderHeightMM = Math.Round(MillimeterToInches(_cylinderHeightMM), decimalPlaces);
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
            CylinderHeightMM = Math.Round(InchesToMillimeter(_cylinderHeightMM), decimalPlaces);
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
        The CreateDiffernceModuleAsync method will handle a single part, and then subtract multiple parts from it.
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
            if (IsCubeSelected && objects.Any())
            {
                // Transform module definitions to call methods
                var methods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                var unionModule = new ModuleDimensions
                {
                    ModuleType = "Union",
                    OuterDimensionsName = Name,
                    SolidType = "Cube",
                    OSCADMethod = ToUnionModule(methods, Name, "Cube").ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                unionModule.Name = ExtractModuleCallMethod(unionModule.OSCADMethod).ToLower();
                await unionModule.UpsertAsync(DbConnection);
            }
            else if ((IsCylinderSelected || IsScrewSelected) && cylinders.Any())
            {
                var cylinderMethods = cylinders.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                var unionModuleCylinder = new ModuleDimensions
                {
                    ModuleType = "Union",
                    OuterDimensionsName = Name,
                    SolidType = "Cylinder",
                    OSCADMethod = ToUnionModule(cylinderMethods, Name, "Cylinder").ToLower(),
                    CreatedAt = DateTime.UtcNow
                };
                unionModuleCylinder.Name = ExtractModuleCallMethod(unionModuleCylinder.OSCADMethod).ToLower();
                await unionModuleCylinder.UpsertAsync(DbConnection);
            }

            // Refresh ModuleDimensions DataGrid
            var moduleRecords = await new ModuleDimensions().GetByOuterDimensionsNameAsync(DbConnection, Name);
            ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
            await GetDimensionsPart(); // Refresh the DataGrid
        }

        /* Create a union module from all "Add" parts. This assumes that the user will treat a collection of parts as a single part.
        The CreateDiffernceModuleAsync method will handle a single part, and then subtract multiple parts from it.
        Cubes and cylinders are handled separately. This is because the properties are different.
        Manual editing within OpenSCAD is likely needed to place cylinders within cubes to the user's satisfaction.
        The custom object builder is designed to create the parts, and then union or subtract them as needed.
        Once all the parts are created, the user can then manually edit the OpenSCAD file to position them as needed.
        Union should be called first, then Difference. OpenSCAD logic is difference() { union() { A,B,C }, D, E, F } */
        public async Task CreateDifferenceModuleAsync()
        {
            
            // Get all objects marked as "Subtract"
            var cylinders = CylinderDimensions.Where(o => o.OperationType == "Subtract").ToList();
            var objects = OuterDimensions.Where(o => o.OperationType == "Subtract").ToList();
            ModuleDimensions? baseObj;

            if (IsCubeSelected && objects.Any())
            {
                baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.OuterDimensionsName == Name && o.SolidType == "Cube");

                if (baseObj != null)
                {
                    // Transform module definition to call method
                    // "module abc_name() { ... }" -> "abc_name();"
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = objects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        OuterDimensionsName = Name,
                        SolidType = baseObj.SolidType,
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, baseObj.SolidType).ToLower(),
                        CreatedAt = DateTime.UtcNow
                    };
                    // get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower(); 
                    await differenceModule.UpsertAsync(DbConnection);
                    // Refresh ModuleDimensions DataGrid
                    var moduleRecords = await new ModuleDimensions().GetByOuterDimensionsNameAsync(DbConnection, Name);
                    ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
                }
            }
            else if ((IsCylinderSelected || IsScrewSelected) && cylinders.Any())
            {
                baseObj = ModuleDimensions.FirstOrDefault(o => o.ModuleType == "Union" && o.OuterDimensionsName == Name && o.SolidType == "Cylinder");
                if (baseObj != null)
                {
                    // Transform module definition to call method
                    // "module abc_name() { ... }" -> "abc_name();"
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = cylinders.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        OuterDimensionsName = Name,
                        SolidType = baseObj.SolidType,
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, Name, baseObj.SolidType).ToLower(),
                        CreatedAt = DateTime.UtcNow
                    };
                    // get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                    await differenceModule.UpsertAsync(DbConnection);
                    // Refresh ModuleDimensions DataGrid
                    var moduleRecords = await new ModuleDimensions().GetByOuterDimensionsNameAsync(DbConnection, Name);
                    ModuleDimensions = new ObservableCollection<ModuleDimensions>(moduleRecords);
                    await GetDimensionsPart(); // Refresh the DataGrid
                }
            }
        }

        /// <summary>
        /// Extracts the module call method from a module definition
        /// </summary>
        /// <param name="moduleDefinition">Full module definition (e.g., "module abc_name() { ... }")</param>
        /// <returns>Module call method (e.g., "abc_name();")</returns>
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
            if (!IsScrewSelected) return;

            var screwData = SelectedScrewSize;

            double radiusValue = SelectedScrewProperty switch
            {
                "ScrewRadius" => screwData.ScrewRadius,
                "ScrewHeadRadius" => screwData.ScrewHeadRadius,
                "ThreadedInsertRadius" => screwData.ThreadedInsertRadius,
                "ClearanceHoleRadius" => screwData.ClearanceHoleRadius,
                _ => screwData.ScrewRadius
            };
            RadiusMM = radiusValue;
            //CylinderHeightIN = Math.Round(InchesToMillimeter(_cylinderHeightIN), _decimalPlaces);
        }

        public double RadiusMM { get => _radiusMM; set => this.RaiseAndSetIfChanged(ref _radiusMM, value); }
        public double? Radius1MM { get => _radius1MM; set => this.RaiseAndSetIfChanged(ref _radius1MM, value); }
        public double? Radius2MM { get => _radius2MM; set => this.RaiseAndSetIfChanged(ref _radius2MM, value); }
        public double CylinderHeightMM { get => _cylinderHeightMM; set => this.RaiseAndSetIfChanged(ref _cylinderHeightMM, value); }
        public double RadiusIN { get => _radiusIN; set => this.RaiseAndSetIfChanged(ref _radiusIN, value); }
        public double? Radius1IN { get => _radius1IN; set => this.RaiseAndSetIfChanged(ref _radius1IN, value); }
        public double? Radius2IN { get => _radius2IN; set => this.RaiseAndSetIfChanged(ref _radius2IN, value); }
        public double CylinderHeightIN { get => _cylinderHeightIN; set => this.RaiseAndSetIfChanged(ref _cylinderHeightIN, value); }

        public bool IsCubeSelected
        {
            get => _isCubeSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCubeSelected, value);
                if (value)
                {
                    _isCylinderSelected = false;
                    _isScrewSelected = false;
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsScrewSelected));
                    if (_isCubeSelected)
                    {
                        // If there are any parts to save in the file, or modules to save in the object file that are cube objects
                        if (ModuleDimensions.Where(o => o.SolidType == "Cube").Any())
                        {
                            SaveFileButton = true;
                        }
                        else
                        {
                            SaveFileButton = false;
                        }
                        // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                        if (ModuleDimensionsUnions.Where(o => o.SolidType == "Cube").Any() && OuterDimensions.Where(o => o.OperationType == "Subtract").Any())
                        {
                            DifferenceButton = true;
                        }
                        else
                        {
                            DifferenceButton = false;
                        }
                        // If there is at least one add item in the OuterDimensions table
                        if (OuterDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            UnionButton = true;
                        }
                        else
                        {
                            UnionButton = false;
                        }
                        if (_baseSelectedUnit == UnitSystem.Imperial)
                        {
                            IsMetric = false; // If base object was Imperial, but then M screw was metric, then revert to Imperial for tubing, if user is measuring in Imperial units.
                            IsImperial = true;
                            SelectedUnitValue = UnitSystem.Imperial;
                        }
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
                    _isCubeSelected = false;
                    _isScrewSelected = false;
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsScrewSelected));
                    if (_isCylinderSelected)
                    {
                        // If there are any parts to save in the file, or modules to save in the object file that are cube objects
                        if (ModuleDimensions.Where(o => o.SolidType == "Cylinder").Any())
                        {
                            SaveFileButton = true;
                        }
                        else
                        {
                            SaveFileButton = false;
                        }
                        // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                        if (ModuleDimensionsUnions.Where(o => o.SolidType == "Cylinder").Any() && CylinderDimensions.Where(o => o.OperationType == "Subtract").Any())
                        {
                            DifferenceButton = true;
                        }
                        else
                        {
                            DifferenceButton = false;
                        }
                        // If there is at least one add item in the CylinderDimensions table
                        if (CylinderDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            UnionButton = true;
                        }
                        else
                        {
                            UnionButton = false;
                        }

                        if (_baseSelectedUnit == UnitSystem.Imperial)
                        {
                            IsMetric = false; // If base object was Imperial, but then M screw was metric, then revert to Imperial for tubing, if user is measuring in Imperial units.
                            IsImperial = true;
                            SelectedUnitValue = UnitSystem.Imperial;
                        }
                    }

                }
            }
        }

        // Add these public properties (after the IsCylinderSelected property, around line 600)
        public bool IsScrewSelected
        {
            get => _isScrewSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isScrewSelected, value);
                if (value)
                {
                    _isCubeSelected = false;
                    _isCylinderSelected = false; 
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    if (_isScrewSelected)
                    {
                        // If there are any parts to save in the file, or modules to save in the object file that are cube objects
                        if (ModuleDimensions.Where(o => o.SolidType == "Cylinder").Any())
                        {
                            SaveFileButton = true;
                        }
                        else
                        {
                            SaveFileButton = false;
                        }
                        // If there is at least one union row for a cube solid to create a difference for, along with at least one subtract item.
                        if (ModuleDimensionsUnions.Where(o => o.SolidType == "Cylinder").Any() && CylinderDimensions.Where(o => o.OperationType == "Subtract").Any())
                        {
                            DifferenceButton = true;
                        }
                        else
                        {
                            DifferenceButton = false;
                        }
                        // If there is at least one add item in the CylinderDimensions table
                        if (CylinderDimensions.Where(o => o.OperationType == "Add").Any())
                        {
                            UnionButton = true;
                        }
                        else
                        {
                            UnionButton = false;
                        }

                        //_baseSelectedUnit = SelectedUnitValue;
                        IsMetric = true; // If base object was Imperial, but then M screw was metric, then revert to Imperial for tubing, if user is measuring in Imperial units.
                        IsImperial = false;
                        SelectedUnitValue = UnitSystem.Metric;
                    }
                }
            }
        }

        public ScrewSize SelectedScrewSize
        {
            get => _selectedScrewSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedScrewSize, value);
                if (IsScrewSelected)
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
                if (IsScrewSelected)
                {
                    _ = UpdateScrewRadiusFromSelection();
                }
            }
        }

        public List<ScrewSize>? ScrewSizes { get => _screwSizes; set => this.RaiseAndSetIfChanged(ref _screwSizes, value); }

        public List<string> ScrewProperties { get; } = new List<string>
        {
            "ScrewRadius",
            "ScrewHeadRadius",
            "ThreadedInsertRadius",
            "ClearanceHoleRadius",
        };
    }
}
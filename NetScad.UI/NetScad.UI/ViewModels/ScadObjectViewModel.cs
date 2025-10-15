using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Axis.Scad.Models;
using NetScad.Axis.Scad.Utility;
using NetScad.Core.Interfaces;
using NetScad.Core.Material;
using NetScad.Core.Primitives;
using NetScad.Designer.Repositories;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static NetScad.Core.Measurements.Conversion;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.ViewModels
{
    public class ScadObjectViewModel : ValidatableBase
    {
        private ObservableCollection<OuterDimensions> _outerDimensions;
        private ObservableCollection<GeneratedModule>? _axesModulesList;
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
        public int? _axisId;
        public int? _outerDimensionsId;

        public ScadObjectViewModel()
        {
            // Get the DbConnection from the DI container
            DbConnection = App.Host.Services.GetRequiredService<SqliteConnection>();
            OuterDimensions = GetDimensionsPart().Result;
            _decimalPlaces = Designer.Repositories.OuterDimensions.OpenSCAD_DecimalPlaces;

            // Populate filament types for ComboBox
            FilamentTypes = Enum.GetValues<FilamentType>().ToList();

            // Populate unit system values
            UnitSystemValues = Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>().ToList();
            SelectedUnitValue = UnitSystem.Metric;
            _ = GetAxesList();  // Get existing list of axes generated
            SelectedAxisValue = AxesList.FirstOrDefault();
            AxesSelectEnabled = OuterDimensions.Count == 0; // Enable axis selection only if no OuterDimensions exist
            ObjectAxisDisplay = _objectAxisDisplay;
            AppendObject = _appendObject;
        }

        public ObservableCollection<OuterDimensions> OuterDimensions { get => _outerDimensions; set => this.RaiseAndSetIfChanged(ref _outerDimensions, value); }
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
        public bool AppendObject { get => _appendObject; set => this.RaiseAndSetIfChanged(ref _appendObject, value); }
        public List<FilamentType> FilamentTypes { get; }
        public List<UnitSystem> UnitSystemValues { get; }
        public List<string> AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }

        /**** Outer Dimensions DataGrid ****/
        private async Task<ObservableCollection<OuterDimensions>> GetDimensionsPart()
        {
            // This is safe and efficient since CreateTable uses IF NOT EXISTS
            await AxisDimensionsExtensions.CreateTable(DbConnection); // Ensure AxisDimensions table exists first
            await OuterDimensionsExtensions.CreateTable(DbConnection); // Ensure OuterDimensions table exists
            //var records = await new OuterDimensions().GetAllWithAxisAsync(DbConnection); // Gets OuterDimensions with related AxisDimensions
            var records = await new OuterDimensions().GetByNameWithAxisAsync(DbConnection, Name); // Gets OuterDimensions with related AxisDimensions
            return new ObservableCollection<OuterDimensions>(records);
        }

        // Clear all input fields
        public async Task ClearInputsAsync()
        {
            Name = !AppendObject ? string.Empty : _name;
            Description = string.Empty;
            LengthMM = 0;
            WidthMM = 0;
            HeightMM = 0;
            ThicknessMM = 0;
            LengthIN = 0;
            WidthIN = 0;
            HeightIN = 0;
            ThicknessIN = 0;
            SelectedFilament = FilamentType.PLA;
            SelectedUnitValue = !AppendObject ? UnitSystem.Metric : _selectedUnit;
            await GetAxesList(); // Refresh axes list when unit changes
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
            SelectedFilament = FilamentType.PLA;
            OuterDimensions = new ObservableCollection<OuterDimensions>();
            ObjectAxisDisplay = string.Empty;
            AxesSelectEnabled = true;
            SelectedUnitValue = UnitSystem.Metric;
            AppendObject = false;
            SelectedAxisValue = null;
            _axisId = AxesSelectEnabled ? null : _axisId;  // If the ability to select an axis is disabled, then use existing Id
            _selectedAxis = AxesSelectEnabled ? null : _selectedAxis;
            await GetAxesList(); // Refresh axes list when unit changes
        }

        // Create object and save to database
        public async Task<int> CreateObjectAsync()
        {
            if (_axisId is null) // New axis being applied
                _axisId = await CreateAxisAsync(); // Create or get AxisDimensions and return its Id

            var newObject = new OuterDimensions  // Create new OuterDimensions instance
            {
                Name = _name,
                Description = _description,
                Length_MM = _lengthMM,
                Width_MM = _widthMM,
                Height_MM = _heightMM,
                Thickness_MM = _thicknessMM,
                Material = _selectedFilament.ToString(),
                CreatedAt = DateTime.UtcNow,
                AxisDimensionsId = _axisId
            };

            newObject.OSCADMethod = await GenerateOSCADAsync(newObject); // Get the OSCAD method

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

            OuterDimensions = await GetDimensionsPart(); // Refresh the DataGrid
            if(AxesSelectEnabled && OuterDimensions.Count > 0)
            {
                var axisUsed = OuterDimensions.SingleOrDefault()?.AxisOSCADMethod;
                AxesSelectEnabled = false; // Disable axis selection after first object is created, since axis should be reused
                ObjectAxisDisplay = $"Dimensions: {axisUsed?
                    .Replace("use <Axes/axes.scad>; ", "")
                    .Replace("Get_", "")
                    .Replace("_", " ")
                    .Replace("();", "")
                    .Replace(" Orig ", ", Origin: ")
                    .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                    .Replace("Light ","")
                    .Replace("Dark ", "")
                    .Replace("MM", "mm")
                    .Replace("Inch", "in")
                    .Replace("x", " x ")}"; // Format for display
            }
                

            return newObject.Id;
        }

        public async Task<int> CreateAxisAsync()
        {
            var newAxis = new AxisDimensions
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
            newAxis.OSCADMethod = $"{newAxis.IncludeMethod} {_selectedAxis.CallingMethod}";
            return await newAxis.UpsertAsync(DbConnection); // Save to database
        }

        public async Task<string> GenerateOSCADAsync(OuterDimensions oDim)
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

            // x,y offset for rounding
            var transParams = new Dictionary<string, object> { { "x", -oDim.Round_r_MM }, { "y", -oDim.Round_r_MM }, { "z", 0.0 }, { "children", new IScadObject[] { roundedCube } } };
            var translate = OScadTransform.Translate.ToScadObject(transParams);
            return translate.OSCADMethod; // translate([-Length, -Width, 0]) { roundedCube(...) };
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
            UnitHasChanged = false;
        }

        private async Task ConvertInputsMetric(int decimalPlaces)
        {
            // Convert from imperial unit system to metric (inches to mm)
            LengthMM = Math.Round(InchesToMillimeter(_lengthMM), decimalPlaces);
            WidthMM = Math.Round(InchesToMillimeter(_widthMM), decimalPlaces);
            HeightMM = Math.Round(InchesToMillimeter(_heightMM), decimalPlaces);
            ThicknessMM = Math.Round(InchesToMillimeter(_thicknessMM), decimalPlaces);
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
    }
}
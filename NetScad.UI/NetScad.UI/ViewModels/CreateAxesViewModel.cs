using Microsoft.Data.Sqlite;
using NetScad.Axis.Scad.Models;
using NetScad.Axis.Scad.Utility;
using NetScad.Axis.SCAD.Modules;
using NetScad.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static NetScad.Axis.SCAD.Utility.AxisConfig;
using static NetScad.Core.Measurements.Conversion;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.ViewModels
{
    public class CreateAxesViewModel : ValidatableBase
    {
        private UnitSystem _selectedUnit;         // Pass these back to backend functions
        private BackgroundType _selectedBackground;
        private double _minX;
        private double _maxX;
        private double _minY;
        private double _maxY;
        private double _minZ;
        private double _maxZ;
        private bool _unitHasChanged;
        private bool _isMetric;
        private bool _isImperial;
        private CustomAxis _customAxis;
        private bool _axisDetailsShown;
        private string _moduleName;
        private string _callingMethod;
        private string _includeFile;
        private double _totalCubicVolume;
        private double _totalCubicVolumeScale;
        public int _decimalPlaces;
        public int _callingMethodLength;
        private string _inputMinX;
        private string _inputMaxX;
        private string _inputMinY;
        private string _inputMaxY;
        private string _inputMinZ;
        private string _inputMaxZ;
        private ObservableCollection<GeneratedModule> _axesList;
        private bool _createButtonEnabled;

        public CreateAxesViewModel()
        {
            UnitSystemValues = Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>().ToList();
            BackgroundTypeValues = Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>().ToList();
            SelectedBackgroundValue = BackgroundType.Light;
            SelectedUnitValue = UnitSystem.Metric;
            MaxXValue = SelectedUnitValue == UnitSystem.Metric ? 300 : 12; // Set based on defaults
            MaxYValue = SelectedUnitValue == UnitSystem.Metric ? 300 : 12;
            MaxZValue = SelectedUnitValue == UnitSystem.Metric ? 300 : 12;
            MinXValue = 0;
            MinYValue = 0;
            MinZValue = 0;
            _unitHasChanged = false;
            _createButtonEnabled = true;
            UnitHasChanged = false;
            _isImperial = SelectedUnitValue == UnitSystem.Metric ? false : true; // Set based on SelectedUnit
            _isMetric = SelectedUnitValue == UnitSystem.Metric ? true : false;
            IsImperial = SelectedUnitValue == UnitSystem.Metric ? false : true;
            IsMetric = SelectedUnitValue == UnitSystem.Metric ? true : false;
            _axisDetailsShown = true; // Module Details
            AxisDetailsShown = false;
            ModuleName = string.Empty;
            CallingMethod = string.Empty;
            IncludeFile = string.Empty;
            TotalCubicVolume = 0;
            TotalCubicVolumeScale = 0;
            _decimalPlaces = 12; // Rounding for conversions
            _callingMethodLength = 0; // For selectable text for module to be called in SCAD file
            _inputMinX = "Min X <= 0"; // Watermarks for X coordinates
            _inputMaxX = "Max X > Min X";
            _inputMinY = "Min Y <= 0"; // Watermarks for Y coordinates
            _inputMaxY = "Max Y > Min Y";
            _inputMinZ = "Min Z <= 0"; // Watermarks for Z coordinates
            _inputMaxZ = "Max Z > Min Z";
            _ = GetAxesList();  // Get existing list of axes generated
        }
        public double MinXValue
        {
            get => _minX; set
            {
                this.RaiseAndSetIfChanged(ref _minX, value);
                _ = ValidateMinMax();
            }
        }
        public double MaxXValue
        {
            get => _maxX; set
            {
                this.RaiseAndSetIfChanged(ref _maxX, value);
                _ = ValidateMinMax();
            }
        }
        public double MinYValue
        {
            get => _minY; set
            {
                this.RaiseAndSetIfChanged(ref _minY, value);
                _ = ValidateMinMax();
            }
        }
        public double MaxYValue
        {
            get => _maxY; set
            {
                this.RaiseAndSetIfChanged(ref _maxY, value);
                _ = ValidateMinMax();
            }
        }
        public double MinZValue
        {
            get => _minZ; set
            {
                this.RaiseAndSetIfChanged(ref _minZ, value);
                _ = ValidateMinMax();
            }
        }
        public double MaxZValue
        {
            get => _maxZ; set 
            {
                this.RaiseAndSetIfChanged(ref _maxZ, value);
                _ = ValidateMinMax();
            }
        }
        public string MinXWatermark { get => _inputMinX; set => this.RaiseAndSetIfChanged(ref _inputMinX, value); }
        public string MaxXWatermark { get => _inputMaxX; set => this.RaiseAndSetIfChanged(ref _inputMaxX, value); }
        public string MinYWatermark { get => _inputMinY; set => this.RaiseAndSetIfChanged(ref _inputMinY, value); }
        public string MaxYWatermark { get => _inputMaxY; set => this.RaiseAndSetIfChanged(ref _inputMaxY, value); }
        public string MinZWatermark { get => _inputMinZ; set => this.RaiseAndSetIfChanged(ref _inputMinZ, value); }
        public string MaxZWatermark { get => _inputMaxZ; set => this.RaiseAndSetIfChanged(ref _inputMaxZ, value); }
        public List<UnitSystem> UnitSystemValues { get; set; }
        public UnitSystem SelectedUnitValue
        {
            get => _selectedUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedUnit, value);
                UnitHasChanged = true; // For use in conversions when _selectedUnit has changed
                _ = ConvertInputs(_decimalPlaces);
                _ = GetAxesList(); // Update AxesList based on unit system
            }
        }
        public List<BackgroundType> BackgroundTypeValues { get; set; }
        public BackgroundType SelectedBackgroundValue { get => _selectedBackground; set => this.RaiseAndSetIfChanged(ref _selectedBackground, value); }
        public bool UnitHasChanged { get => _unitHasChanged; set => this.RaiseAndSetIfChanged(ref _unitHasChanged, value); }
        public bool IsMetric { get => _isMetric; set => this.RaiseAndSetIfChanged(ref _isMetric, value); }
        public bool IsImperial { get => _isImperial; set => this.RaiseAndSetIfChanged(ref _isImperial, value); }
        public bool AxisDetailsShown { get => _axisDetailsShown; set => this.RaiseAndSetIfChanged(ref _axisDetailsShown, value); }
        public string ModuleName { get => _moduleName; set => this.RaiseAndSetIfChanged(ref _moduleName, value); }
        public string CallingMethod { get => _callingMethod; set => this.RaiseAndSetIfChanged(ref _callingMethod, value); }
        public string IncludeFile { get => _includeFile; set => this.RaiseAndSetIfChanged(ref _includeFile, value); }
        public double TotalCubicVolume { get => _totalCubicVolume; set => this.RaiseAndSetIfChanged(ref _totalCubicVolume, value); }
        public double TotalCubicVolumeScale { get => _totalCubicVolumeScale; set => this.RaiseAndSetIfChanged(ref _totalCubicVolumeScale, value); }
        public int CallingMethodLength { get => _callingMethodLength; set => this.RaiseAndSetIfChanged(ref _callingMethodLength, value); }
        public ObservableCollection<GeneratedModule> AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }
        public bool CreateButtonEnabled { get => _createButtonEnabled; set => this.RaiseAndSetIfChanged(ref _createButtonEnabled, value); }
        public static SqliteConnection Connection { get; set; } = new SqliteConnection();

        public async Task ConvertInputs(int decimalPlaces) // Convert from unit system to another
        {
            if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged) { await ConvertInputsImperial(decimalPlaces); }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged) { await ConvertInputsMetric(decimalPlaces); }
            IsImperial = SelectedUnitValue == UnitSystem.Metric ? false : true;
            IsMetric = SelectedUnitValue == UnitSystem.Metric ? true : false;
        }

        public async Task CreateCustomAxisAsync()
        {
            AxisDetailsShown = false;      // Disables display of previous generated output details
            if (!double.IsNaN(_minX) && !double.IsNaN(_maxX) &&   // Check inputs to ensure they're doubles
                !double.IsNaN(_minY) && !double.IsNaN(_maxY) &&
                !double.IsNaN(_minZ) && !double.IsNaN(_maxZ))
            {
                if (_selectedUnit == UnitSystem.Imperial && !UnitHasChanged)
                {
                    // Convert if inputs are inches but function is mm
                    _minX = Math.Round(InchesToMillimeter(_minX), _decimalPlaces);
                    _maxX = Math.Round(InchesToMillimeter(_maxX), _decimalPlaces);
                    _minY = Math.Round(InchesToMillimeter(_minY), _decimalPlaces);
                    _maxY = Math.Round(InchesToMillimeter(_maxY), _decimalPlaces);
                    _minZ = Math.Round(InchesToMillimeter(_minZ), _decimalPlaces);
                    _maxZ = Math.Round(InchesToMillimeter(_maxZ), _decimalPlaces);
                    UnitHasChanged = true; // To prevent converting inches to mm twice
                }
                var axisSettings = new AxisSettings(
                 //outputDirectory: PathHelper.GetProjectRoot(), // ..NetScad.UI.<Platform>/Scad/Axes
                 outputDirectory: "", // ..NetScad.UI.<Platform>/Scad/Axes
                 backgroundType: _selectedBackground,
                 measureType: _selectedUnit,
                 minX: _minX,
                 maxX: _maxX,
                 minY: _minY,
                 maxY: _maxY,
                 minZ: _minZ,
                 maxZ: _maxZ
                );

                /**** SetAxis Generation function ****/
                _customAxis = new CustomAxis();             //Clear out previous custom axis data
                _customAxis = await GUI.SetAxis(axisSettings);

                // Convert values back to inches since backend function uses mm
                if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged)  // Update frontend with adjusted imperial values
                {
                    await ConvertInputsImperial((int)(_decimalPlaces / 1.5));
                }
                else   // Update frontend with adjusted metric values, no conversion needed
                {
                    MinXValue = Math.Round(_customAxis.Settings.MinX, (int)(_decimalPlaces / 1.5));
                    MaxXValue = Math.Round(_customAxis.Settings.MaxX, (int)(_decimalPlaces / 1.5));
                    MinYValue = Math.Round(_customAxis.Settings.MinY, (int)(_decimalPlaces / 1.5));
                    MaxYValue = Math.Round(_customAxis.Settings.MaxY, (int)(_decimalPlaces / 1.5));
                    MinZValue = Math.Round(_customAxis.Settings.MinZ, (int)(_decimalPlaces / 1.5));
                    MaxZValue = Math.Round(_customAxis.Settings.MaxZ, (int)(_decimalPlaces / 1.5));
                }

                // Set Post-Axis Generation Details
                IsImperial = SelectedUnitValue == UnitSystem.Metric ? false : true;
                IsMetric = SelectedUnitValue == UnitSystem.Metric ? true : false;
                TotalCubicVolume = _customAxis.TotalCubicVolume; // Provides the total volume of the axes
                TotalCubicVolumeScale = _customAxis.TotalCubicVolumeScale; // Provides the total volume of the axes
                ModuleName = _customAxis.ModuleName;          // Update new axis details
                CallingMethod = _customAxis.CallingMethod; // To call the axis module
                CallingMethodLength = _customAxis.CallingMethod.Length - 1; // For copy and paste into main file
                IncludeFile = $"include <{_customAxis.CallingMethod.ToLower().Replace("();", "")}.scad>";
                AxisDetailsShown = true;
                await GetAxesList();  // Get updated AxesList
            }
            else
            {
                CallingMethod = "Please enter only numeric coordinates";
                AxisDetailsShown = true;
            }
        }

        public async Task ClearInputs()
        {
            ClearErrors(nameof(MaxXValue));
            ClearErrors(nameof(MaxYValue));
            ClearErrors(nameof(MaxZValue));
            ClearErrors(nameof(MinXValue));
            ClearErrors(nameof(MinYValue));
            ClearErrors(nameof(MinZValue));
            SelectedUnitValue = UnitSystem.Metric; // Defaults for enums
            SelectedBackgroundValue = BackgroundType.Light;
            AxisDetailsShown = false; // Post-gen of axis details - static resources disabled in XAML
            TotalCubicVolume = 0;
            TotalCubicVolumeScale = 0;
            ModuleName = string.Empty;
            CallingMethod = string.Empty;
            IncludeFile = string.Empty;
            MaxXValue = 300;
            MaxYValue = 300;
            MaxZValue = 300;
            MinXValue = 0;  // Set to 0 for coordinates
            MinYValue = 0;
            MinZValue = 0;
        }

        /**** Axes List DataGrid ****/
        private async Task GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            _axesList = parser.AxesModulesList(filePath);
            // Filter and select based on unit system
            AxesList = SelectedUnitValue switch
            {
                UnitSystem.Metric => [.. _axesList
                    .Where(x => x.CallingMethod.Contains("_MM_"))],

                UnitSystem.Imperial => [.. _axesList
                    .Where(x => x.CallingMethod.Contains("_Inch_"))],

                _ => _axesList
            };

        }

        // ViewModel helper functions for conversions - stateful
        private async Task ConvertInputsImperial(int decimalPlaces)
        {
            // Convert from metric unit system to imperial
            MinXValue = Math.Round(MillimeterToInches(_minX), decimalPlaces);  // mm to inches
            MaxXValue = Math.Round(MillimeterToInches(_maxX), decimalPlaces);
            MinYValue = Math.Round(MillimeterToInches(_minY), decimalPlaces);
            MaxYValue = Math.Round(MillimeterToInches(_maxY), decimalPlaces);
            MinZValue = Math.Round(MillimeterToInches(_minZ), decimalPlaces);
            MaxZValue = Math.Round(MillimeterToInches(_maxZ), decimalPlaces);
            TotalCubicVolume = Math.Round(VolumeConverter.ConvertCm3ToIn3(_totalCubicVolume), decimalPlaces);  // cm3 to in3
            TotalCubicVolumeScale = Math.Round(VolumeConverter.ConvertM3ToFt3(_totalCubicVolumeScale), decimalPlaces);  // m to feet 
            UnitHasChanged = false;
        }

        private async Task ConvertInputsMetric(int decimalPlaces)
        {
            // Convert from imperial unit system to metric
            MinXValue = Math.Round(InchesToMillimeter(_minX), decimalPlaces);  // inches to mm
            MaxXValue = Math.Round(InchesToMillimeter(_maxX), decimalPlaces);
            MinYValue = Math.Round(InchesToMillimeter(_minY), decimalPlaces);
            MaxYValue = Math.Round(InchesToMillimeter(_maxY), decimalPlaces);
            MinZValue = Math.Round(InchesToMillimeter(_minZ), decimalPlaces);
            MaxZValue = Math.Round(InchesToMillimeter(_maxZ), decimalPlaces);
            TotalCubicVolume = Math.Round(VolumeConverter.ConvertIn3ToCm3(_totalCubicVolume), decimalPlaces);  // inches to cm
            TotalCubicVolumeScale = Math.Round(VolumeConverter.ConvertFt3ToM3(_totalCubicVolumeScale), decimalPlaces);  // feet to m
            UnitHasChanged = false;
        }

        private async Task ValidateMinMax()
        {
            ClearErrors(nameof(MaxXValue));
            ClearErrors(nameof(MaxYValue));
            ClearErrors(nameof(MaxZValue));
            ClearErrors(nameof(MinXValue));
            ClearErrors(nameof(MinYValue));
            ClearErrors(nameof(MinZValue));

            var xValid = true;
            var yValid = true;
            var zValid = true;

            // X
            if (MinXValue > 0)
            {
                AddError(nameof(MinXValue), "Min X <= 0");
                xValid = false;
            }
            if (MaxXValue < 0)
            {
                AddError(nameof(MaxXValue), "Max X >= 0");
                xValid = false;
            }
            if (MinXValue >= MaxXValue)
            {
                AddError(nameof(MinXValue), "Min X < Max X");
                AddError(nameof(MaxXValue), "Max X > Min X");
                xValid = false;
            }

            // Y
            if (MinYValue > 0)
            {
                AddError(nameof(MinYValue), "Min Y <= 0");
                yValid = false;
            }
            if (MaxYValue < 0)
            {
                AddError(nameof(MaxYValue), "Max Y >= 0");
                yValid = false;
            }
            if (MinYValue >= MaxYValue)
            {
                AddError(nameof(MinYValue), "Min Y < Max Y");
                AddError(nameof(MaxYValue), "Max Y > Min Y");
                yValid = false;
            }

            // Z
            if (MinZValue > 0)
            {
                AddError(nameof(MinZValue), "Min Z <= 0");
                zValid = false;
            }
            if (MaxZValue < 0)
            {
                AddError(nameof(MaxZValue), "Max Z >= 0");
                zValid = false;
            }
            if (MinZValue >= MaxZValue)
            {
                AddError(nameof(MinZValue), "Min Z < Max Z");
                AddError(nameof(MaxZValue), "Max Z > Min Z");
                zValid = false;
            }

            if (xValid && yValid && zValid) { CreateButtonEnabled = true; }
            else { CreateButtonEnabled = false; }
        }
    }
}
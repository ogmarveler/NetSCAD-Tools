using Microsoft.Data.Sqlite;
using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Axis.SCAD.Modules;
using NetGenCAD.Core.Measurements;
using NetGenCAD.Designer.Functions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static NetGenCAD.Axis.SCAD.Utility.AxisConfig;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;

namespace NetGenCAD.UI.ViewModels
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
        private CustomAxis? _customAxis;
        private bool _axisDetailsShown;
        private string _moduleName = string.Empty;
        private string _callingMethod = string.Empty;
        private string _includeFile = string.Empty;
        private double _totalCubicVolume;
        private double _totalCubicVolumeScale;
        public int _decimalPlaces;
        public int _callingMethodLength;
        private string _inputMinX = string.Empty;
        private string _inputMaxX = string.Empty;
        private string _inputMinY = string.Empty;
        private string _inputMaxY = string.Empty;
        private string _inputMinZ = string.Empty;
        private string _inputMaxZ = string.Empty;
        private ObservableCollection<GeneratedModule> _axesList = [];
        private ObservableCollection<GeneratedModule> _axesListImperial = [];
        private ObservableCollection<GeneratedModule> _axesListMetric = [];
        private bool _createButtonEnabled;
        private string _modalTitle = string.Empty;
        private string _modalContent = string.Empty;
        private bool _isModalOpen;

        [UnconditionalSuppressMessage("Trimming", "IL2026")]
        [UnconditionalSuppressMessage("AOT", "IL3050")]
        public CreateAxesViewModel()
        {
            UnitSystemValues = [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
            BackgroundTypeValues = [.. Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>()];
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
            _isImperial = SelectedUnitValue != UnitSystem.Metric; // Set based on SelectedUnit
            _isMetric = SelectedUnitValue == UnitSystem.Metric;
            IsImperial = SelectedUnitValue != UnitSystem.Metric;
            IsMetric = SelectedUnitValue == UnitSystem.Metric;
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

        public void RaisePropertyChanged(string propertyName) { this.RaisePropertyChanged(propertyName); }
        public double MinXValue { get => _minX; set { this.RaiseAndSetIfChanged(ref _minX, value); _ = ValidateMinMax(); } }
        public double MaxXValue { get => _maxX; set { this.RaiseAndSetIfChanged(ref _maxX, value); _ = ValidateMinMax(); } }
        public double MinYValue { get => _minY; set { this.RaiseAndSetIfChanged(ref _minY, value); _ = ValidateMinMax(); } }
        public double MaxYValue { get => _maxY; set { this.RaiseAndSetIfChanged(ref _maxY, value); _ = ValidateMinMax(); } }
        public double MinZValue { get => _minZ; set { this.RaiseAndSetIfChanged(ref _minZ, value); _ = ValidateMinMax(); } }
        public double MaxZValue { get => _maxZ; set { this.RaiseAndSetIfChanged(ref _maxZ, value); _ = ValidateMinMax(); } }
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
                // Update IsMetric and IsImperial IMMEDIATELY (synchronously)
                IsImperial = _selectedUnit != UnitSystem.Metric;
                IsMetric = _selectedUnit == UnitSystem.Metric;

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
        public ObservableCollection<GeneratedModule> AxesListImperial { get => _axesListImperial; set => this.RaiseAndSetIfChanged(ref _axesListImperial, value); }
        public ObservableCollection<GeneratedModule> AxesListMetric { get => _axesListMetric; set => this.RaiseAndSetIfChanged(ref _axesListMetric, value); }
        public bool CreateButtonEnabled { get => _createButtonEnabled; set => this.RaiseAndSetIfChanged(ref _createButtonEnabled, value); }
        public static SqliteConnection Connection { get; set; } = new SqliteConnection();
        public string ModalTitle { get => _modalTitle; set => this.RaiseAndSetIfChanged(ref _modalTitle, value); }
        public string ModalContent { get => _modalContent; set => this.RaiseAndSetIfChanged(ref _modalContent, value); }
        public bool IsModalOpen { get => _isModalOpen; set => this.RaiseAndSetIfChanged(ref _isModalOpen, value); }

        public async Task CreateCustomAxisAsync()
        {
            AxisDetailsShown = false; // Disables display of previous generated output details

            // Define the callback to update ViewModel properties
            CreateAxesFunctions.CreateCustomAxisCallbackAsync onAxisCreated = async (
                customAxis,
                displayMinX,
                displayMaxX,
                displayMinY,
                displayMaxY,
                displayMinZ,
                displayMaxZ,
                callingMethodLength,
                newUnitHasChanged) =>
            {
                // Update axis range display values
                MinXValue = displayMinX;
                MaxXValue = displayMaxX;
                MinYValue = displayMinY;
                MaxYValue = displayMaxY;
                MinZValue = displayMinZ;
                MaxZValue = displayMaxZ;

                // Update unit changed flag
                UnitHasChanged = newUnitHasChanged;

                // Set Post-Axis Generation Details
                IsImperial = SelectedUnitValue != UnitSystem.Metric;
                IsMetric = SelectedUnitValue == UnitSystem.Metric;
                TotalCubicVolume = customAxis.TotalCubicVolume;
                TotalCubicVolumeScale = customAxis.TotalCubicVolumeScale;
                ModuleName = customAxis.ModuleName;
                CallingMethod = customAxis.CallingMethod;
                CallingMethodLength = callingMethodLength;
                IncludeFile = $"include <{customAxis.CallingMethod.ToLower().Replace("();", "")}.scad>";
                AxisDetailsShown = true;

                // Refresh the axes list
                await GetAxesList();
            };

            // Call the static function with callback
            await CreateAxesFunctions.CreateCustomAxisWithCallbackAsync(
                _minX, _maxX,
                _minY, _maxY,
                _minZ, _maxZ,
                _selectedUnit,
                _selectedBackground,
                _decimalPlaces,
                UnitHasChanged,
                onAxisCreated);

            // Handle invalid input case
            if (!AxisDetailsShown)
            {
                CallingMethod = "Please enter only numeric coordinates";
                AxisDetailsShown = true;
            }
        }

        public Task ClearInputs()
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
            return Task.CompletedTask;
        }

        /**** Axes List DataGrid ****/
        private Task GetAxesList()
        {
            // Call the static function
            var (allAxes, metricAxes, imperialAxes) = CreateAxesFunctions.GetAxesList();

            // Update ViewModel properties
            _axesList = allAxes;
            AxesListMetric = metricAxes;
            AxesListImperial = imperialAxes;

            return Task.CompletedTask;
        }

        public async Task ConvertInputs(int decimalPlaces)
        {
            if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged)
            {
                var result = CreateAxesFunctions.ConvertInputToImperial(
                    _minX, _maxX, _minY, _maxY, _minZ, _maxZ, _totalCubicVolume, _totalCubicVolumeScale, decimalPlaces);

                MinXValue = result.MinX;
                MaxXValue = result.MaxX;
                MinYValue = result.MinY;
                MaxYValue = result.MaxY;
                MinZValue = result.MinZ;
                MaxZValue = result.MaxZ;
                TotalCubicVolume = result.Volume;
                TotalCubicVolumeScale = result.VolumeScale;
                UnitHasChanged = false;
            }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged)
            {
                var result = CreateAxesFunctions.ConvertInputToMetric(
                    _minX, _maxX, _minY, _maxY, _minZ, _maxZ, _totalCubicVolume, _totalCubicVolumeScale, decimalPlaces);

                MinXValue = result.MinX;
                MaxXValue = result.MaxX;
                MinYValue = result.MinY;
                MaxYValue = result.MaxY;
                MinZValue = result.MinZ;
                MaxZValue = result.MaxZ;
                TotalCubicVolume = result.Volume;
                TotalCubicVolumeScale = result.VolumeScale;
                UnitHasChanged = false;
            }

            await Task.CompletedTask;
        }

        private Task ValidateMinMax()
        {
            // Call the static validation function
            var validationResult = CreateAxesFunctions.ValidateAxisRanges(
                _minX, _maxX,
                _minY, _maxY,
                _minZ, _maxZ);

            // Clear all previous errors
            ClearErrors(nameof(MaxXValue));
            ClearErrors(nameof(MaxYValue));
            ClearErrors(nameof(MaxZValue));
            ClearErrors(nameof(MinXValue));
            ClearErrors(nameof(MinYValue));
            ClearErrors(nameof(MinZValue));

            // Apply errors from validation result
            foreach (var errorEntry in validationResult.ErrorMessages)
            {
                string propertyName = errorEntry.Key switch
                {
                    nameof(_minX) or $"{nameof(_minX)}_range" => nameof(MinXValue),
                    nameof(_maxX) or $"{nameof(_maxX)}_range" => nameof(MaxXValue),
                    nameof(_minY) or $"{nameof(_minY)}_range" => nameof(MinYValue),
                    nameof(_maxY) or $"{nameof(_maxY)}_range" => nameof(MaxYValue),
                    nameof(_minZ) or $"{nameof(_minZ)}_range" => nameof(MinZValue),
                    nameof(_maxZ) or $"{nameof(_maxZ)}_range" => nameof(MaxZValue),
                    _ => errorEntry.Key
                };

                AddError(propertyName, errorEntry.Value);
            }

            // Update button enabled state based on validation
            CreateButtonEnabled = validationResult.IsValid;
            return Task.CompletedTask;
        }
    }
}
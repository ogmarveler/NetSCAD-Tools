using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Core.Interfaces;
using NetGenCAD.Core.Material;
using NetGenCAD.Core.Primitives;
using NetGenCAD.Designer.Functions;
using NetGenCAD.Designer.Repositories;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static NetGenCAD.Core.Measurements.Selector;
using static NetGenCAD.Designer.Functions.ObjectScadFunctions;
using static NetGenCAD.Designer.Functions.ShapeScadFunctions;

namespace NetGenCAD.UI.ViewModels
{
    public class ScadShapeViewModel : ValidatableBase
    {
        private SqliteConnection? _dbConnection;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private FilamentType _selectedFilament = FilamentType.Other;
        private UnitSystem _selectedUnit = UnitSystem.Metric;
        private bool _unitHasChanged;
        private bool _isMetric = true;
        private bool _isImperial = false;
        public int _decimalPlaces;
        private readonly string _objectFilePath;
        private PolyhedronOperationType _selectedPolyhedronOperationType = PolyhedronOperationType.Points;
        private double _pointXMM = 0;
        private double _pointYMM = 0;
        private double _pointZMM = 0;
        private int _pointsId = 0;
        private int _faceId = 0;
        private string _facePoints = string.Empty;
        private bool _isModalOpen;
        private string _modalTitle = string.Empty;
        private string _modalContent = string.Empty;
        private int _polyhedronConvexity = 1;
        private static readonly System.Text.RegularExpressions.Regex NameSanitizerRegex = new System.Text.RegularExpressions.Regex(@"[^a-z0-9]+", System.Text.RegularExpressions.RegexOptions.Compiled);
        private ObservableCollection<PolyhedronDimensions> _polyhedronDimensions = new();
        private ObservableCollection<PolyhedronDimensions> _polyhedronDimensionsPoints = new();
        private ObservableCollection<PolyhedronDimensions> _polyhedronDimensionsFaces = new();
        private ObservableCollection<ShapeDimensions> _shapeDimensions = new();
        private string _shapeScad = string.Empty;

        // Axis-related fields
        public int? _axisId = null;
        private AxisDimensions? _axisDimensions = new();
        private List<string> _axesList = [];
        private string? _selectedAxisValue = "Select Axis";
        private bool _axisStored = false;
        private double _axisXPositionMM = 0;
        private double _axisYPositionMM = 0;
        private double _axisZPositionMM = 0;
        private UnitSystem _selectedAxisUnit = UnitSystem.Metric;
        private bool _isAxisMetric = true;
        private bool _isAxisImperial = false;
        private bool _axesSelectEnabled = true;
        private string _shapeAxisDisplay = string.Empty;
        private string _shapeAxisUnitDisplay = string.Empty;
        private ObservableCollection<GeneratedModule> _axesModulesList;
        private GeneratedModule? _selectedAxis = new();
        private double _AxisXPositionMM = 0;
        private double _AxisYPositionMM = 0;
        private double _AxisZPositionMM = 0;
        private bool _originalRemoveAxis = false;
        private string _originalAxisCall = string.Empty;
        private bool _removeAxis = false;
        private bool _isOpenSCADOpened = false;

        [UnconditionalSuppressMessage("Trimming", "IL2026")]
        [UnconditionalSuppressMessage("AOT", "IL3050")]
        public ScadShapeViewModel()
        {
            PolyhedronDimensions = [];
            _polyhedronDimensions = [];
            PolyhedronDimensionsPoints = [];
            _polyhedronDimensionsPoints = [];
            PolyhedronDimensionsFaces = [];
            _polyhedronDimensionsFaces = [];
            _axesModulesList = [];
            DbConnection = App.Services!.GetRequiredService<SqliteConnection>(); // Get the DbConnection from the DI container
            ClearShape();
            GetAxesList();
            AxisStored = false;
            _decimalPlaces = NetGenCAD.Designer.Repositories.PolyhedronDimensions.OpenSCAD_DecimalPlaces;
            UnitSystemValues = [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
            SelectedUnitValue = UnitSystem.Metric;
            SelectedAxisValue = AxesList?.FirstOrDefault() ?? string.Empty;
            _selectedAxis = _axesModulesList.FirstOrDefault(x => x.CallingMethod == SelectedAxisValue);
            SelectedUnitValue = UnitSystem.Metric;
            _objectFilePath = App.Services!.GetRequiredService<IScadPathProvider>().ScadPath;
        }

        // Stores the current polyhedron shape (reference by PolyhedronDimensions Name into ShapeDimensions)
        public async Task CreateNewShapeModuleAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(ShapeScad) || 
                PolyhedronDimensionsPoints.Count == 0 || PolyhedronDimensionsFaces.Count == 0)
            {
                ModalTitle = "Error";
                ModalContent = "Please ensure the shape has a name, points, faces, and generated SCAD code.";
                IsModalOpen = true;
                return;
            }

            try
            {
                int shapeId = await ShapeScadFunctions.CreateNewShapeModuleAsync(
                    shapeName: Name,
                    shapeDescription: Description,
                    shapeScadCode: ShapeScad,
                    polyhedronDimensions: PolyhedronDimensions,
                    dbConnection: DbConnection!);

                if (shapeId > 0)
                {
                    ModalTitle = "Success";
                    ModalContent = $"Shape '{Name}' saved successfully with ID: {shapeId}";
                    IsModalOpen = false;
                    GetDimensionPolyhedronParts(); // Refresh datagrids
                }
                else
                {
                    ModalTitle = "Error";
                    ModalContent = "Failed to save shape. Please try again.";
                    IsModalOpen = true;
                }
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"An error occurred while saving the shape: {ex.Message}";
                IsModalOpen = true;
            }
        }

        // Updates all rows in SolidDimensions where the Object's PolyhedronName = Name (PolyhedronDimensions or ShapeDimensions Name)
        public async Task UpdateSolidDimensionsAsync()
        {
            try
            {
                if (DbConnection == null || string.IsNullOrWhiteSpace(Name) || ShapeDimensions.Count == 0)
                {
                    ModalTitle = "Error";
                    ModalContent = "No shape found or database connection unavailable.";
                    IsModalOpen = true;
                    return;
                }

                // Get the first (and typically only) ShapeDimensions record for this shape
                var shape = ShapeDimensions.FirstOrDefault();
                if (shape == null)
                {
                    ModalTitle = "Error";
                    ModalContent = "Shape record not found in database.";
                    IsModalOpen = true;
                    return;
                }

                // Update all SolidDimensions rows that reference this shape
                int rowsUpdated = await ShapeScadFunctions.UpdateSolidDimensionsWithShapeAsync(
                    shapeName: Name,
                    newShapeScadCode: shape.OSCADMethod,
                    boxLengthMM: shape.BoxLength_MM,
                    boxWidthMM: shape.BoxWidth_MM,
                    boxHeightMM: shape.BoxHeight_MM,
                    volumeCM3: shape.Volume_CM3,
                    boxLengthIN: shape.BoxLength_IN,
                    boxWidthIN: shape.BoxWidth_IN,
                    boxHeightIN: shape.BoxHeight_IN,
                    volumeIN3: shape.Volume_IN3,
                    dbConnection: DbConnection);

                if (rowsUpdated > 0)
                {
                    ModalTitle = "Success";
                    ModalContent = $"Updated {rowsUpdated} solid(s) that use the '{Name}' shape.";
                    IsModalOpen = true;
                }
                else
                {
                    ModalTitle = "Info";
                    ModalContent = $"No solids found using the '{Name}' shape, or update completed with no changes.";
                    IsModalOpen = true;
                }
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"An error occurred while updating solids: {ex.Message}";
                IsModalOpen = true;
            }

            // Update ScadObjectViewModel - regenerate scad code from updated polyhedrons
            var scObjectVM = App.Services!.GetRequiredService<ScadObjectViewModel>();
            scObjectVM.GetDimensionsParts(); // Refresh the datagrids first

            if (scObjectVM.SolidDimensions.Any())
            {
                scObjectVM.CreateUnionModule();
                scObjectVM.CreateDifferenceModule();
                scObjectVM.CreateIntersectionModule();
            }
        }

        // Clear all input fields
        public void ClearInputs()
        {
            Name = string.IsNullOrEmpty(Name) ? string.Empty : Name; // Needs to remain since object process can have multiple components
            Description = string.Empty;  // Needed for making sure that parts added don't have the same description
            SelectedPolyhedronOperationType = PolyhedronOperationType.Points;
            PolyhedronConvexity = 1;
            PointYMM = 0;
            PointXMM = 0;
            PointZMM = 0;
            FacePoints = string.Empty;
            UpdatePolyhedronIds(); // When interacting with datagrids, to default to the latest available ids for points and faces
        }

        public void UpdatePolyhedronIds()
        {
            // Update IDs for new entries
            PointsId = PolyhedronDimensionsPoints.Any() ? PolyhedronDimensionsPoints.Max(p => p.PointsId) + 1 : 0;
            FaceId = PolyhedronDimensionsFaces.Any() ? PolyhedronDimensionsFaces.Max(p => p.FaceId) + 1 : 0;
        }

        // Clear all shape fields
        public void ClearShape()
        {
            Name = string.Empty;
            Description = string.Empty;
            ClearInputs(); // Clear input fields
            PolyhedronDimensions = [];
            PolyhedronDimensionsPoints = [];
            PolyhedronDimensionsFaces = [];
            SelectedUnitValue = UnitSystem.Metric;
            GetDimensionPolyhedronParts();
        }

        public void ConvertInputs(int decimalPlaces)
        {
            if (_selectedUnit == UnitSystem.Imperial && UnitHasChanged)
            {
                var (convertedPointX, convertedPointY, convertedPointZ) =
                    ConvertInputsToImperial(
                        _pointXMM,
                        _pointYMM,
                        _pointZMM,
                        decimalPlaces);

                PointXMM = convertedPointX;
                PointYMM = convertedPointY;
                PointZMM = convertedPointZ;

            }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged)
            {
                var (convertedPointX, convertedPointY, convertedPointZ) =
                    ConvertInputsToMetric(
                        _pointXMM,
                        _pointYMM,
                        _pointZMM,
                        decimalPlaces);

                PointXMM = convertedPointX;
                PointYMM = convertedPointY;
                PointZMM = convertedPointZ;
            }

            IsImperial = SelectedUnitValue != UnitSystem.Metric;
            IsMetric = SelectedUnitValue == UnitSystem.Metric;
            UnitHasChanged = false;
        }

        /*** Public Variables ***/
        public bool UnitHasChanged { get => _unitHasChanged; set => this.RaiseAndSetIfChanged(ref _unitHasChanged, value); }
        public bool IsMetric { get => _isMetric; set => this.RaiseAndSetIfChanged(ref _isMetric, value); }
        public bool IsImperial { get => _isImperial; set => this.RaiseAndSetIfChanged(ref _isImperial, value); }
        public List<PolyhedronOperationType> PolyhedronOperationTypes { get; } = new List<PolyhedronOperationType> { PolyhedronOperationType.Points, PolyhedronOperationType.Faces };
        public List<UnitSystem> UnitSystemValues { get; }
        public List<OperationType> OperationTypes { get; }
        public PolyhedronOperationType SelectedPolyhedronOperationType { get => _selectedPolyhedronOperationType; set => this.RaiseAndSetIfChanged(ref _selectedPolyhedronOperationType, value); }
        // Temporary collection for polyhedron dimensions
        public ObservableCollection<PolyhedronDimensions> PolyhedronDimensions
        {
            get => _polyhedronDimensions;
            set
            {
                this.RaiseAndSetIfChanged(ref _polyhedronDimensions, value);
                PolyhedronDimensionsPoints = new ObservableCollection<PolyhedronDimensions>(_polyhedronDimensions.Where(p => p.PolyhedronOperationType == "Points"));
                PolyhedronDimensionsFaces = new ObservableCollection<PolyhedronDimensions>(_polyhedronDimensions.Where(p => p.PolyhedronOperationType == "Faces"));

                // Update IDs for new entries
                UpdatePolyhedronIds();
            }
        }
        public ObservableCollection<PolyhedronDimensions> PolyhedronDimensionsPoints { get => _polyhedronDimensionsPoints; set => this.RaiseAndSetIfChanged(ref _polyhedronDimensionsPoints, value); }
        public ObservableCollection<PolyhedronDimensions> PolyhedronDimensionsFaces { get => _polyhedronDimensionsFaces; set => this.RaiseAndSetIfChanged(ref _polyhedronDimensionsFaces, value); }
        public ObservableCollection<ShapeDimensions> ShapeDimensions { get => _shapeDimensions; set => this.RaiseAndSetIfChanged(ref _shapeDimensions, value); }
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
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
        public FilamentType SelectedFilament { get => _selectedFilament; set => this.RaiseAndSetIfChanged(ref _selectedFilament, value); }
        public double PointXMM { get => _pointXMM; set => this.RaiseAndSetIfChanged(ref _pointXMM, value); }
        public double PointYMM { get => _pointYMM; set => this.RaiseAndSetIfChanged(ref _pointYMM, value); }
        public double PointZMM { get => _pointZMM; set => this.RaiseAndSetIfChanged(ref _pointZMM, value); }
        public int PointsId { get => _pointsId; set => this.RaiseAndSetIfChanged(ref _pointsId, value); }
        public int FaceId { get => _faceId; set => this.RaiseAndSetIfChanged(ref _faceId, value); }
        public int PolyhedronConvexity { get => _polyhedronConvexity; set => this.RaiseAndSetIfChanged(ref _polyhedronConvexity, value); }
        public string FacePoints { get => _facePoints; set => this.RaiseAndSetIfChanged(ref _facePoints, value); }
        public bool IsModalOpen { get => _isModalOpen; set => this.RaiseAndSetIfChanged(ref _isModalOpen, value); }
        public string ModalTitle { get => _modalTitle; set => this.RaiseAndSetIfChanged(ref _modalTitle, value); }
        public string ModalContent { get => _modalContent; set => this.RaiseAndSetIfChanged(ref _modalContent, value); }
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

        public string ShapeScad
        {
            get => _shapeScad;
            set => this.RaiseAndSetIfChanged(ref _shapeScad, value);
        }

        public bool IsAxisMetric
        {
            get => _isAxisMetric;
            set => this.RaiseAndSetIfChanged(ref _isAxisMetric, value);
        }

        public bool IsAxisImperial
        {
            get => _isAxisImperial;
            set => this.RaiseAndSetIfChanged(ref _isAxisImperial, value);
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
                    _axisDimensions = null; // Clear selected axis
                }
            }
        }
        public bool RemoveAxis { get => _removeAxis; set => this.RaiseAndSetIfChanged(ref _removeAxis, value); }
        public string ShapeAxisDisplay { get => _shapeAxisDisplay; set => this.RaiseAndSetIfChanged(ref _shapeAxisDisplay, value); }
        public string ShapeAxisUnitDisplay { get => _shapeAxisUnitDisplay; set => this.RaiseAndSetIfChanged(ref _shapeAxisUnitDisplay, value); }
        public List<string> AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }
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
                    _ = CreateAxis();
            }
        }

        public bool AxisStored
        {
            get => _axisStored;
            set => this.RaiseAndSetIfChanged(ref _axisStored, value);
        }
        public double AxisXPositionMM { get => _axisXPositionMM; set { this.RaiseAndSetIfChanged(ref _axisXPositionMM, value); if (AxisStored && !string.IsNullOrEmpty(ShapeScad)) _ = ShowShapePreviewAsync(); } }
        public double AxisYPositionMM { get => _axisYPositionMM; set { this.RaiseAndSetIfChanged(ref _axisYPositionMM, value); if (AxisStored && !string.IsNullOrEmpty(ShapeScad)) _ = ShowShapePreviewAsync(); } }
        public double AxisZPositionMM { get => _axisZPositionMM; set { this.RaiseAndSetIfChanged(ref _axisZPositionMM, value); if (AxisStored && !string.IsNullOrEmpty(ShapeScad)) _ = ShowShapePreviewAsync(); } }
        public UnitSystem SelectedAxisUnit
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

        // Method to load available axes
        public void GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            _axesModulesList = parser.AxesModulesList(filePath);

            // Call the static function to get filtered axes and updated values
            var (filteredAxes, updatedAxisValue, updatedAxis) = ObjectScadFunctions.GetFilteredAxesList(
                SelectedAxisUnitValue,
                _axesModulesList,
                AxisStored,
                SelectedAxisValue);

            // Update ViewModel properties from the results
            AxesList = [.. filteredAxes];

            if (updatedAxisValue != SelectedAxisValue)
            {
                SelectedAxisValue = updatedAxisValue;
            }

            _selectedAxis = updatedAxis;
        }

        // Method to create axis
        public async Task CreateAxis()
        {
            // Define the callback that updates the ViewModel properties
            CreateAxisCallbackAsync onAxisCreated = async (
                axisId,
                axisDimensions,
                axisXPositionMM,
                axisYPositionMM,
                axisZPositionMM,
                shapeAxisDisplay,
                shapeAxisUnitDisplay,
                selectedUnitValue) =>
            {
                // Update all ViewModel properties via the callback
                _axisId = axisId;
                _axisDimensions = axisDimensions;

                AxisStored = true;
                RemoveAxis = false;
                AxesSelectEnabled = false;

                ShapeAxisDisplay = shapeAxisDisplay;
                ShapeAxisUnitDisplay = shapeAxisUnitDisplay;

                AxisXPositionMM = axisXPositionMM;
                AxisYPositionMM = axisYPositionMM;
                AxisZPositionMM = axisZPositionMM;

                SelectedUnitValue = selectedUnitValue;
            };

            // Call the static function with the callback
            await CreateAxisWithCallbackAsync(
                _selectedAxis,
                _selectedAxisUnit,
                _axisXPositionMM,
                _axisYPositionMM,
                _axisZPositionMM,
                _decimalPlaces,
                _objectFilePath,
                DbConnection!,
                onAxisCreated);
        }

        public async void UpdateAxisTranslate()
        {
            if (!AxisStored) return;

            // Build the original axis call for replacement
            var originalAxisCallLocal = _originalRemoveAxis
                ? $"// translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}"
                : $"translate ([{_AxisXPositionMM}, {_AxisYPositionMM}, {_AxisZPositionMM}]) {_axisDimensions?.OSCADMethod.Replace(_axisDimensions.IncludeMethod, "")}";

            // Define the callback that updates the ViewModel properties
            UpdateAxisTranslateCallbackAsync onAxisTranslateComplete = async (
                axisDimensions,
                newAxisXPositionMM,
                newAxisYPositionMM,
                newAxisZPositionMM,
                newOriginalRemoveAxis) =>
            {
                // Update all ViewModel properties via the callback
                _axisDimensions = axisDimensions;
                _AxisXPositionMM = newAxisXPositionMM;
                _AxisYPositionMM = newAxisYPositionMM;
                _AxisZPositionMM = newAxisZPositionMM;
                _originalRemoveAxis = newOriginalRemoveAxis;
                _originalAxisCall = originalAxisCallLocal;
            };

            // Call the static function with the callback
            await UpdateAxisTranslateWithCallbackAsync(
                AxisStored,
                _axisDimensions,
                _selectedAxis,
                _axisXPositionMM,
                _axisYPositionMM,
                _axisZPositionMM,
                _selectedAxisUnit,
                _decimalPlaces,
                RemoveAxis,
                _originalRemoveAxis,
                originalAxisCallLocal,
                _objectFilePath,
                DbConnection!,
                onAxisTranslateComplete);
        }

        public async Task CreatePolyhedron()
        {
            if (DbConnection == null)
            {
                ModalTitle = "Error";
                ModalContent = "Database connection is not available.";
                IsModalOpen = true;
                return;
            }

            // Ensure only relevant IDs are set based on operation type. -1 indicates not used or null.
            if (SelectedPolyhedronOperationType == PolyhedronOperationType.Points)
            {
                FaceId = -1;
                FacePoints = string.Empty;
            }
            else if (SelectedPolyhedronOperationType == PolyhedronOperationType.Faces)
            {
                PointsId = -1;
                PointXMM = 0;
                PointYMM = 0;
                PointZMM = 0;
            }

            try
            {
                var polyhedronId = await CreatePolyhedronWithCallbackAsync(
                    name: Name,
                    description: Description,
                    polyhedronOperationType: SelectedPolyhedronOperationType.ToString(),
                    pointXMM: PointXMM,
                    pointYMM: PointYMM,
                    pointZMM: PointZMM,
                    pointsId: PointsId,
                    face: FacePoints,
                    faceId: FaceId,
                    selectedUnit: SelectedUnitValue,
                    decimalPlaces: _decimalPlaces,
                    dbConnection: DbConnection,
                    generateOscadCallback: async (polyhedron) =>
                    {
                        // Placeholder - actual SCAD generation happens in onPolyhedronCreated
                        return await Task.FromResult("");
                    },
                    onPolyhedronCreated: async (polyhedronId, updatedPolyhedronDimensions) =>
                    {
                        ClearInputs(); // Clear input fields after creation but before incrementing PointsId/FaceId
                        PolyhedronDimensions = updatedPolyhedronDimensions;
                        
                        // Generate OpenSCAD code for the entire shape with UPDATED dimensions and convexity
                        var scadCode = ShapeScadFunctions.GenerateOSCADShapeAsync(Name, updatedPolyhedronDimensions, PolyhedronConvexity);
                        // Store the generated SCAD code in the property
                        ShapeScad = scadCode;
                        if (_isOpenSCADOpened)
                            await ShowShapePreviewAsync(); // Regenerate scad code

                        // Modal popup disabled - user feedback via DataGrid update and preview opening
                        await Task.CompletedTask;
                    });

                if (polyhedronId == 0)
                {
                    ModalTitle = "Error";
                    ModalContent = "Failed to create polyhedron. Please try again.";
                    IsModalOpen = true;
                }
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"An error occurred while creating the polyhedron: {ex.Message}";
                IsModalOpen = true;
            }
        }

        /**** Dimensions DataGrids ****/
        public async void GetDimensionPolyhedronParts()
        {
            // Call the static function to retrieve polyhedron dimensions
            var polyhedronDimensions = await GetDimensionPolyhedronPartsAsync(DbConnection!,Name);

            // Update ObservableCollection
            PolyhedronDimensions = polyhedronDimensions;

            // Retrieve the saved shape from ShapeDimensions if it exists
            await GetShapeDimensionsAsync();
        }

        public void ShowOSCADMethod(PolyhedronDimensions polyhedron)
        {
            ModalTitle = "OSCAD Method";
            ModalContent = polyhedron.OSCADMethod ?? "No OSCAD method available";
            IsModalOpen = true;
        }

        public async Task DeleteSelectedItemAsync(PolyhedronDimensions polyhedron)
        {
            try
            {
                if (DbConnection == null)
                {
                    ModalTitle = "Error";
                    ModalContent = "Database connection is not available.";
                    IsModalOpen = true;
                    return;
                }

                // Delete the polyhedron from the database
                await polyhedron.DeleteAsync(DbConnection);

                // Refresh the collection
                GetDimensionPolyhedronParts();
                var scadCode = ShapeScadFunctions.GenerateOSCADShapeAsync(Name, PolyhedronDimensions);
                ShapeScad = scadCode;
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"An error occurred while deleting the polyhedron: {ex.Message}";
                IsModalOpen = true;
            }
        }

        public void ShowShapeScadCode()
        {
            if (string.IsNullOrWhiteSpace(ShapeScad))
            {
                ModalTitle = "No Code Generated";
                ModalContent = "No OpenSCAD code has been generated yet. Create points and faces first.";
                IsModalOpen = true;
                return;
            }

            ModalTitle = "Shape OpenSCAD Code";
            ModalContent = ShapeScad;
            IsModalOpen = true;
        }

        public async Task ShowShapePreviewAsync()
        {
            if (string.IsNullOrWhiteSpace(ShapeScad))
            {
                ModalTitle = "No Preview Available";
                ModalContent = "No OpenSCAD code has been generated yet. Create points and faces first.";
                IsModalOpen = false;
                return;
            }

            try
            {
                var previewFilePath = await ShapeScadFunctions.ShapeToScadPreviewAsync(
                    Name,
                    ShapeScad,
                    _objectFilePath,
                    _selectedAxisUnit,
                    PolyhedronDimensions,
                    AxisStored ? _axisDimensions : null,
                    AxisStored ? AxisXPositionMM : null,
                    AxisStored ? AxisYPositionMM : null,
                    AxisStored ? AxisZPositionMM : null);


                if (string.IsNullOrWhiteSpace(previewFilePath))
                {
                    ModalTitle = "Error";
                    ModalContent = "Failed to generate preview file.";
                    IsModalOpen = false;
                    return;
                }

                if (!_isOpenSCADOpened)
                {
                    await ShapeScadFunctions.OpenShapePreviewAsync(previewFilePath, allowDuplicates: false);
                    _isOpenSCADOpened = true;
                }
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"Failed to open preview: {ex.Message}";
                IsModalOpen = false;
            }
        }

        /// <summary>
        /// Retrieves the ShapeDimensions record that matches the current shape name
        /// </summary>
        private async Task GetShapeDimensionsAsync()
        {
            try
            {
                if (DbConnection == null || string.IsNullOrWhiteSpace(Name))
                {
                    ShapeDimensions = [];
                    return;
                }

                var shapeDimensions = await ShapeScadFunctions.GetShapeDimensionsByNameAsync(
                    shapeName: Name,
                    dbConnection: DbConnection);

                ShapeDimensions = new ObservableCollection<ShapeDimensions>(shapeDimensions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving shape dimensions: {ex.Message}");
                ShapeDimensions = [];
            }
        }

        public void ShowShapeOSCADMethod(ShapeDimensions shape)
        {
            ModalTitle = "Shape OpenSCAD Method";
            ModalContent = shape.OSCADMethod ?? "No OSCAD method available";
            IsModalOpen = true;
        }

        public async Task DeleteShapeAsync(ShapeDimensions shape)
        {
            try
            {
                if (DbConnection == null)
                {
                    ModalTitle = "Error";
                    ModalContent = "Database connection is not available.";
                    IsModalOpen = true;
                    return;
                }

                // Delete the shape from the database
                await shape.DeleteAsync(DbConnection);

                ModalTitle = "Success";
                ModalContent = $"Shape '{shape.Name}' deleted successfully.";
                IsModalOpen = true;

                // Refresh the shape dimensions collection
                await GetShapeDimensionsAsync();
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"An error occurred while deleting the shape: {ex.Message}";
                IsModalOpen = true;
            }
        }
    }
}
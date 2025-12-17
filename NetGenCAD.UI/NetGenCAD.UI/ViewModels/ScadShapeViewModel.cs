using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
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
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static NetGenCAD.Core.Measurements.Selector;
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
        private string _shapeScad = string.Empty;


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
            DbConnection = App.Services!.GetRequiredService<SqliteConnection>(); // Get the DbConnection from the DI container
            ClearShape();
            _decimalPlaces = NetGenCAD.Designer.Repositories.PolyhedronDimensions.OpenSCAD_DecimalPlaces;
            UnitSystemValues = [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
            SelectedUnitValue = UnitSystem.Metric;
            _objectFilePath = App.Services!.GetRequiredService<IScadPathProvider>().ScadPath;
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

        private void UpdatePolyhedronIds()
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
                FaceId = -1;
            else if (SelectedPolyhedronOperationType == PolyhedronOperationType.Faces)
                PointsId = -1;

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
                    convexity: PolyhedronConvexity,
                    selectedUnit: SelectedUnitValue,
                    decimalPlaces: _decimalPlaces,
                    dbConnection: DbConnection,
                    generateOscadCallback: async (polyhedron) =>
                    {
                        // Placeholder - actual SCAD generation happens in onPolyhedronCreated
                        return await Task.FromResult("// OSCAD method placeholder");
                    },
                    onPolyhedronCreated: async (polyhedronId, updatedPolyhedronDimensions) =>
                    {
                        ClearInputs(); // Clear input fields after creation but before incrementing PointsId/FaceId
                        PolyhedronDimensions = updatedPolyhedronDimensions;
                        
                        // Generate OpenSCAD code for the entire shape with UPDATED dimensions and convexity
                        var scadCode = ShapeScadFunctions.GenerateOSCADShapeAsync(Name, updatedPolyhedronDimensions, PolyhedronConvexity);
                        // Store the generated SCAD code in the property
                        ShapeScad = scadCode;
                        
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
            var polyhedronDimensions = await GetDimensionPolyhedronPartsAsync(
                DbConnection!,
                Name);

            // Update ObservableCollection
            PolyhedronDimensions = polyhedronDimensions;
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

                ModalTitle = "Success";
                ModalContent = $"Polyhedron '{polyhedron.Description}' deleted successfully.";
                    
                // Refresh the collection
                GetDimensionPolyhedronParts();

                IsModalOpen = true;
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
                IsModalOpen = true;
                return;
            }

            try
            {
                // Generate the preview file with convexity parameter
                var previewFilePath = await ShapeScadFunctions.ShapeToScadPreviewAsync(Name, ShapeScad, _objectFilePath);
                
                if (string.IsNullOrWhiteSpace(previewFilePath))
                {
                    ModalTitle = "Error";
                    ModalContent = "Failed to generate preview file.";
                    IsModalOpen = true;
                    return;
                }

                // Open the preview in OpenSCAD (allowDuplicates: false prevents opening if already open)
                await ShapeScadFunctions.OpenShapePreviewAsync(previewFilePath, allowDuplicates: false);
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalContent = $"Failed to open preview: {ex.Message}";
                IsModalOpen = true;
            }
        }
    }
}
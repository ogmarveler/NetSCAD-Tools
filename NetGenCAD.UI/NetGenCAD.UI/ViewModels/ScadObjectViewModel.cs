using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Core.Interfaces;
using NetGenCAD.Core.Material;
using NetGenCAD.Core.Measurements;
using NetGenCAD.Core.Models;
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
using System.Threading.Tasks;
using static NetGenCAD.Core.Measurements.Colors;
using static NetGenCAD.Core.Measurements.Selector;
using static NetGenCAD.Designer.Functions.ObjectScadFunctions;

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
        private bool _isPolyhedronSelected = false;
        private bool _copyObject = false;
        private ObservableCollection<ModuleDimensions>? _cachedAllModuleDimensions;
        private ObservableCollection<ShapeDimensions>? _availablePolyhedrons;
        private ShapeDimensions? _selectedPolyhedron;
        private bool _isColorFromHex = false;
        private string _openScadColorHex = string.Empty;
        private string _textInput = string.Empty;
        private double _textSize = 12.0;
        private string _fontInput = string.Empty;
        private string _textAlign = "left";
        private string _verticalAlign = "top";
        private string _textDirection = "ltr";
        private string _languageInput = string.Empty;
        private bool _isTextSelected = false;


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

        public async void GetPolyhedronsList() =>
            // Get the current list of polyhedrons available to be used in solids
            AvailablePolyhedrons = new ObservableCollection<ShapeDimensions>(await ShapeDimensionsExtensions.GetShapesList(DbConnection!));

        /**** Dimensions DataGrids ****/
        public async void GetDimensionsParts()
        {
            // Call the static function to retrieve dimensions
            var (solidDimensions, moduleDimensions) = await GetDimensionsPartsAsync(
                DbConnection!,
                Name);

            // Update ObservableCollections
            ModuleDimensions = moduleDimensions;
            SolidDimensions = solidDimensions;
            GetPolyhedronsList();

            // Update UI selection buttons to reflect current state
            IsCubeSelected = _isCubeSelected;
            IsRoundCubeSelected = _isRoundCubeSelected;
            IsCylinderSelected = _isCylinderSelected;
            IsRoundCylinderSelected = _isRoundCylinderSelected;
            IsSphereSelected = _isSphereSelected;
            IsSurfaceSelected = _isSurfaceSelected;
            IsPolyhedronSelected = _isPolyhedronSelected;
            IsTextSelected = _isTextSelected;
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
            SelectedPolyhedron = null;
            SurfaceFilePath = string.Empty;
            AutoSmoothFile = false;
            SurfaceCenter = false;
            SurfaceConvexity = 1;
            SurfaceInvert = false;
            SurfaceScaleX = 1;
            SurfaceScaleY = 1;
            SurfaceScaleZ = 1;
            XOffsetMM = 0;
            YOffsetMM = 0;
            ZOffsetMM = 0;
            XRotate = 0;
            YRotate = 0;
            ZRotate = 0;
            LayerIntValue = 0;
            AlphaIntValue = 1;
            IsCubeSelected = false;
            IsRoundCubeSelected = false;
            IsCylinderSelected = false;
            IsSurfaceSelected = false;
            IsRoundSurfaceSelected = false;
            IsSphereSelected = false;
            IsRoundCylinderSelected = false;
            IsPolyhedronSelected = false;
            IsTextSelected = false;
            SelectedOpenSCADColor = OpenScadColor.Silver;
            OpenSCADColorHex = string.Empty;
            TextInput = string.Empty;
            TextSize = 12.0;
            FontInput = string.Empty;
            TextAlign = "left";
            VerticalAlign = "top";
            TextDirection = "ltr";
            LanguageInput = string.Empty;
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
            IsPreRendered = false;
            CopyObject = false;
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
            // Validate solid type selection
            if (string.IsNullOrEmpty(SelectedSolidType) || SelectedSolidType == "Select Solid")
                return 0;

            // Ensure axis is created if needed
            if (_axisId is null)
                await CreateAxis();

            // Define the callback for generating OSCAD (delegates to existing logic)
            Func<SolidDimensions, Task<string>> generateOscadCallback = async (solidDim) =>
            {
                return await GenerateOSCADAsync(
                    solidDim,
                    IsCubeSelected,
                    IsRoundCubeSelected,
                    IsSurfaceSelected,
                    IsRoundSurfaceSelected,
                    IsCylinderSelected,
                    IsRoundCylinderSelected,
                    IsSphereSelected,
                    IsTextSelected,
                    SelectedUnitValue,
                    _decimalPlaces,
                    SurfaceScaleX,
                    SurfaceScaleY,
                    SurfaceScaleZ,
                    SurfaceFilePath,
                    SurfaceInvert,
                    SurfaceCenter,
                    SurfaceConvexity,
                    IsPreRendered,
                    TextInput,
                TextSize,
                FontInput,
                TextAlign,
                VerticalAlign,
                TextDirection
                    );
            };

            // Define the callback for refreshing dimensions
            Func<Task> refreshDimensionsCallback = async () =>
            {
                GetDimensionsParts();
                await Task.CompletedTask;
            };

            // Define the callback that updates the ViewModel properties
            CreateObjectAsyncCallbackAsync onObjectCreated = async (
                solidId,
                newAppendObject,
                updatedSolidDimensions,
                objectAxisDisplay) =>
            {
                // Update all ViewModel properties via the callback
                AppendObject = newAppendObject;
                SolidDimensions = updatedSolidDimensions;

                // Handle module creation based on operation type
                switch (SelectedOperationType)
                {
                    case OperationType.Union:
                        CreateDifferenceModule(); // Union module created within DifferenceModule for proper layering
                        break;
                    case OperationType.Difference:
                        CreateDifferenceModule();
                        break;
                    case OperationType.Intersection:
                        CreateIntersectionModule();
                        break;
                }

                // Clear inputs for next object
                ClearInputs();
                // Final refresh
                GetDimensionsParts();
            };

            // Call the static function with all callbacks and polyhedron parameters
            var result = await CreateObjectWithCallbackAsync(
                SelectedSolidType,
                Name,
                Description,
                SelectedFilament,
                SelectedOperationType,
                LengthMM,
                WidthMM,
                HeightMM,
                ThicknessMM,
                RadiusMM,
                Radius1MM,
                Radius2MM,
                CylinderHeightMM,
                XOffsetMM,
                YOffsetMM,
                ZOffsetMM,
                XRotate,
                YRotate,
                ZRotate,
                SelectedOpenSCADColor,
                LayerIntValue,
                AlphaIntValue,
                SurfaceFilePath,
                SurfaceCenter ? 1 : 0,
                SurfaceInvert ? 1 : 0,
                IsCubeSelected,
                IsRoundCubeSelected,
                IsSurfaceSelected,
                IsRoundSurfaceSelected,
                IsCylinderSelected,
                IsRoundCylinderSelected,
                IsSphereSelected,
                IsPolyhedronSelected,
                IsTextSelected,
                SelectedPolyhedron,
                SelectedUnitValue,
                _decimalPlaces,
                _axisId,
                AxesSelectEnabled,
                AppendObject,
                DbConnection!,
                generateOscadCallback,
                refreshDimensionsCallback,
                _axisDimensions?.OSCADMethod ?? string.Empty,
                onObjectCreated,
                IsColorFromHex,
                OpenSCADColorHex,
                SurfaceScaleX,
                SurfaceScaleY,
                SurfaceScaleZ,
                TextInput,
                TextSize,
                FontInput,
                TextAlign,
                VerticalAlign,
                TextDirection
            );

            return result;
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

        public async Task CreateAxis()
        {
            // Define the callback that updates the ViewModel properties
            CreateAxisCallbackAsync onAxisCreated = async (
                axisId,
                axisDimensions,
                axisXPositionMM,
                axisYPositionMM,
                axisZPositionMM,
                objectAxisDisplay,
                objectAxisUnitDisplay,
                selectedUnitValue) =>
            {
                // Update all ViewModel properties via the callback
                _axisId = axisId;
                _axisDimensions = axisDimensions;

                AxisStored = true;
                RemoveAxis = false;
                AxesSelectEnabled = false;
                GetDimensionsParts();

                ObjectAxisDisplay = objectAxisDisplay;
                ObjectAxisUnitDisplay = objectAxisUnitDisplay;

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

        public async Task PartsToScadFilesAsync()
        {
            // Get latest data from DB
            GetDimensionsParts();

            // Call the static function
            await ObjectScadFunctions.PartsToScadFilesAsync(
                Name,
                Description,
                _objectFilePath,
                DbConnection!,
                SolidDimensions,
                ModuleDimensions);

            // Refresh the DataGrid
            GetDimensionsParts();
        }

        /// <summary>
        /// Copies all object data (solids, modules, axis) from a source object to a new object.
        /// Uses the ObjectScadFunctions wrapper which performs database copy off the UI thread,
        /// retrieves dimensions from the database, and updates the ViewModel via callback.
        /// </summary>
        public async Task CopyObjectAsync()
        {
            try
            {
                var currentObjectName = _solidDimensions.FirstOrDefault()?.Name ?? string.Empty;
                if (string.IsNullOrEmpty(currentObjectName))
                    return;

                // Call the wrapper function in ObjectScadFunctions
                var (sDim, mDim) = await CopyObjectWithCallbackAsync(currentObjectName, DbConnection!);

                    ClearObject(); // Clear out old object
                    // Update ViewModel collections with refreshed data
                    SolidDimensions = sDim;
                    ModuleDimensions = mDim;
                    // Update the Name property to the new object name
                    Name = _solidDimensions.FirstOrDefault()?.Name ?? string.Empty;
                CopyObject = false; // Reset the CopyObject flag
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying object: {ex.Message}");
            }
        }

        // Deletes the selected item from any DataGrid (Cube, Cylinder, or Module)
        public async Task DeleteSelectedItemAsync(object? selectedItem)
        {
            if (selectedItem == null) return;

            // Call the static deletion function
            await ObjectScadFunctions.DeleteSelectedItemAsync(selectedItem, DbConnection!);

            // Remove from appropriate collection based on type
            switch (selectedItem)
            {
                case SolidDimensions solid:
                    SolidDimensions.Remove(solid);
                    break;

                case ModuleDimensions module:
                    ModuleDimensions.Remove(module);
                    break;
            }

            // Refresh the DataGrids and files after successful deletion
            await PartsToScadFilesAsync();  // Only update parts file
        }

        public void UpdateObjectMirror()
        {
            // Call the static function from ObjectScadFunctions
            ObjectScadFunctions.UpdateObjectMirror(
                XMirror,
                YMirror,
                ZMirror,
                _XMirror,
                _YMirror,
                _ZMirror,
                _objectFilePath);

            // Update the internal tracking fields
            _XMirror = XMirror;
            _YMirror = YMirror;
            _ZMirror = ZMirror;
        }

        public async Task ObjectToScadFilesAsync()
        {
            // Get any additional updates to parts
            await PartsToScadFilesAsync();

            // Clear current object, reapply axis
            await CreateAxis();

            // Generate and write the object SCAD file
            await ObjectScadFunctions.ObjectToScadFilesAsync(
                _objectFilePath,
                ModuleDimensions,
                XMirror,
                YMirror,
                ZMirror);

            // Handle STL export if enabled
            if (ExportToStl)
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
            await ExportToStlAsync(
                _objectFilePath,
                tempRemoveAxis);

            // Then restore axis if needed
            RemoveAxis = tempRemoveAxis;
            UpdateAxisTranslate();

            // Set ExportToStl back to false
            ExportToStl = false;
        }

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
                var (length, width, height, thickness, radius, radius1, radius2, cylinderHeight, xOffset, yOffset, zOffset) =
                    ConvertInputsToImperial(
                        _lengthMM, _widthMM, _heightMM, _thicknessMM,
                        _radiusMM, _radius1MM, _radius2MM, _cylinderHeightMM,
                        _xOffsetMM, _yOffsetMM, _zOffsetMM,
                        decimalPlaces);

                LengthMM = length;
                WidthMM = width;
                HeightMM = height;
                ThicknessMM = thickness;
                RadiusMM = radius;
                Radius1MM = radius1;
                Radius2MM = radius2;
                CylinderHeightMM = cylinderHeight;
                XOffsetMM = xOffset;
                YOffsetMM = yOffset;
                ZOffsetMM = zOffset;
            }
            else if (_selectedUnit == UnitSystem.Metric && UnitHasChanged)
            {
                var (length, width, height, thickness, radius, radius1, radius2, cylinderHeight, xOffset, yOffset, zOffset) =
                    ConvertInputsToMetric(
                        _lengthMM, _widthMM, _heightMM, _thicknessMM,
                        _radiusMM, _radius1MM, _radius2MM, _cylinderHeightMM,
                        _xOffsetMM, _yOffsetMM, _zOffsetMM,
                        decimalPlaces);

                LengthMM = length;
                WidthMM = width;
                HeightMM = height;
                ThicknessMM = thickness;
                RadiusMM = radius;
                Radius1MM = radius1;
                Radius2MM = radius2;
                CylinderHeightMM = cylinderHeight;
                XOffsetMM = xOffset;
                YOffsetMM = yOffset;
                ZOffsetMM = zOffset;
            }
            
            IsImperial = SelectedUnitValue != UnitSystem.Metric;
            IsMetric = SelectedUnitValue == UnitSystem.Metric;
            UnitHasChanged = false;
        }

        /**** Axes List DataGrid ****/
        public void GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            _axesModulesList = parser.AxesModulesList(filePath);

            // Call the static function to get filtered axes and updated values
            var (filteredAxes, updatedAxisValue, updatedAxis) = GetFilteredAxesList(
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

        public async void CreateUnionModule()
        {
            await CreateUnionModuleAsync(
                Name,
                Description,
                SolidDimensions,
                ModuleDimensions,
                DbConnection!,
                _isPreRendered);

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        public async void CreateDifferenceModule()
        {
            await CreateDifferenceModuleAsync(
                Name,
                Description,
                SolidDimensions,
                ModuleDimensions,
                DbConnection!,
                _isPreRendered);

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        public async void CreateIntersectionModule()
        {
            await CreateIntersectionModuleAsync(
                Name,
                Description,
                SolidDimensions,
                ModuleDimensions,
                DbConnection!,
                _isPreRendered);

            GetDimensionsParts(); // Refresh the datagrids
            await PartsToScadFilesAsync(); // Only update parts file
        }

        private void UpdateScrewRadiusFromSelection()
        {
            if (IsCylinderSelected || IsRoundCylinderSelected)
            {
                RadiusMM = ObjectScadFunctions.CalculateScrewRadius(
                    SelectedScrewSize,
                    SelectedScrewProperty,
                    SelectedUnitValue,
                    _decimalPlaces);
            }
        }

        // Add this method to update dimensions when server rack is selected
        private void UpdateServerRackDimensionsFromSelection()
        {
            // Call the static function to calculate dimensions
            var (updatedWidth, updatedHeight) = ObjectScadFunctions.CalculateServerRackDimensions(
                IsCubeSelected,
                IsRoundCubeSelected,
                SelectedServerRackWidthType,
                SelectedServerRack,
                SelectedUnitValue,
                _decimalPlaces);

            // Apply calculated dimensions to UI properties
            if (updatedWidth > 0)
                WidthMM = updatedWidth;

            if (updatedHeight > 0)
                HeightMM = updatedHeight;
        }

        private void UpdateViewButtons()
        {
            var (saveFile, difference, union, intersection) = ObjectScadFunctions.CalculateButtonStates(
                ModuleDimensions,
                SolidDimensions);

            SaveFileButton = saveFile;
            DifferenceButton = difference;
            UnionButton = union;
            IntersectionButton = intersection;
        }

        public void LoadPngDimensions(string filePath)
        {
            // Call the static function to get calculated PNG dimensions
            var (length, width, height, xOffset, _, updatedFilePath) = ObjectScadFunctions.LoadPngDimensionsData(
                filePath,
                AutoSmoothFile,
                _selectedUnit,
                _decimalPlaces,
                SurfaceScaleX,
                SurfaceScaleY,
                SurfaceScaleZ,
                SurfaceInvert);

            // Image width and height are equivalent to width and length axis by default, 
            // so we rotate for user to interpret width and height as the same context
            XRotate = XRotate != 0 ? 90 : XRotate;
            ZRotate = ZRotate != 0 ? 90 : ZRotate;

            // Set textboxes for UI
            LengthMM = length;
            WidthMM = width;
            HeightMM = height;
            SurfaceFilePath = updatedFilePath;  // return the file path if it has changed in smoothing
            XOffsetMM = xOffset;
        }

        // Method to show OSCAD methods
        public void ShowOSCADMethods(ModuleDimensions module)
        {
            var solids = SolidDimensions.Where(s => s.ModuleDimensionsId == module.Id).ToList();

            if (solids.Any())
            {
                var (title, content) = BuildOscadMethodsModal(module, solids);

                ModalTitle = title;
                ModalContent = content;
                IsModalOpen = true;
            }
        }

        /// <summary>
        /// Updates the solid color in the database and regenerates with the new color.
        /// /// <param name="solidId">The ID of the solid to update</param>
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

                // Regenerate the SCAD content with the new color
                solid.OSCADMethod = RegenerateSolidWithColor(solid.OSCADMethod, color);

                // Update the solid in the database with new OSCAD method
                await solid.UpdateAsync(DbConnection!);

                // Refresh the parts file
                await PartsToScadFilesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating solid color: {ex.Message}");
            }
        }

        /*** Public Variables ***/
        public string TextInput { get => _textInput; set => this.RaiseAndSetIfChanged(ref _textInput, value); }
        public string TextAlign { get => _textAlign; set => this.RaiseAndSetIfChanged(ref _textAlign, value); }
        public double TextSize { get => _textSize; set => this.RaiseAndSetIfChanged(ref _textSize, value); }
        public string TextDirection { get => _textDirection; set => this.RaiseAndSetIfChanged(ref _textDirection, value); }
        public string FontInput { get => _fontInput; set => this.RaiseAndSetIfChanged(ref _fontInput, value); }
        public string LanguageInput { get => _languageInput; set => this.RaiseAndSetIfChanged(ref _languageInput, value); }
        public string VerticalAlign { get => _verticalAlign; set => this.RaiseAndSetIfChanged(ref _verticalAlign, value); }
        public string SurfaceFilePath { get => _surfaceFilePath; set => this.RaiseAndSetIfChanged(ref _surfaceFilePath, value); }
        public bool SurfaceCenter { get => _surfaceCenter; set => this.RaiseAndSetIfChanged(ref _surfaceCenter, value); }
        public bool AutoSmoothFile { get => _autoSmoothFile; set => this.RaiseAndSetIfChanged(ref _autoSmoothFile, value); }
        public int SurfaceConvexity { get => _surfaceConvexity; set => this.RaiseAndSetIfChanged(ref _surfaceConvexity, value); }
        public bool SurfaceInvert { get => _surfaceInvert; set => this.RaiseAndSetIfChanged(ref _surfaceInvert, value); }
        public double SurfaceScaleX { get => _surfaceScaleX; set { if (value > 0.0) this.RaiseAndSetIfChanged(ref _surfaceScaleX, value); } }
        public double SurfaceScaleY { get => _surfaceScaleY; set { if (value > 0.0) this.RaiseAndSetIfChanged(ref _surfaceScaleY, value); } }
        public double SurfaceScaleZ { get => _surfaceScaleZ; set { if (value > 0.0) this.RaiseAndSetIfChanged(ref _surfaceScaleZ, value); } }
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
        public bool CopyObject
        {
            get => _copyObject;
            set
            {
                this.RaiseAndSetIfChanged(ref _copyObject, value);
                if (_copyObject)
                    _ = CopyObjectAsync();
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
        public string OpenSCADColorHex { get => _openScadColorHex; set => this.RaiseAndSetIfChanged(ref _openScadColorHex, value); }
        public bool AxisStored { get => _axisStored; set => this.RaiseAndSetIfChanged(ref _axisStored, value); }
        public bool AppendObject { get => _appendObject; set => this.RaiseAndSetIfChanged(ref _appendObject, value); }
        public bool UnionButton { get => _unionButton; set => this.RaiseAndSetIfChanged(ref _unionButton, value); }
        public bool IntersectionButton { get => _intersectionButton; set => this.RaiseAndSetIfChanged(ref _intersectionButton, value); }
        public bool DifferenceButton { get => _differenceButton; set => this.RaiseAndSetIfChanged(ref _differenceButton, value); }
        public bool SaveFileButton { get => _saveFileButton; set => this.RaiseAndSetIfChanged(ref _saveFileButton, value); }
        public bool IsColorFromHex { get => _isColorFromHex; set => this.RaiseAndSetIfChanged(ref _isColorFromHex, value); }
        public List<FilamentType> FilamentTypes { get; }
        public List<UnitSystem> UnitSystemValues { get; }
        public List<string>? AxesList { get => _axesList; set => this.RaiseAndSetIfChanged(ref _axesList, value); }
        public List<OperationType> OperationTypes { get; }
        public OperationType SelectedOperationType { get => _selectedOperationType; set => this.RaiseAndSetIfChanged(ref _selectedOperationType, value); }
        public ShapeDimensions? SelectedPolyhedron { get => _selectedPolyhedron; set => this.RaiseAndSetIfChanged(ref _selectedPolyhedron, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsUnions { get => _moduleDimensionsUnions; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsUnions, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsIntersections { get => _moduleDimensionsIntersections; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsIntersections, value); }
        public ObservableCollection<ModuleDimensions> ModuleDimensionsDifferences { get => _moduleDimensionsDifferences; set => this.RaiseAndSetIfChanged(ref _moduleDimensionsDifferences, value); }
        public ObservableCollection<SolidDimensions> SolidDimensions { get => _solidDimensions; set => this.RaiseAndSetIfChanged(ref _solidDimensions, value); }
        public ObservableCollection<ModuleDimensions> LayeredModuleDimensions { get => _layeredModuleDimensions; set => this.RaiseAndSetIfChanged(ref _layeredModuleDimensions, value); }
        public ObservableCollection<ShapeDimensions>? AvailablePolyhedrons { get => _availablePolyhedrons; set => this.RaiseAndSetIfChanged(ref _availablePolyhedrons, value); }
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
        public bool IsTextSelected
        {
            get => _isTextSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isTextSelected, value);
                if (_isTextSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    _isPolyhedronSelected = false;
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
                }
            }
        }
        public bool IsPolyhedronSelected
        {
            get => _isPolyhedronSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isPolyhedronSelected, value);
                if (_isPolyhedronSelected)
                {
                    _isCubeSelected = false;
                    _isRoundCubeSelected = false;
                    _isCylinderSelected = false;
                    _isSurfaceSelected = false;
                    _isRoundSurfaceSelected = false;
                    _isSphereSelected = false;
                    _isRoundCylinderSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsCubeSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCylinderSelected));
                    this.RaisePropertyChanged(nameof(IsSphereSelected));
                    this.RaisePropertyChanged(nameof(IsSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundSurfaceSelected));
                    this.RaisePropertyChanged(nameof(IsRoundCubeSelected));
                    this.RaisePropertyChanged(nameof(IsCylinderSelected));
                }
            }
        }
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _isPolyhedronSelected = false;
                    _isTextSelected = false;
                    this.RaisePropertyChanged(nameof(IsTextSelected));
                    this.RaisePropertyChanged(nameof(IsPolyhedronSelected));
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
                    _ = CreateAxis();
            }
        }

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
                    case "Polyhedron":
                        IsPolyhedronSelected = true;
                        break;
                    case "Sphere":
                        IsSphereSelected = true;
                        break;
                    case "Surface":
                        IsSurfaceSelected = true;
                        break;
                    case "Text":
                        IsTextSelected = true;
                        break;
                }
            }
        }
    }
}
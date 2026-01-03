using NetGenCAD.Core.Interfaces;
using NetGenCAD.Core.Primitives;
using NetGenCAD.Core.Utility;
using NetGenCAD.Designer.Repositories;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Text;
using static NetGenCAD.Core.Measurements.Colors;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;
using static NetGenCAD.Core.Utility.WrapInModule;
using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Core.Material;
using NetGenCAD.Designer.Utility;
using NetGenCAD.Core.Measurements;
using NetGenCAD.Core.Models;

namespace NetGenCAD.Designer.Functions
{
    /// <summary>
    /// Static utility class for ObjectScadViewModel business logic operations.
    /// All methods are static and accept parameters, returning modified data for ViewModel binding.
    /// </summary>
    public static class ObjectScadFunctions
    {
        // Add this delegate type at the top of the ObjectScadFunctions class
        public delegate Task CreateAxisCallbackAsync(
            int? axisId,
            AxisDimensions axisDimensions,
            double axisXPositionMM,
            double axisYPositionMM,
            double axisZPositionMM,
            string objectAxisDisplay,
            string objectAxisUnitDisplay,
            UnitSystem selectedUnitValue);

        // Replace the CreateAxisAsync method with callback version
        public static async Task CreateAxisWithCallbackAsync(
            GeneratedModule? selectedAxis,
            UnitSystem selectedAxisUnit,
            double axisXPositionMM,
            double axisYPositionMM,
            double axisZPositionMM,
            int decimalPlaces,
            string objectFilePath,
            SqliteConnection dbConnection,
            CreateAxisCallbackAsync onAxisCreated)
        {
            if (selectedAxis == null)
                return;

            var axisDimensions = new AxisDimensions
            {
                Theme = selectedAxis.Theme!,
                OSCADMethod = selectedAxis.CallingMethod!,
                Unit = selectedAxis.Unit!,
                MinX = selectedAxis.MinX,
                MaxX = selectedAxis.MaxX,
                MinY = selectedAxis.MinY,
                MaxY = selectedAxis.MaxY,
                MinZ = selectedAxis.MinZ,
                MaxZ = selectedAxis.MaxZ,
                CreatedAt = DateTime.UtcNow,
            };

            axisDimensions.OSCADMethod = $"{axisDimensions.IncludeMethod} {selectedAxis.CallingMethod}";
            var axisId = await axisDimensions.UpsertAsync(dbConnection);

            if (axisId != null)
            {
                // Convert offsets to metric if imperial
                if (selectedAxisUnit == UnitSystem.Imperial)
                {
                    axisXPositionMM = Math.Round(InchesToMillimeter(axisXPositionMM), decimalPlaces);
                    axisYPositionMM = Math.Round(InchesToMillimeter(axisYPositionMM), decimalPlaces);
                    axisZPositionMM = Math.Round(InchesToMillimeter(axisZPositionMM), decimalPlaces);
                }

                // Build the wrapped axis call
                var wrappedAxisCall = $"translate ([{axisXPositionMM}, {axisYPositionMM}, {axisZPositionMM}]) {axisDimensions?.OSCADMethod.Replace(axisDimensions.IncludeMethod, "")}";

                // Build SCAD content
                var sb = new StringBuilder();
                sb.AppendLine("// Custom axis");
                sb.AppendLine(axisDimensions?.IncludeMethod);
                sb.AppendLine(wrappedAxisCall);
                sb.AppendLine();

                // Write to file
                await Output.WriteToSCAD(
                    content: sb.ToString(),
                    filePath: Path.Combine(objectFilePath, "Solids", "object.scad"),
                    overWrite: true,
                    cancellationToken: new CancellationToken());

                // Format display values
                var objectAxisDisplay = StringFunctions.FormatAxisDisplay(axisDimensions?.OSCADMethod);
                var objectAxisUnitDisplay = axisDimensions?.Unit == "mm" ? "Metric (mm)" : "Imperial (in)";
                var selectedUnitValue = axisDimensions!.Unit == "mm" ? UnitSystem.Metric : UnitSystem.Imperial;

                // Invoke callback with updated values
                await onAxisCreated(
                    axisId,
                    axisDimensions,
                    axisDimensions!.MinX,
                    axisDimensions!.MinY,
                    axisDimensions!.MinZ,
                    objectAxisDisplay,
                    objectAxisUnitDisplay,
                    selectedUnitValue);
            }
        }

        // Add this delegate type for UpdateAxisTranslate callback
        public delegate Task UpdateAxisTranslateCallbackAsync(
            AxisDimensions axisDimensions,
            double axisXPositionMM,
            double axisYPositionMM,
            double axisZPositionMM,
            bool originalRemoveAxis);

        // Add this static method to ObjectScadFunctions
        public static async Task UpdateAxisTranslateWithCallbackAsync(
            bool axisStored,
            AxisDimensions? axisDimensions,
            GeneratedModule? selectedAxis,
            double axisXPositionMM,
            double axisYPositionMM,
            double axisZPositionMM,
            UnitSystem selectedAxisUnit,
            int decimalPlaces,
            bool removeAxis,
            bool originalRemoveAxis,
            string originalAxisCall,
            string objectFilePath,
            SqliteConnection dbConnection,
            UpdateAxisTranslateCallbackAsync onAxisTranslateComplete)
        {
            if (!axisStored) return; // No axis has been applied yet

            try
            {
                // Create new AxisDimensions object
                var updatedAxisDimensions = new AxisDimensions
                {
                    Theme = selectedAxis?.Theme!,
                    OSCADMethod = selectedAxis?.CallingMethod!,
                    Unit = selectedAxis?.Unit!,
                    MinX = selectedAxis!.MinX,
                    MaxX = selectedAxis!.MaxX,
                    MinY = selectedAxis!.MinY,
                    MaxY = selectedAxis!.MaxY,
                    MinZ = selectedAxis!.MinZ,
                    MaxZ = selectedAxis!.MaxZ,
                    CreatedAt = DateTime.UtcNow,
                };
                updatedAxisDimensions.OSCADMethod = $"{updatedAxisDimensions.IncludeMethod} {selectedAxis.CallingMethod}";
                var axisId = await updatedAxisDimensions.UpsertAsync(dbConnection);

                // Convert offsets if imperial
                var newAxisXPositionMM = axisXPositionMM;
                var newAxisYPositionMM = axisYPositionMM;
                var newAxisZPositionMM = axisZPositionMM;

                if (selectedAxisUnit == UnitSystem.Imperial)
                {
                    newAxisXPositionMM = Math.Round(InchesToMillimeter(axisXPositionMM), decimalPlaces);
                    newAxisYPositionMM = Math.Round(InchesToMillimeter(axisYPositionMM), decimalPlaces);
                    newAxisZPositionMM = Math.Round(InchesToMillimeter(axisZPositionMM), decimalPlaces);
                }

                // Build the file path
                var filePath = Path.Combine(objectFilePath, "Solids", "object.scad");
                if (!File.Exists(filePath)) return;

                // Build the wrapped axis call
                var wrappedAxisCall = removeAxis
                    ? $"// translate ([{newAxisXPositionMM}, {newAxisYPositionMM}, {newAxisZPositionMM}]) {updatedAxisDimensions?.OSCADMethod.Replace(updatedAxisDimensions.IncludeMethod, "")}"
                    : $"translate ([{newAxisXPositionMM}, {newAxisYPositionMM}, {newAxisZPositionMM}]) {updatedAxisDimensions?.OSCADMethod.Replace(updatedAxisDimensions.IncludeMethod, "")}";

                // Update the file
                UpdateFIle.ChangeContentBlockFile(
                    oldCodeBlock: originalAxisCall,
                    newCodeBlock: wrappedAxisCall,
                    filePath: filePath);

                // Invoke callback with updated values
                await onAxisTranslateComplete(
                    updatedAxisDimensions,
                    newAxisXPositionMM,
                    newAxisYPositionMM,
                    newAxisZPositionMM,
                    removeAxis);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating axis translate in object.scad file: " + ex.Message);
            }
        }

        /// <summary>
        /// Extracts the inner content from an OpenSCAD module declaration.
        /// Removes the module header and outer braces, returning only the module body.
        /// </summary>
        /// <param name="moduleContent">The full module declaration including header and braces</param>
        /// <returns>The module body content without the module declaration or outer braces</returns>
        public static string ExtractModuleBody(string moduleContent)
        {
            if (string.IsNullOrWhiteSpace(moduleContent))
                return string.Empty;

            var trimmed = moduleContent.Trim();

            // Find the opening brace of the module body
            int openingBraceIndex = trimmed.IndexOf('{');
            if (openingBraceIndex == -1)
                return trimmed; // No opening brace found, return as-is

            // Find the matching closing brace
            int closingBraceIndex = FindMatchingCloseBrace(trimmed, openingBraceIndex);
            if (closingBraceIndex == -1)
                return trimmed; // No matching closing brace found, return as-is

            // Extract content between braces and trim
            string moduleBody = trimmed.Substring(openingBraceIndex + 1, closingBraceIndex - openingBraceIndex - 1).Trim();

            return moduleBody;
        }

        // Add this delegate type for CreateObjectAsync callback
        public delegate Task CreateObjectAsyncCallbackAsync(
            int solidId,
            bool appendObject,
            ObservableCollection<SolidDimensions> updatedSolidDimensions,
            string objectAxisDisplay);

        // Add this static method to ObjectScadFunctions
        public static async Task<int> CreateObjectWithCallbackAsync(
            string selectedSolidType,
            string name,
            string description,
            FilamentType selectedFilament,
            OperationType selectedOperationType,
            double lengthMM,
            double widthMM,
            double heightMM,
            double thicknessMM,
            double radiusMM,
            double radius1MM,
            double radius2MM,
            double cylinderHeightMM,
            double xOffsetMM,
            double yOffsetMM,
            double zOffsetMM,
            double xRotate,
            double yRotate,
            double zRotate,
            OpenScadColor selectedOpenScadColor,
            int layerIntValue,
            double alphaIntValue,
            string surfaceFilePath,
            int surfaceCenter,
            int surfaceInvert,
            bool isCubeSelected,
            bool isRoundCubeSelected,
            bool isSurfaceSelected,
            bool isRoundSurfaceSelected,
            bool isCylinderSelected,
            bool isRoundCylinderSelected,
            bool isSphereSelected,
            bool isPolyhedronSelected,
            bool isTextSelected,
            ShapeDimensions? selectedPolyhedron,
            UnitSystem selectedUnit,
            int decimalPlaces,
            int? axisId,
            bool axesSelectEnabled,
            bool appendObject,
            SqliteConnection dbConnection,
            Func<SolidDimensions, Task<string>> generateOscadCallback,
            Func<Task> refreshDimensionsCallback,
            string currentAxisDisplay,
            CreateObjectAsyncCallbackAsync onObjectCreated,
            bool isColorFromHex,
            string openScadColorHex,
            double surfaceScaleX,
            double surfaceScaleY,
            double surfaceScaleZ,
            string textInput,
            double textSize,
            string fontInput,
            string textAlign,
            string verticalAlign,
            string textDirection)
        {
            try
            {
                // Determine the color to use: if hex is selected and not blank, use hex; otherwise use selected color
                var colorToUse = isColorFromHex && !string.IsNullOrEmpty(openScadColorHex)
                    ? openScadColorHex
                    : selectedOpenScadColor.ToString();

                if (isTextSelected)
                {
                    if (textSize <= 0)
                        return 0;

                    // Calculate text metrics here if needed
                    // Use internal heightmaps for portable fonts
                    var (width, height) = CalculateTextBounds(textInput, fontInput, textSize);

                    // Factors in tables based on 10 pt font. Adjust based on this.
                    //Formulas for adjustments
                    double factoredLength = 0.0;
                    double factoredWidth = 0.0;
                    double factoredHeight = 1.0;

                    switch(textDirection)
                    {
                        // Without rotation, for ltr, width is along X axis, height along Y axis
                        case "L-to-R":
                            factoredLength = width;
                            factoredWidth = height;
                            factoredHeight = 1.0;
                            break;
                        case "R-to-L":
                            factoredLength = width;
                            factoredWidth = height;
                            factoredHeight = 1.0;
                            break;
                        case "T-to-B":
                            factoredWidth = width;
                            factoredLength = height;
                            factoredHeight = 1.0;
                            break;
                        case "B-to-T":
                            factoredWidth = width;
                            factoredLength = height;
                            factoredHeight = 1.0;
                            break;
                    };

                    var newObject = new SolidDimensions
                    {
                        Name = name,
                        Description = description,
                        Material = selectedFilament.ToString(),
                        OperationType = selectedOperationType.ToString(),
                        SolidType = "Text",
                        Length_MM = RoundFloatingPoints(factoredLength),
                        Width_MM = RoundFloatingPoints(factoredWidth),
                        Height_MM = factoredHeight,
                        Thickness_MM = 0,
                        Radius_MM = 0,
                        Radius1_MM = 0,
                        Radius2_MM = 0,
                        CylinderHeight_MM = 0,
                        XOffset_MM = xOffsetMM,
                        YOffset_MM = yOffsetMM,
                        ZOffset_MM = zOffsetMM,
                        XRotate = xRotate,
                        YRotate = yRotate,
                        ZRotate = zRotate,
                        ScaleX = surfaceScaleX,
                        ScaleY = surfaceScaleY,
                        ScaleZ = surfaceScaleZ,
                        CreatedAt = DateTime.UtcNow,
                        AxisDimensionsId = axisId,
                        SurfaceCenter = 0,
                        SurfaceInvert = 0,
                        SurfaceFilePath = string.Empty,
                        ModuleColor = colorToUse,
                        Layer = layerIntValue,
                        Alpha = alphaIntValue,
                        TextContent = textInput,
                        TextSize = textSize,
                        FontStyle = fontInput,
                        TextHAlign = textAlign,
                        TextVAlign = verticalAlign,
                        TextDirection = textDirection
                    };

                    // Generate OSCAD method via callback
                    newObject.OSCADMethod = await generateOscadCallback(newObject);

                    // Save to database
                    await newObject.UpsertAsync(dbConnection);

                    // Refresh dimensions from database
                    await refreshDimensionsCallback();

                    // Get current solid dimensions for axis display
                    var textDimensions = await new SolidDimensions().GetByNameWithAxisAndModuleAsync(dbConnection, name);

                    // Use the provided currentAxisDisplay if axis exists, otherwise query from database
                    var axisDisplayText = string.Empty;
                    if (axesSelectEnabled && textDimensions.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(currentAxisDisplay))
                        {
                            axisDisplayText = currentAxisDisplay;
                        }
                        else
                        {
                            var axisUsed = textDimensions.SingleOrDefault()?.AxisOSCADMethod;
                            axisDisplayText = StringFunctions.FormatAxisDisplay(axisUsed);
                        }
                    }

                    // Invoke callback with updated values
                    await onObjectCreated(
                        newObject.Id,
                        true,
                        new ObservableCollection<SolidDimensions>(textDimensions),
                        axisDisplayText);

                    return newObject.Id;
                }

                // Handle Polyhedron case first - if selected, skip all other solid type processing
                if (isPolyhedronSelected && selectedPolyhedron != null)
                {
                    // For offsets. Box dimensions handled in Create Shape module
                    if (selectedUnit == UnitSystem.Imperial)
                    {
                        xOffsetMM = Math.Round(InchesToMillimeter(xOffsetMM), decimalPlaces);
                        yOffsetMM = Math.Round(InchesToMillimeter(yOffsetMM), decimalPlaces);
                        zOffsetMM = Math.Round(InchesToMillimeter(zOffsetMM), decimalPlaces);
                    }

                    // Create new SolidDimensions instance from polyhedron
                    var newObject = new SolidDimensions
                    {
                        Name = name,
                        Description = description,
                        Material = selectedFilament.ToString(),
                        OperationType = selectedOperationType.ToString(),
                        SolidType = "Polyhedron",
                        Length_MM = selectedPolyhedron.BoxLength_MM,
                        Width_MM = selectedPolyhedron.BoxWidth_MM,
                        Height_MM = selectedPolyhedron.BoxHeight_MM,
                        Thickness_MM = 0, // Not applicable for polyhedrons
                        Radius_MM = 0,
                        Radius1_MM = 0,
                        Radius2_MM = 0,
                        CylinderHeight_MM = 0,
                        XOffset_MM = xOffsetMM,
                        YOffset_MM = yOffsetMM,
                        ZOffset_MM = zOffsetMM,
                        XRotate = xRotate,
                        YRotate = yRotate,
                        ZRotate = zRotate,
                        ScaleX = surfaceScaleX,
                        ScaleY = surfaceScaleY,
                        ScaleZ = surfaceScaleZ,
                        CreatedAt = DateTime.UtcNow,
                        AxisDimensionsId = axisId,
                        SurfaceCenter = 0,
                        SurfaceInvert = 0,
                        SurfaceFilePath = string.Empty,
                        ModuleColor = colorToUse,
                        Layer = layerIntValue,
                        Alpha = alphaIntValue,
                        ShapeName = selectedPolyhedron.Name, // Map shape name
                        OSCADMethod = selectedPolyhedron.OSCADMethod
                    };

                    // Build the OSCAD method with translate, rotate, and color
                    // Apply rotation if needed
                    string moduleBody = ExtractModuleBody(newObject.OSCADMethod);
                    var rotatedMethod = $"rotate([{xRotate}, {yRotate}, {zRotate}]) {{{moduleBody}}}";

                    // Apply translation if needed
                    string translatedMethod = rotatedMethod;
                    translatedMethod = $"translate([{xOffsetMM}, {yOffsetMM}, {zOffsetMM}]) {{{rotatedMethod}}}";

                    // Wrap in module
                    newObject.OSCADMethod = ToModule(
                        translatedMethod,
                        newObject.Name,
                        newObject.Description!,
                        newObject.OperationType,
                        newObject.SolidType.ToLower(),
                        newObject.ModuleColor.ToLower(),
                        newObject.Alpha).Trim();

                    // Save to database
                    await newObject.UpsertAsync(dbConnection);

                    // Refresh dimensions from database
                    await refreshDimensionsCallback();

                    // Get current solid dimensions for axis display
                    var allSolids = await new SolidDimensions().GetByNameWithAxisAndModuleAsync(dbConnection, name);

                    // Use the provided currentAxisDisplay if axis exists, otherwise query from database
                    var objectAxisDisplay = string.Empty;
                    if (axesSelectEnabled && allSolids.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(currentAxisDisplay))
                        {
                            objectAxisDisplay = currentAxisDisplay;
                        }
                        else
                        {
                            var axisUsed = allSolids.SingleOrDefault()?.AxisOSCADMethod;
                            objectAxisDisplay = StringFunctions.FormatAxisDisplay(axisUsed);
                        }
                    }

                    // Invoke callback with updated values
                    await onObjectCreated(
                        newObject.Id,
                        true,
                        new ObservableCollection<SolidDimensions>(allSolids),
                        objectAxisDisplay);

                    return newObject.Id;
                }

                // Validate solid type selection for standard solids
                var solidType = selectedSolidType switch
                {
                    "Cube" => "Cube",
                    "Round Cube" => "Round Cube",
                    "Cylinder" => "Cylinder",
                    "Round Cylinder" => "Round Cylinder",
                    "Sphere" => "Sphere",
                    "Surface" => "Surface",
                    "Text" => "Text",
                    _ => null
                };

                if (solidType == null)
                    return 0;

                // Create new SolidDimensions instance for standard solids
                var solidObject = new SolidDimensions
                {
                    Name = name,
                    Description = description,
                    Material = selectedFilament.ToString(),
                    OperationType = selectedOperationType.ToString(),
                    SolidType = solidType,
                    Length_MM = lengthMM,
                    Width_MM = widthMM,
                    Height_MM = heightMM,
                    Thickness_MM = thicknessMM,
                    Radius_MM = radiusMM,
                    Radius1_MM = radius1MM,
                    Radius2_MM = radius2MM,
                    CylinderHeight_MM = cylinderHeightMM,
                    XOffset_MM = xOffsetMM,
                    YOffset_MM = yOffsetMM,
                    ZOffset_MM = zOffsetMM,
                    XRotate = xRotate,
                    YRotate = yRotate,
                    ZRotate = zRotate,
                    ScaleX = surfaceScaleX,
                    ScaleY = surfaceScaleY,
                    ScaleZ = surfaceScaleZ,
                    CreatedAt = DateTime.UtcNow,
                    AxisDimensionsId = axisId,
                    SurfaceCenter = surfaceCenter,
                    SurfaceInvert = surfaceInvert,
                    SurfaceFilePath = surfaceFilePath,
                    ModuleColor = colorToUse,
                    Layer = layerIntValue,
                    Alpha = alphaIntValue,
                };

                // Generate OSCAD method via callback
                solidObject.OSCADMethod = await generateOscadCallback(solidObject);

                // Save to database
                await solidObject.UpsertAsync(dbConnection);

                // Refresh dimensions from database
                await refreshDimensionsCallback();

                // Get current solid dimensions for axis display
                var solidDimensions = await new SolidDimensions().GetByNameWithAxisAndModuleAsync(dbConnection, name);

                // Use the provided currentAxisDisplay if axis exists, otherwise query from database
                var axisDisplay = string.Empty;
                if (axesSelectEnabled && solidDimensions.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(currentAxisDisplay))
                    {
                        axisDisplay = currentAxisDisplay;
                    }
                    else
                    {
                        var axisUsed = solidDimensions.SingleOrDefault()?.AxisOSCADMethod;
                        axisDisplay = StringFunctions.FormatAxisDisplay(axisUsed);
                    }
                }

                // Invoke callback with updated values
                await onObjectCreated(
                    solidObject.Id,
                    true,
                    new ObservableCollection<SolidDimensions>(solidDimensions),
                    axisDisplay);

                return solidObject.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating object: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Applies rotation transformation to an OpenSCAD object.
        /// Returns the original object if all rotation values are zero.
        /// </summary>
        /// <param name="scadObject">The IScadObject to rotate</param>
        /// <param name="xRotate">Rotation around X-axis in degrees</param>
        /// <param name="yRotate">Rotation around Y-axis in degrees</param>
        /// <param name="zRotate">Rotation around Z-axis in degrees</param>
        /// <returns>Rotated IScadObject or original if no rotation needed</returns>
        public static Task<IScadObject> GetRotate(IScadObject scadObject, double xRotate, double yRotate, double zRotate)
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

        public static Task<IScadObject> GetTranslate(
            IScadObject scadObject,
            double xOffsetMM,
            double yOffsetMM,
            double zOffsetMM,
            bool isCubeSelected,
            bool isRoundCubeSelected,
            bool isSurfaceSelected,
            bool isRoundSurfaceSelected,
            bool isCylinderSelected,
            bool isSphereSelected,
            bool isRoundCylinderSelected,
            double lengthMM,
            double widthMM,
            double heightMM,
            double thicknessMM,
            double radiusMM,
            double radius1MM,
            double radius2MM,
            double cylinderHeightMM,
            OperationType selectedOperationType,
            UnitSystem selectedUnit,
            int decimalPlaces)
        {
            if (isCubeSelected || isRoundCubeSelected || isSurfaceSelected || isRoundSurfaceSelected)
            {
                var oDim = new SolidDimensions
                {
                    Length_MM = lengthMM,
                    Width_MM = widthMM,
                    Height_MM = heightMM,
                    Thickness_MM = thicknessMM,
                };

                if (isRoundCubeSelected || isRoundSurfaceSelected)
                {
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), decimalPlaces);

                    switch (selectedOperationType)
                    {
                        case OperationType.Union:
                            xOffsetMM += oDim.Round_r_MM;
                            yOffsetMM += oDim.Round_r_MM;
                            zOffsetMM += -oDim.Round_h_MM;
                            break;
                        case OperationType.Difference:
                            xOffsetMM += oDim.Round_r_MM + oDim.Thickness_MM;
                            yOffsetMM += oDim.Round_r_MM + oDim.Thickness_MM;
                            zOffsetMM += -oDim.Round_h_MM + oDim.Thickness_MM;
                            break;
                        case OperationType.Intersection:
                            xOffsetMM += oDim.Round_r_MM + oDim.Thickness_MM;
                            yOffsetMM += oDim.Round_r_MM + oDim.Thickness_MM;
                            zOffsetMM += -oDim.Round_h_MM + oDim.Thickness_MM;
                            break;
                    }
                }
                else if (isCubeSelected || isSurfaceSelected)
                {
                    switch (selectedOperationType)
                    {
                        case OperationType.Difference:
                            xOffsetMM += oDim.Thickness_MM;
                            yOffsetMM += oDim.Thickness_MM;
                            zOffsetMM += oDim.Thickness_MM;
                            break;
                        case OperationType.Intersection:
                            xOffsetMM += oDim.Thickness_MM;
                            yOffsetMM += oDim.Thickness_MM;
                            zOffsetMM += oDim.Thickness_MM;
                            break;
                    }
                }
            }
            else if (isCylinderSelected || isSphereSelected || isRoundCylinderSelected)
            {
                var oDim = new SolidDimensions
                {
                    Radius_MM = radiusMM,
                    Radius1_MM = radius1MM,
                    Radius2_MM = radius2MM,
                    CylinderHeight_MM = cylinderHeightMM,
                };

                if (isRoundCylinderSelected)
                {
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.CylinderHeight_MM), decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), decimalPlaces);
                    zOffsetMM += oDim.Round_r_MM;
                }
            }

            var translateParams = new Dictionary<string, object>
            {
                { "x", xOffsetMM },
                { "y", yOffsetMM },
                { "z", zOffsetMM },
                { "children", new IScadObject[] { scadObject } }
            };
            var translate = OScadTransform.Translate.ToScadObject(translateParams);
            return Task.FromResult(translate);
        }

        /// <summary>
        /// Generates OpenSCAD OSCAD code for a solid based on its dimensions and properties.
        /// Handles all solid types: Cube, RoundCube, Surface, RoundSurface, Cylinder, RoundCylinder, Sphere.
        /// Converts dimensions from Imperial to Metric if needed, applies rotation and translation transformations.
        /// </summary>
        public static async Task<string> GenerateOSCADAsync(
            SolidDimensions oDim,
            bool isCubeSelected,
            bool isRoundCubeSelected,
            bool isSurfaceSelected,
            bool isRoundSurfaceSelected,
            bool isCylinderSelected,
            bool isRoundCylinderSelected,
            bool isSphereSelected,
            bool isTextSelected,
            UnitSystem selectedUnit,
            int decimalPlaces,
            double surfaceScaleX,
            double surfaceScaleY,
            double surfaceScaleZ,
            string surfaceFilePath,
            bool surfaceInvert,
            bool surfaceCenter,
            int surfaceConvexity,
            bool isPreRendered,
            string textInput,
            double textSize,
            string fontInput,
            string textAlign,
            string verticalAlign,
            string textDirection
            )
        {
            if (isCubeSelected || isRoundCubeSelected || isSurfaceSelected || isRoundSurfaceSelected)
            {
                if (selectedUnit == UnitSystem.Imperial)
                {
                    // Convert dimensions to metric for OpenSCAD
                    oDim.Length_MM = Math.Round(InchesToMillimeter(oDim.Length_MM), decimalPlaces);
                    oDim.Width_MM = Math.Round(InchesToMillimeter(oDim.Width_MM), decimalPlaces);
                    oDim.Height_MM = Math.Round(InchesToMillimeter(oDim.Height_MM), decimalPlaces);
                    oDim.Thickness_MM = Math.Round(InchesToMillimeter(oDim.Thickness_MM), decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), decimalPlaces);
                }

                if (isRoundCubeSelected)
                {
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), decimalPlaces);

                    var roundedCubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim.Length_MM }, { "size_y", oDim.Width_MM }, { "size_z", oDim.Height_MM }, { "round_r", oDim.Round_r_MM }, { "round_h", oDim.Round_h_MM }, { "resolution", oDim.Resolution }
                    };
                    var roundedCube = OScad3D.RoundedCube.ToScadObject(roundedCubeParams);
                    var rotated = await GetRotate(roundedCube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                    var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: oDim.Length_MM, widthMM: oDim.Width_MM, heightMM: oDim.Height_MM, thicknessMM: oDim.Thickness_MM, radiusMM: 0, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
                else if (isCubeSelected)
                {
                    var cubeParams = new Dictionary<string, object>
                    {
                        { "size_x", oDim.Length_MM }, { "size_y", oDim.Width_MM }, { "size_z", oDim.Height_MM },
                    };
                    var cube = OScad3D.Cube.ToScadObject(cubeParams);
                    var rotated = await GetRotate(cube, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                    var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: oDim.Length_MM, widthMM: oDim.Width_MM, heightMM: oDim.Height_MM, thicknessMM: oDim.Thickness_MM, radiusMM: 0, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
                else if (isSurfaceSelected)
                {
                    var surfaceParams = new Dictionary<string, object>
                    {
                        { "file", $"{surfaceFilePath.Replace("\\", "/")}" },
                        { "scaleX", surfaceScaleX },
                        { "scaleY", surfaceScaleY },
                        { "scaleZ", surfaceScaleZ },
                        { "invert", surfaceInvert },
                        { "center", surfaceCenter },
                        { "convexity", surfaceConvexity }
                    };
                    var surface = OScad3D.Surface.ToScadObject(surfaceParams);
                    var rotated = await GetRotate(surface, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                    var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: oDim.Length_MM, widthMM: oDim.Width_MM, heightMM: oDim.Height_MM, thicknessMM: oDim.Thickness_MM, radiusMM: 0, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
                else if (isRoundSurfaceSelected)
                {
                    oDim.Round_r_MM = Math.Round(RoundFromWidth(oDim.Width_MM), decimalPlaces);
                    oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), decimalPlaces);

                    var surfaceParams = new Dictionary<string, object>
                    {
                        { "file", $"{surfaceFilePath.Replace("\\", "/")}" },
                        { "scaleX", surfaceScaleX },
                        { "scaleY", surfaceScaleY },
                        { "scaleZ", surfaceScaleZ },
                        { "invert", surfaceInvert },
                        { "center", surfaceCenter },
                        { "convexity", surfaceConvexity },
                        { "round_r", oDim.Round_r_MM },
                        { "round_h", oDim.Round_h_MM },
                        { "resolution", oDim.Resolution },
                    };
                    var roundSurface = OScad3D.RoundedSurface.ToScadObject(surfaceParams);
                    var rotated = await GetRotate(roundSurface, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                    var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: oDim.Length_MM, widthMM: oDim.Width_MM, heightMM: oDim.Height_MM, thicknessMM: oDim.Thickness_MM, radiusMM: 0, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                    return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
                }
            }
            else if (isCylinderSelected)
            {
                if (selectedUnit == UnitSystem.Imperial)
                {
                    oDim.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), decimalPlaces);
                }

                var cylParams = new Dictionary<string, object>
                {
                    { "r", oDim.Radius_MM },
                    { "r1", oDim.Radius1_MM },
                    { "r2", oDim.Radius2_MM },
                    { "h", oDim.CylinderHeight_MM },
                    { "resolution", 360 }
                };
                var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
                var rotated = await GetRotate(cylinder, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: 0, widthMM: 0, heightMM: 0, thicknessMM: 0, radiusMM: oDim.Radius_MM, radius1MM: oDim.Radius1_MM, radius2MM: oDim.Radius2_MM, cylinderHeightMM: oDim.CylinderHeight_MM, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }
            else if (isRoundCylinderSelected)
            {
                if (selectedUnit == UnitSystem.Imperial)
                {
                    oDim.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), decimalPlaces);
                    oDim.Radius1_MM = Math.Round(InchesToMillimeter(oDim.Radius1_MM), decimalPlaces);
                    oDim.Radius2_MM = Math.Round(InchesToMillimeter(oDim.Radius2_MM), decimalPlaces);
                    oDim.CylinderHeight_MM = Math.Round(InchesToMillimeter(oDim.CylinderHeight_MM), decimalPlaces);
                    oDim.Round_r_MM = Math.Round(InchesToMillimeter(oDim.Round_r_MM), decimalPlaces);
                    oDim.Round_h_MM = Math.Round(InchesToMillimeter(oDim.Round_h_MM), decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), decimalPlaces);
                }

                oDim.Round_r_MM = Math.Round(oDim.Round_r_MM > 0 ? oDim.Round_r_MM : RoundFromWidth(oDim.CylinderHeight_MM), decimalPlaces);
                oDim.Round_h_MM = Math.Round(RoundEdgeHeight(oDim.Round_r_MM), decimalPlaces);

                var roundedCylParams = new Dictionary<string, object>
                {
                    { "r", oDim.Radius_MM },
                    { "r1", oDim.Radius1_MM },
                    { "r2", oDim.Radius2_MM },
                    { "h", oDim.CylinderHeight_MM },
                    { "round_r", oDim.Round_r_MM },
                    { "round_h", oDim.Round_h_MM },
                    { "resolution", oDim.Resolution }
                };
                var roundedCylinder = OScad3D.RoundedCylinder.ToScadObject(roundedCylParams);
                var rotated = await GetRotate(roundedCylinder, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: 0, widthMM: 0, heightMM: 0, thicknessMM: 0, radiusMM: oDim.Radius_MM, radius1MM: oDim.Radius1_MM, radius2MM: oDim.Radius2_MM, cylinderHeightMM: oDim.CylinderHeight_MM, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }
            else if (isSphereSelected)
            {
                if (selectedUnit == UnitSystem.Imperial)
                {
                    oDim.Radius_MM = Math.Round(InchesToMillimeter(oDim.Radius_MM), decimalPlaces);
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), decimalPlaces);
                }

                var sphereParams = new Dictionary<string, object>
                {
                    { "r", oDim.Radius_MM },
                    { "resolution", 360 }
                };
                var sphere = OScad3D.Sphere.ToScadObject(sphereParams);
                var rotated = await GetRotate(sphere, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: 0, widthMM: 0, heightMM: 0, thicknessMM: 0, radiusMM: oDim.Radius_MM, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }
            else if (isTextSelected)
            {
                if (selectedUnit == UnitSystem.Imperial)
                {
                    oDim.XOffset_MM = Math.Round(InchesToMillimeter(oDim.XOffset_MM), decimalPlaces);
                    oDim.YOffset_MM = Math.Round(InchesToMillimeter(oDim.YOffset_MM), decimalPlaces);
                    oDim.ZOffset_MM = Math.Round(InchesToMillimeter(oDim.ZOffset_MM), decimalPlaces);
                }
                var textParams = new Dictionary<string, object>
                {
                    { "text", textInput },
                    { "size", textSize },
                    { "font", fontInput },
                    { "halign", textAlign },
                    { "valign", verticalAlign },
                    { "direction", textDirection },
                    { "convexity", 10 }
                };
                var textObj = OScadSpecial.Text.ToScadObject(textParams);
                var rotated = await GetRotate(textObj, oDim.XRotate, oDim.YRotate, oDim.ZRotate);
                var translate = await GetTranslate(scadObject: rotated, xOffsetMM: oDim.XOffset_MM, yOffsetMM: oDim.YOffset_MM, zOffsetMM: oDim.ZOffset_MM, isCubeSelected: isCubeSelected, isRoundCubeSelected: isRoundCubeSelected, isSurfaceSelected: isSurfaceSelected, isRoundSurfaceSelected: isRoundSurfaceSelected, isCylinderSelected: isCylinderSelected, isSphereSelected: isSphereSelected, isRoundCylinderSelected: isRoundCylinderSelected, lengthMM: 0, widthMM: 0, heightMM: 0, thicknessMM: 0, radiusMM: 0, radius1MM: 0, radius2MM: 0, cylinderHeightMM: 0, selectedOperationType: (OperationType)Enum.Parse(typeof(OperationType), oDim.OperationType), selectedUnit: selectedUnit, decimalPlaces: decimalPlaces);
                return ToModule(translate.OSCADMethod, oDim.Name, oDim.Description!, oDim.OperationType, oDim.SolidType.ToLower(), oDim.ModuleColor.ToLower(), oDim.Alpha).Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// Updates the mirror values in the object.scad file.
        /// Replaces the old mirror call block with the new one containing updated X, Y, Z values.
        /// </summary>
        /// <param name="xMirror">X mirror value</param>
        /// <param name="yMirror">Y mirror value</param>
        /// <param name="zMirror">Z mirror value</param>
        /// <param name="originalXMirror">Previous X mirror value for finding the old block</param>
        /// <param name="originalYMirror">Previous Y mirror value for finding the old block</param>
        /// <param name="originalZMirror">Previous Z mirror value for finding the old block</param>
        /// <param name="objectFilePath">Path to the object directory</param>
        public static void UpdateObjectMirror(
            double xMirror,
            double yMirror,
            double zMirror,
            double originalXMirror,
            double originalYMirror,
            double originalZMirror,
            string objectFilePath)
        {
            try
            {
                var filePath = Path.Combine(objectFilePath, "Solids", "object.scad");
                if (!File.Exists(filePath)) return;

                // Build the old mirror call using original values
                var originalMirrorCall = $"mirror([{originalXMirror}, {originalYMirror}, {originalZMirror}]) ";

                // Build the new mirror call with updated values
                var wrappedMirrorCall = $"mirror([{xMirror}, {yMirror}, {zMirror}]) ";

                // Replace the old block with the new one
                UpdateFIle.ChangeContentBlockFile(
                    oldCodeBlock: originalMirrorCall,
                    newCodeBlock: wrappedMirrorCall,
                    filePath: filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating mirror in object.scad file: " + ex.Message);
            }
        }

        /// <summary>
        /// Generates and writes parts SCAD file content based on solid and module dimensions.
        /// Updates the include method in the database and writes formatted SCAD code to file.
        /// </summary>
        /// <param name="objectName">Name of the object (used for filename)</param>
        /// <param name="objectDescription">Description of the object</param>
        /// <param name="objectFilePath">Root path where Solids folder exists</param>
        /// <param name="dbConnection">Database connection for updates</param>
        /// <param name="solidDimensions">Collection of solid dimensions to include</param>
        /// <param name="moduleDimensions">Collection of module dimensions to include</param>
        /// <returns>Task representing the async file write operation</returns>
        public static async Task PartsToScadFilesAsync(
            string objectName,
            string objectDescription,
            string objectFilePath,
            SqliteConnection dbConnection,
            IEnumerable<SolidDimensions> solidDimensions,
            IEnumerable<ModuleDimensions> moduleDimensions)
        {
            var sbpart = new StringBuilder();
            var fileName = $"{objectName.Replace(" ", "_").Trim().ToLower()}.scad";
            var moduleIncludeMethod = $"include <{fileName}>;";

            // Update ModuleDimensions OSCADMethod in DB
            var moduleUpdate = new ModuleDimensions
            {
                ObjectDescription = objectDescription,
                ObjectName = objectName,
                SolidType = "Object",
                IncludeMethod = moduleIncludeMethod,
                CreatedAt = DateTime.UtcNow
            };
            await moduleUpdate.UpdateIncludeMethodByNameDescriptionSolidTypeAsync(dbConnection);

            // Parts file creation header
            sbpart.AppendLine($"//Use in main file: {moduleIncludeMethod}");
            sbpart.AppendLine();

            // Add all solids
            foreach (var solid in solidDimensions)
            {
                sbpart.AppendLine($"// {solid.Name} - Solid Type: {solid.SolidType}, Description: {solid.Description}, Operation Type: {solid.OperationType}");
                sbpart.AppendLine(solid.OSCADMethod);
                sbpart.AppendLine();
            }

            // Add all object-type modules
            foreach (var module in moduleDimensions.Where(m => m.SolidType == "Object"))
            {
                sbpart.AppendLine($"// {module.ObjectName} - Type: {module.ModuleType}");
                sbpart.AppendLine(module.OSCADMethod);
                sbpart.AppendLine();
            }

            // Filter modules by type
            var objDDim = moduleDimensions.Where(x => x.SolidType == "Object" && x.ModuleType == "Difference");
            var objUDim = moduleDimensions.Where(x => x.SolidType == "Object" && x.ModuleType == "Union");
            var objIDim = moduleDimensions.Where(x => x.SolidType == "Object" && x.ModuleType == "Intersection");

            // Add calling method comments based on hierarchy
            if (objDDim.Any())
            {
                foreach (var module in objDDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
            else if (objUDim.Any())
            {
                foreach (var module in objUDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }
            else if (objIDim.Any())
            {
                foreach (var module in objIDim)
                {
                    sbpart.AppendLine($"// Calling method to use in your object.scad file");
                    sbpart.AppendLine($"// {module.Name}");
                }
            }

            // Write to part file
            await Output.WriteToSCAD(
                content: sbpart.ToString(),
                filePath: Path.Combine(objectFilePath, "Solids", fileName),
                overWrite: true,
                cancellationToken: new CancellationToken());
        }

        /// <summary>
        /// Generates the complete object SCAD file by combining parts, axis, modules, and mirror operations.
        /// Orchestrates parts generation, axis creation, and layered module assembly.
        /// </summary>
        /// <param name="objectFilePath">Root path where Solids folder exists</param>
        /// <param name="moduleDimensions">Collection of module dimensions to assemble</param>
        /// <param name="xMirror">X mirror value</param>
        /// <param name="yMirror">Y mirror value</param>
        /// <param name="zMirror">Z mirror value</param>
        /// <returns>Task representing the async file operations</returns>
        public static async Task ObjectToScadFilesAsync(
            string objectFilePath,
            IEnumerable<ModuleDimensions> moduleDimensions,
            double xMirror,
            double yMirror,
            double zMirror)
        {
            // Put Scad object file together
            var sb = new StringBuilder();
            sb.AppendLine("// Solid components used in main object");

            if (moduleDimensions.Any())
            {
                foreach (string includeMethod in moduleDimensions.Select(y => y.IncludeMethod).Distinct().ToList())
                {
                    sb.AppendLine(includeMethod);  // Include parts
                }
            }

            sb.AppendLine(); // Calling methods below
            sb.AppendLine($"difference() {{");
            sb.AppendLine($"    mirror([{xMirror}, {yMirror}, {zMirror}]) ");
            sb.AppendLine($"    union() {{");

            // Get the maximum layer value
            if (moduleDimensions.Any())
            {
                int maxLayer = moduleDimensions.Max(m => m.Layer);

                // Iterate through each layer from 0 to maxLayer
                for (int currentLayer = 0; currentLayer <= maxLayer; currentLayer++)
                {
                    // Get all modules for the current layer
                    var modulesForLayer = moduleDimensions
                        .Where(m => m.Layer == currentLayer)
                        .ToList();

                    // Intersections take precedence over differences and unions
                    var moduleIntersections = modulesForLayer
                        .Where(m => m.ModuleType == OperationType.Intersection.ToString())
                        .ToList();

                    var moduleDifferences = modulesForLayer
                        .Where(m => m.ModuleType == OperationType.Difference.ToString())
                        .ToList();

                    var moduleUnions = modulesForLayer
                        .Where(m => m.ModuleType == OperationType.Union.ToString())
                        .ToList();

                    if (moduleIntersections.Any())
                    {
                        foreach (ModuleDimensions module in moduleIntersections)
                        {
                            sb.Append($"        "); // Formatting
                            sb.AppendLine(module.Name);
                        }
                    }
                    else if (moduleDifferences.Any())
                    {
                        foreach (ModuleDimensions module in moduleDifferences)
                        {
                            sb.Append($"        "); // Formatting
                            sb.AppendLine(module.Name);
                        }
                    }
                    else if (moduleUnions.Any())
                    {
                        foreach (ModuleDimensions module in moduleUnions)
                        {
                            sb.Append($"        "); // Formatting
                            sb.AppendLine(module.Name);
                        }
                    }
                }
            }

            sb.AppendLine($"    }}");  // Union close bracket
            sb.AppendLine($"}}"); // Difference close bracket

            // Write the call methods to the main object.scad file
            var filePath = Path.Combine(objectFilePath, "Solids", "object.scad");
            await Output.AppendToSCAD(
                content: sb.ToString(),
                filePath: filePath,
                cancellationToken: new CancellationToken());

            // Open the file in whatever the user has designated as the SCAD IDE associated with opening .scad files
            await ScadFileOperations.OpenScadFileAsync(filePath, allowDuplicates: false);
        }

        /// <summary>
        /// Creates and persists union modules for each layer from solid dimensions.
        /// Organizes solids by layer and operation type, generates union modules, and updates database.
        /// </summary>
        /// <param name="objectName">Name of the object</param>
        /// <param name="objectDescription">Description of the object</param>
        /// <param name="solidDimensions">Collection of solids to organize into union modules</param>
        /// <param name="moduleDimensions">Existing module dimensions for context</param>
        /// <param name="dbConnection">Database connection for persistence</param>
        /// <param name="isPreRendered">Whether modules should be pre-rendered</param>
        /// <returns>Task representing the async database operations</returns>
        public static async Task CreateUnionModuleAsync(
            string objectName,
            string objectDescription,
            IEnumerable<SolidDimensions> solidDimensions,
            IEnumerable<ModuleDimensions> moduleDimensions,
            SqliteConnection dbConnection,
            bool isPreRendered)
        {
            var objects = solidDimensions
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
                    ObjectName = objectName,
                    ObjectDescription = objectDescription,
                    SolidType = "Object",
                    OSCADMethod = ToUnionModule(addMethods, objectName, string.Empty, solidType, isPreRendered).ToLower(),
                    CreatedAt = DateTime.UtcNow,
                    Layer = layer,
                };

                // Build call method and store in Db
                unionModule.Name = ExtractModuleCallMethod(unionModule.OSCADMethod).ToLower();
                var moduleId = await unionModule.UpsertAsync(dbConnection);

                // Update all solid objects for this layer with the new ModuleDimensionsId
                var solidIds = layerObjects.Select(o => o.Id);
                await dbConnection.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
            }
        }

        /// <summary>
        /// Creates and persists difference modules for each layer from solid dimensions.
        /// First creates union modules, then creates difference modules with union as base.
        /// </summary>
        /// <param name="objectName">Name of the object</param>
        /// <param name="objectDescription">Description of the object</param>
        /// <param name="solidDimensions">Collection of solids to organize into modules</param>
        /// <param name="moduleDimensions">Existing module dimensions for context and updates</param>
        /// <param name="dbConnection">Database connection for persistence</param>
        /// <param name="isPreRendered">Whether modules should be pre-rendered</param>
        /// <returns>Task representing the async database operations</returns>
        public static async Task CreateDifferenceModuleAsync(
            string objectName,
            string objectDescription,
            IEnumerable<SolidDimensions> solidDimensions,
            IEnumerable<ModuleDimensions> moduleDimensions,
            SqliteConnection dbConnection,
            bool isPreRendered)
        {
            // First create union modules
            await CreateUnionModuleAsync(objectName, objectDescription, solidDimensions, moduleDimensions, dbConnection, isPreRendered);

            var objects = solidDimensions
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
                var baseObj = moduleDimensions.FirstOrDefault(o =>
                    o.ModuleType == "Union" &&
                    o.ObjectName == objectName &&
                    o.Layer == layer);

                if (baseObj != null)
                {
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var subtractMethods = layerObjects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                    var solidType = $"L{layer}";
                    var differenceModule = new ModuleDimensions
                    {
                        ModuleType = "Difference",
                        ObjectName = objectName,
                        ObjectDescription = objectDescription,
                        SolidType = "Object",
                        OSCADMethod = ToDifferenceModule(baseCallMethod, subtractMethods, objectName, string.Empty, solidType, isPreRendered).ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        Layer = layer,
                    };

                    // Get calling method for differenceModule
                    differenceModule.Name = ExtractModuleCallMethod(differenceModule.OSCADMethod).ToLower();
                    var moduleId = await differenceModule.UpsertAsync(dbConnection);

                    // Update all solid objects for this layer with the new ModuleDimensionsId
                    var solidIds = layerObjects.Select(o => o.Id);
                    await dbConnection.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                }
            }
        }

        /// <summary>
        /// Creates and persists intersection modules for each layer from solid dimensions.
        /// Creates intersection modules with union modules as the base.
        /// </summary>
        /// <param name="objectName">Name of the object</param>
        /// <param name="objectDescription">Description of the object</param>
        /// <param name="solidDimensions">Collection of solids to organize into modules</param>
        /// <param name="moduleDimensions">Existing module dimensions for context</param>
        /// <param name="dbConnection">Database connection for persistence</param>
        /// <param name="isPreRendered">Whether modules should be pre-rendered</param>
        /// <returns>Task representing the async database operations</returns>
        public static async Task CreateIntersectionModuleAsync(
            string objectName,
            string objectDescription,
            IEnumerable<SolidDimensions> solidDimensions,
            IEnumerable<ModuleDimensions> moduleDimensions,
            SqliteConnection dbConnection,
            bool isPreRendered)
        {
            // Get all objects marked as "Intersection"
            var objects = solidDimensions.Where(o => o.OperationType == "Intersection").ToList();

            if (!objects.Any())
                return; // No objects to process

            // Get unique layers and create a module for each layer
            var layers = objects.Select(o => o.Layer).Distinct().OrderBy(l => l).ToList();

            foreach (var layer in layers)
            {
                // Get objects for this specific layer
                var layerObjects = objects.Where(o => o.Layer == layer).ToList();

                // Find the base union module for this layer
                var baseObj = moduleDimensions.FirstOrDefault(o =>
                    o.ModuleType == "Union" &&
                    o.ObjectName == objectName &&
                    o.Layer == layer);

                if (baseObj != null)
                {
                    var baseCallMethod = ExtractModuleCallMethod(baseObj.OSCADMethod).ToLower();
                    var intersectMethods = layerObjects.Select(o => ExtractModuleCallMethod(o.OSCADMethod)).ToList();

                    var solidType = $"L{layer}";
                    var intersectionModule = new ModuleDimensions
                    {
                        ModuleType = "Intersection",
                        ObjectName = objectName,
                        ObjectDescription = objectDescription,
                        SolidType = "Object",
                        OSCADMethod = ToIntersectionModule(baseCallMethod, intersectMethods, objectName, string.Empty, solidType, isPreRendered).ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        Layer = layer,
                    };

                    // Get calling method for intersectionModule
                    intersectionModule.Name = ExtractModuleCallMethod(intersectionModule.OSCADMethod).ToLower();
                    var moduleId = await intersectionModule.UpsertAsync(dbConnection);

                    // Update all solid objects for this layer with the new ModuleDimensionsId
                    var solidIds = layerObjects.Select(o => o.Id);
                    await dbConnection.UpdateModuleDimensionsIdAsync(solidIds, moduleId);
                }
            }
        }

        /// <summary>
        /// Exports the object SCAD file to STL format by temporarily removing the axis,
        /// performing the export, and then restoring the axis if needed.
        /// </summary>
        /// <param name="objectFilePath">Root path where Solids folder exists</param>
        /// <param name="shouldRestoreAxis">Whether to restore the axis after export</param>
        /// <returns>Task representing the async export operation</returns>
        public static async Task ExportToStlAsync(
            string objectFilePath,
            bool shouldRestoreAxis)
        {
            try
            {
                var scadFile = Path.Combine(objectFilePath, "Solids", "object.scad");
                var stlFile = Path.Combine(objectFilePath, "Solids", "object.stl");

                await ScadFileOperations.ExportToStlAsync(scadFile, stlFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting to STL: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a selected item (SolidDimensions or ModuleDimensions) from the database and removes it from collections.
        /// Refreshes the parts file after successful deletion.
        /// </summary>
        /// <param name="selectedItem">The item to delete (SolidDimensions or ModuleDimensions)</param>
        /// <param name="dbConnection">Database connection for deletion</param>
        /// <returns>Task representing the async deletion operation</returns>
        public static async Task DeleteSelectedItemAsync(
            object? selectedItem,
            SqliteConnection dbConnection)
        {
            if (selectedItem == null)
                return;

            try
            {
                switch (selectedItem)
                {
                    case SolidDimensions solid:
                        await solid.DeleteAsync(dbConnection);
                        break;

                    case ModuleDimensions module:
                        await module.DeleteAsync(dbConnection);
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown item type: {selectedItem.GetType().Name}");
                        return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting item: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves and filters the axes list based on the selected unit system.
        /// Parses axes from the axes.scad file and filters by unit system prefix.
        /// </summary>
        /// <param name="selectedAxisUnitValue">The selected axis unit system (Metric or Imperial)</param>
        /// <param name="axesModulesList">The collection of parsed GeneratedModule objects from axes.scad</param>
        /// <param name="axisStored">Whether an axis is currently stored</param>
        /// <param name="selectedAxisValue">The currently selected axis value</param>
        /// <returns>Tuple containing: filtered axes list, updated selected axis value, and updated selected axis module</returns>
        public static (List<string> FilteredAxes, string SelectedAxisValue, GeneratedModule? SelectedAxis) GetFilteredAxesList(
            UnitSystem selectedAxisUnitValue,
            ObservableCollection<GeneratedModule> axesModulesList,
            bool axisStored,
            string? selectedAxisValue)
        {
            // Filter based on AXIS unit system (not general unit system)
            var filteredAxes = selectedAxisUnitValue switch
            {
                UnitSystem.Metric => axesModulesList
                    .Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_MM_"))
                    .Select(x => x.CallingMethod!)
                    .ToList(),
                UnitSystem.Imperial => axesModulesList
                    .Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_Inch_"))
                    .Select(x => x.CallingMethod!)
                    .ToList(),
                _ => axesModulesList
                    .Where(x => x.CallingMethod != null)
                    .Select(x => x.CallingMethod!)
                    .ToList()
            };

            // Add "Select Axis" as the first item if no axis is stored
            if (!axisStored)
            {
                filteredAxes.Insert(0, "Select Axis");
            }

            // Determine the selected axis value
            var updatedSelectedAxisValue = selectedAxisValue;
            if (!filteredAxes.Contains(selectedAxisValue!))
            {
                updatedSelectedAxisValue = axisStored ? filteredAxes.FirstOrDefault() : "Select Axis";
            }

            // Find the corresponding GeneratedModule
            var selectedAxis = axesModulesList.FirstOrDefault(x => x.CallingMethod == updatedSelectedAxisValue);

            return (filteredAxes, updatedSelectedAxisValue ?? "Select Axis", selectedAxis);
        }

        /// <summary>
        /// Loads PNG image dimensions and calculates surface parameters with unit conversion.
        /// Applies rotation and scaling to image dimensions, handles unit conversion, and calculates offsets.
        /// </summary>
        /// <param name="filePath">Path to the PNG file</param>
        /// <param name="autoSmoothFile">Whether auto-smooth is enabled</param>
        /// <param name="selectedUnit">Current unit system (Metric or Imperial)</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <param name="surfaceScaleX">X-axis scale factor</param>
        /// <param name="surfaceScaleY">Y-axis scale factor</param>
        /// <param name="surfaceScaleZ">Z-axis scale factor</param>
        /// <param name="surfaceInvert">Whether surface is inverted</param>
        /// <returns>Tuple containing: length, width, height, xOffset, zOffset, updatedFilePath</returns>
        public static (double Length, double Width, double Height, double XOffset, double ZOffset, string UpdatedFilePath) LoadPngDimensionsData(
            string filePath,
            bool autoSmoothFile,
            UnitSystem selectedUnit,
            int decimalPlaces,
            double surfaceScaleX,
            double surfaceScaleY,
            double surfaceScaleZ,
            bool surfaceInvert)
        {
            // Get PNG dimensions from ImageHelper
            var (width, height, depth, updatedFilePath) = ImageHelper.GetPngDimensions(filePath, autoSmoothFile);

            // Convert units if necessary
            if (selectedUnit == UnitSystem.Imperial)
            {
                depth = Math.Round(MillimeterToInches(depth), decimalPlaces);
                width = Math.Round(MillimeterToInches(width), decimalPlaces);
                height = Math.Round(MillimeterToInches(height), decimalPlaces);
            }

            // Apply scaling
            var length = depth * surfaceScaleX;
            var widthScaled = width * surfaceScaleY;
            var heightScaled = height * surfaceScaleZ;

            // Calculate X offset based on invert setting
            var xOffset = 0.0;
            if (surfaceInvert)
            {
                xOffset = length;  // 100mm OpenSCAD default equivalent
            }

            return (length, widthScaled, heightScaled, xOffset, 0.0, updatedFilePath);
        }

        /// <summary>
        /// Regenerates a solid's OSCAD method with the specified color.
        /// Replaces any existing color wrapper with the new one (does not append).
        /// </summary>
        /// <param name="moduleContent">The original OSCAD method string</param>
        /// <param name="color">The color to apply</param>
        /// <returns>Updated OSCAD method string with color wrapper</returns>
        public static string RegenerateSolidWithColor(string moduleContent, OpenScadColor color)
        {
            // Find the opening brace after the module declaration
            int openingBraceIndex = moduleContent.IndexOf('{');
            int closingBraceIndex = moduleContent.LastIndexOf('}');

            if (openingBraceIndex == -1 || closingBraceIndex == -1 || closingBraceIndex <= openingBraceIndex)
            {
                // Invalid format, just wrap it
                return $"color(\"{color.ToString().ToLower()}\") {{ {moduleContent} }}";
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

            return sb.ToString();
        }

        /// <summary>
        /// Removes nested color() wrappers to prevent accumulation when updating colors.
        /// </summary>
        /// <param name="content">The OSCAD method content to clean</param>
        /// <returns>Content with color wrappers stripped</returns>
        public static string StripColorWrappers(string content)
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
        /// <param name="text">The text to search</param>
        /// <param name="openBraceIndex">The index of the opening brace</param>
        /// <returns>Index of matching closing brace, or -1 if not found</returns>
        public static int FindMatchingCloseBrace(string text, int openBraceIndex)
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

        /// <summary>
        /// Retrieves solid dimensions and module dimensions from the database for a given object name.
        /// Creates necessary tables if they don't exist and populates collections with retrieved data.
        /// </summary>
        /// <param name="dbConnection">Database connection for data retrieval</param>
        /// <param name="objectName">Name of the object to retrieve dimensions for</param>
        /// <returns>Tuple containing: solid dimensions collection, module dimensions collection</returns>
        public static async Task<(ObservableCollection<SolidDimensions> Solids, ObservableCollection<ModuleDimensions> Modules)> GetDimensionsPartsAsync(
            SqliteConnection dbConnection,
            string objectName)
        {
            // Ensure all necessary tables exist, for both solids, shapes, polyhedrons, and modules
            await AxisDimensionsExtensions.CreateTable(dbConnection);
            await SolidDimensionsExtensions.CreateTable(dbConnection);
            await ModuleDimensionsExtensions.CreateTable(dbConnection);
            await PolyhedronDimensionsExtensions.CreateTable(dbConnection);
            await ShapeDimensionsExtensions.CreateTable(dbConnection);

            // Get records from database with both Axis and Module joins
            var records = await new SolidDimensions().GetByNameWithAxisAndModuleAsync(dbConnection, objectName);
            var moduleRecords = await new ModuleDimensions().GetByObjectNameAsync(dbConnection, objectName);

            // Return as ObservableCollections
            return (
                new ObservableCollection<SolidDimensions>(records),
                new ObservableCollection<ModuleDimensions>(moduleRecords)
            );
        }

        /// <summary>
        /// Calculates and returns updated server rack dimensions based on selected rack and width type.
        /// Handles both metric and imperial unit systems.
        /// </summary>
        /// <param name="isCubeSelected">Whether cube shape is selected</param>
        /// <param name="isRoundCubeSelected">Whether round cube shape is selected</param>
        /// <param name="selectedServerRackWidthType">Selected width type (Inner Mount or Outer Mount)</param>
        /// <param name="selectedServerRack">Selected server rack</param>
        /// <param name="selectedUnitSystem">Current unit system (Metric or Imperial)</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing: updatedWidth, updatedHeight</returns>
        public static (double UpdatedWidth, double UpdatedHeight) CalculateServerRackDimensions(
            bool isCubeSelected,
            bool isRoundCubeSelected,
            string selectedServerRackWidthType,
            ServerRack? selectedServerRack,
            UnitSystem selectedUnitSystem,
            int decimalPlaces)
        {
            // Return current values if cube/round cube not selected
            if (!isCubeSelected && !isRoundCubeSelected)
                return (0, 0);

            double updatedWidth = 0;
            double updatedHeight = 0;

            // Update width if a width type is selected
            if (!string.IsNullOrEmpty(selectedServerRackWidthType))
            {
                var rackData = ServerRackDimensions.GetAll().FirstOrDefault();
                if (rackData != null)
                {
                    updatedWidth = selectedServerRackWidthType switch
                    {
                        "Inner Mount" => selectedUnitSystem == UnitSystem.Metric
                            ? rackData.InnerWidthMm
                            : rackData.InnerWidthInches,
                        "Outer Mount" => selectedUnitSystem == UnitSystem.Metric
                            ? rackData.OuterWidthMm
                            : rackData.OuterWidthInches,
                        _ => 0
                    };
                    updatedWidth = Math.Round(updatedWidth, decimalPlaces);
                }
            }

            // Update height if server rack is selected
            if (selectedServerRack != null)
            {
                updatedHeight = selectedUnitSystem == UnitSystem.Metric
                    ? selectedServerRack.HeightMm
                    : Math.Round(selectedServerRack.HeightInches, decimalPlaces);
            }

            return (updatedWidth, updatedHeight);
        }

        /// <summary>
        /// Calculates the radius value based on screw property selection and unit system.
        /// </summary>
        /// <param name="screwSize">Selected screw size data</param>
        /// <param name="screwProperty">Selected screw property (Thread, Head, Insert, or Clearance)</param>
        /// <param name="selectedUnitSystem">Current unit system</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Calculated radius value in appropriate units</returns>
        public static double CalculateScrewRadius(
            ScrewSize? screwSize,
            string screwProperty,
            UnitSystem selectedUnitSystem,
            int decimalPlaces)
        {
            if (screwSize == null)
                return 0;

            double radiusValue = screwProperty switch
            {
                "Screw Thread" => screwSize.ScrewRadius,
                "Screw Head" => screwSize.ScrewHeadRadius,
                "Threaded Insert" => screwSize.ThreadedInsertRadius,
                "Clearance Hole" => screwSize.ClearanceHoleRadius,
                _ => 0
            };

            return selectedUnitSystem == UnitSystem.Imperial
                ? Math.Round(MillimeterToInches(radiusValue), decimalPlaces)
                : radiusValue;
        }

        /// <summary>
        /// Determines which view buttons should be enabled based on solid and module dimensions.
        /// Evaluates module types and solid operation types to set button states.
        /// </summary>
        /// <param name="moduleDimensions">Collection of module dimensions</param>
        /// <param name="solidDimensions">Collection of solid dimensions</param>
        /// <returns>Tuple containing: SaveFileButton, DifferenceButton, UnionButton, IntersectionButton</returns>
        public static (bool SaveFile, bool Difference, bool Union, bool Intersection) CalculateButtonStates(
            ObservableCollection<ModuleDimensions> moduleDimensions,
            ObservableCollection<SolidDimensions> solidDimensions)
        {
            var mDim = moduleDimensions.Where(o => o.SolidType == "Object");

            bool saveFileButton = false;
            bool differenceButton = false;
            bool unionButton = false;
            bool intersectionButton = false;

            // Determine SaveFileButton - if there are any "Object" type modules
            if (mDim.Any())
            {
                saveFileButton = true;
                // Determine DifferenceButton - if there is at least one "Difference" operation
                if (solidDimensions.Where(o => o.OperationType == "Difference").Any())
                {
                    differenceButton = true;
                }
            }

            // Determine UnionButton - if there is at least one "Union" operation
            if (solidDimensions.Where(o => o.OperationType == "Union").Any())
            {
                unionButton = true;
            }

            // Determine IntersectionButton - if there is at least one "Intersection" operation
            if (solidDimensions.Where(o => o.OperationType == "Intersection").Any())
            {
                intersectionButton = true;
            }

            return (saveFileButton, differenceButton, unionButton, intersectionButton);
        }

        /// <summary>
        /// Builds modal content displaying OSCAD methods for a given module and its associated solids.
        /// </summary>
        /// <param name="module">The module dimensions to display methods for</param>
        /// <param name="solids">Collection of solids associated with the module</param>
        /// <returns>Tuple containing: modalTitle, modalContent</returns>
        public static (string Title, string Content) BuildOscadMethodsModal(
            ModuleDimensions module,
            IEnumerable<SolidDimensions> solids)
        {
            if (!solids.Any())
                return (string.Empty, string.Empty);

            var title = $"OSCAD Methods for {module.Name}";

            var sb = new StringBuilder();
            sb.AppendLine($"Module Name (Call Method): {module.Name}");
            sb.AppendLine();
            sb.AppendLine("Solids:");
            sb.AppendLine(new string('-', 50));
            sb.Append(string.Join("\n\n", solids.Select(s => s.OSCADMethod)));

            return (title, sb.ToString());
        }

        /// <summary>
        /// Converts input dimensions from metric (mm) to imperial (inches).
        /// </summary>
        /// <param name="lengthMM">Length in millimeters</param>
        /// <param name="widthMM">Width in millimeters</param>
        /// <param name="heightMM">Height in millimeters</param>
        /// <param name="thicknessMM">Thickness in millimeters</param>
        /// <param name="radiusMM">Radius in millimeters</param>
        /// <param name="radius1MM">First radius in millimeters</param>
        /// <param name="radius2MM">Second radius in millimeters</param>
        /// <param name="cylinderHeightMM">Cylinder height in millimeters</param>
        /// <param name="xOffsetMM">X offset in millimeters</param>
        /// <param name="yOffsetMM">Y offset in millimeters</param>
        /// <param name="zOffsetMM">Z offset in millimeters</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted imperial values in the same order</returns>
        public static (double Length, double Width, double Height, double Thickness, double Radius, double Radius1, double Radius2, double CylinderHeight, double XOffset, double YOffset, double ZOffset) ConvertInputsToImperial(
            double lengthMM,
            double widthMM,
            double heightMM,
            double thicknessMM,
            double radiusMM,
            double radius1MM,
            double radius2MM,
            double cylinderHeightMM,
            double xOffsetMM,
            double yOffsetMM,
            double zOffsetMM,
            int decimalPlaces)
        {
            // Convert from metric unit system to imperial (mm to inches)
            var convertedLength = Math.Round(MillimeterToInches(lengthMM), decimalPlaces);
            var convertedWidth = Math.Round(MillimeterToInches(widthMM), decimalPlaces);
            var convertedHeight = Math.Round(MillimeterToInches(heightMM), decimalPlaces);
            var convertedThickness = Math.Round(MillimeterToInches(thicknessMM), decimalPlaces);
            var convertedRadius = Math.Round(MillimeterToInches(radiusMM), decimalPlaces);
            var convertedRadius1 = Math.Round(MillimeterToInches(radius1MM), decimalPlaces);
            var convertedRadius2 = Math.Round(MillimeterToInches(radius2MM), decimalPlaces);
            var convertedCylinderHeight = Math.Round(MillimeterToInches(cylinderHeightMM), decimalPlaces);
            var convertedXOffset = Math.Round(MillimeterToInches(xOffsetMM), decimalPlaces);
            var convertedYOffset = Math.Round(MillimeterToInches(yOffsetMM), decimalPlaces);
            var convertedZOffset = Math.Round(MillimeterToInches(zOffsetMM), decimalPlaces);

            return (convertedLength, convertedWidth, convertedHeight, convertedThickness, convertedRadius, convertedRadius1, convertedRadius2, convertedCylinderHeight, convertedXOffset, convertedYOffset, convertedZOffset);
        }

        /// <summary>
        /// Converts input dimensions from imperial (inches) to metric (mm).
        /// </summary>
        /// <param name="lengthInches">Length in inches</param>
        /// <param name="widthInches">Width in inches</param>
        /// <param name="heightInches">Height in inches</param>
        /// <param name="thicknessInches">Thickness in inches</param>
        /// <param name="radiusInches">Radius in inches</param>
        /// <param name="radius1Inches">First radius in inches</param>
        /// <param name="radius2Inches">Second radius in inches</param>
        /// <param name="cylinderHeightInches">Cylinder height in inches</param>
        /// <param name="xOffsetInches">X offset in inches</param>
        /// <param name="yOffsetInches">Y offset in inches</param>
        /// <param name="zOffsetInches">Z offset in inches</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted metric values in the same order</returns>
        public static (double Length, double Width, double Height, double Thickness, double Radius, double Radius1, double Radius2, double CylinderHeight, double XOffset, double YOffset, double ZOffset) ConvertInputsToMetric(
            double lengthInches,
            double widthInches,
            double heightInches,
            double thicknessInches,
            double radiusInches,
            double radius1Inches,
            double radius2Inches,
            double cylinderHeightInches,
            double xOffsetInches,
            double yOffsetInches,
            double zOffsetInches,
            int decimalPlaces)
        {
            // Convert from imperial unit system to metric (inches to mm)
            var convertedLength = Math.Round(InchesToMillimeter(lengthInches), decimalPlaces);
            var convertedWidth = Math.Round(InchesToMillimeter(widthInches), decimalPlaces);
            var convertedHeight = Math.Round(InchesToMillimeter(heightInches), decimalPlaces);
            var convertedThickness = Math.Round(InchesToMillimeter(thicknessInches), decimalPlaces);
            var convertedRadius = Math.Round(InchesToMillimeter(radiusInches), decimalPlaces);
            var convertedRadius1 = Math.Round(InchesToMillimeter(radius1Inches), decimalPlaces);
            var convertedRadius2 = Math.Round(InchesToMillimeter(radius2Inches), decimalPlaces);
            var convertedCylinderHeight = Math.Round(InchesToMillimeter(cylinderHeightInches), decimalPlaces);
            var convertedXOffset = Math.Round(InchesToMillimeter(xOffsetInches), decimalPlaces);
            var convertedYOffset = Math.Round(InchesToMillimeter(yOffsetInches), decimalPlaces);
            var convertedZOffset = Math.Round(InchesToMillimeter(zOffsetInches), decimalPlaces);

            return (convertedLength, convertedWidth, convertedHeight, convertedThickness, convertedRadius, convertedRadius1, convertedRadius2, convertedCylinderHeight, convertedXOffset, convertedYOffset, convertedZOffset);
        }

        /// <summary>
        /// Copies all object data (solids and modules) from source to new object with callback.
        /// Performs database copy operation off the UI thread, retrieves dimensions, then invokes callback.
        /// </summary>
        public static async Task<(ObservableCollection<SolidDimensions>, ObservableCollection<ModuleDimensions>)> CopyObjectWithCallbackAsync(string sourceObjectName, SqliteConnection dbConnection)
        {
            try
            {
                // Perform database copy operation off UI thread
                await Task.Run(async () =>
                {
                    var sourceSolid = new SolidDimensions { Name = sourceObjectName };
                    await sourceSolid.CopyObjectAsync(dbConnection);
                });

                // Refresh dimensions from database
                var (updatedSolidDimensions, updatedModuleDimensions) = await GetDimensionsPartsAsync(dbConnection, $"{sourceObjectName}_copy");

                // Invoke callback with all updated data
                return (updatedSolidDimensions, updatedModuleDimensions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying object: {ex.Message}");
                return (new ObservableCollection<SolidDimensions>(), new ObservableCollection<ModuleDimensions>());
            }
        }
    }
}
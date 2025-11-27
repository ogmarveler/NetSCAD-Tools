using System.Diagnostics.CodeAnalysis;
using static NetGenCAD.Core.Measurements.Selector;

namespace NetGenCAD.Core.Utility
{
    // Store (const) enums from NetGenCAD.Core here to be used as static resources or compiled bindings
    public static class EnumHelper
    {
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public static IReadOnlyList<BackgroundType> BackgroundTypeValues => [.. Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>()];

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public static IReadOnlyList<UnitSystem> UnitSystemValues => [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
    }
}

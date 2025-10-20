using System.Diagnostics.CodeAnalysis;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.Core.Utility
{
    // Store (const) enums from NetScad.Core here to be used as static resources or compiled bindings
    public static class EnumHelper
    {
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public static IReadOnlyList<BackgroundType> BackgroundTypeValues => [.. Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>()];

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public static IReadOnlyList<UnitSystem> UnitSystemValues => [.. Enum.GetValues(typeof(UnitSystem)).Cast<UnitSystem>()];
    }
}

namespace NetScad.Core.Measurements
{
    using System;

    public static class VolumeConverter
    {
        // Cross-system conversions
        private const double Cm3ToIn3 = 1.0 / 16.387064;     // cm³ to in³
        private const double In3ToCm3 = 16.387064;           // in³ to cm³
        private const double Mm3ToIn3 = 1.0 / 16387.064;     // mm³ to in³ (1 in³ = 16,387.064 mm³)
        private const double In3ToMm3 = 16387.064;           // in³ to mm³
        private const double M3ToFt3 = 35.314666711658;      // m³ to ft³
        private const double Ft3ToM3 = 1.0 / 35.314666711658; // ft³ to m³

        // Metric scale conversions
        private const double Cm3ToM3 = 1.0 / 1_000_000;      // cm³ to m³ (1 m³ = 1,000,000 cm³)
        private const double M3ToCm3 = 1_000_000;            // m³ to cm³
        private const double Mm3ToCm3 = 1.0 / 1_000;         // mm³ to cm³ (1 cm³ = 1,000 mm³)
        private const double Cm3ToMm3 = 1_000;               // cm³ to mm³
        private const double Mm3ToM3 = 1.0 / 1_000_000_000;  // mm³ to m³ (1 m³ = 1,000,000,000 mm³)
        private const double M3ToMm3 = 1_000_000_000;        // m³ to mm³

        // Imperial scale conversions
        private const double In3ToFt3 = 1.0 / 1728;          // in³ to ft³ (1 ft³ = 1728 in³)
        private const double Ft3ToIn3 = 1728;                // ft³ to in³

        // Cross-system conversions
        public static double ConvertCm3ToIn3(double cm3) => cm3 * Cm3ToIn3;
        public static double ConvertM3ToFt3(double m3) => m3 * M3ToFt3;
        public static double ConvertIn3ToCm3(double in3) => in3 * In3ToCm3;
        public static double ConvertFt3ToM3(double ft3) => ft3 * Ft3ToM3;
        public static double ConvertMm3ToIn3(double mm3) => mm3 * Mm3ToIn3;
        public static double ConvertIn3ToMm3(double in3) => in3 * In3ToMm3;

        // Metric scale conversions (cm³ ↔ m³)
        public static double ConvertCm3ToM3(double cm3) => cm3 * Cm3ToM3;
        public static double ConvertM3ToCm3(double m3) => m3 * M3ToCm3;

        // Metric scale conversions (mm³ ↔ cm³)
        public static double ConvertMm3ToCm3(double mm3) => mm3 * Mm3ToCm3;
        public static double ConvertCm3ToMm3(double cm3) => cm3 * Cm3ToMm3;

        // Metric scale conversions (mm³ ↔ m³)
        public static double ConvertMm3ToM3(double mm3) => mm3 * Mm3ToM3;
        public static double ConvertM3ToMm3(double m3) => m3 * M3ToMm3;

        // Imperial scale conversions (in³ ↔ ft³)
        public static double ConvertIn3ToFt3(double in3) => in3 * In3ToFt3;
        public static double ConvertFt3ToIn3(double ft3) => ft3 * Ft3ToIn3;

        // Generic conversion method with enum support
        public enum VolumeUnit { Mm3, Cm3, M3, In3, Ft3 }
        public enum TargetUnit { Mm3, Cm3, M3, In3, Ft3 }

        public static double Convert(double value, VolumeUnit from, TargetUnit to)
        {
            return (from, to) switch
            {
                // Cross-system conversions
                (VolumeUnit.Cm3, TargetUnit.In3) => ConvertCm3ToIn3(value),
                (VolumeUnit.Mm3, TargetUnit.In3) => ConvertMm3ToIn3(value),
                (VolumeUnit.M3, TargetUnit.Ft3) => ConvertM3ToFt3(value),
                (VolumeUnit.In3, TargetUnit.Cm3) => ConvertIn3ToCm3(value),
                (VolumeUnit.In3, TargetUnit.Mm3) => ConvertIn3ToMm3(value),
                (VolumeUnit.Ft3, TargetUnit.M3) => ConvertFt3ToM3(value),

                // Metric scale conversions (cm³ ↔ m³)
                (VolumeUnit.Cm3, TargetUnit.M3) => ConvertCm3ToM3(value),
                (VolumeUnit.M3, TargetUnit.Cm3) => ConvertM3ToCm3(value),

                // Metric scale conversions (mm³ ↔ cm³)
                (VolumeUnit.Mm3, TargetUnit.Cm3) => ConvertMm3ToCm3(value),
                (VolumeUnit.Cm3, TargetUnit.Mm3) => ConvertCm3ToMm3(value),

                // Metric scale conversions (mm³ ↔ m³)
                (VolumeUnit.Mm3, TargetUnit.M3) => ConvertMm3ToM3(value),
                (VolumeUnit.M3, TargetUnit.Mm3) => ConvertM3ToMm3(value),

                // Imperial scale conversions (in³ ↔ ft³)
                (VolumeUnit.In3, TargetUnit.Ft3) => ConvertIn3ToFt3(value),
                (VolumeUnit.Ft3, TargetUnit.In3) => ConvertFt3ToIn3(value),

                // Same unit (no conversion)
                _ when from.ToString() == to.ToString() => value,

                _ => throw new ArgumentException($"Unsupported conversion from {from} to {to}")
            };
        }
    }
}
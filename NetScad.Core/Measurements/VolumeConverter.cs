namespace NetScad.Core.Measurements
{
    using System;

    public static class VolumeConverter
    {
        private const double Cm3ToIn3 = 1.0 / 16.387064;     // cm³ to in³
        private const double In3ToCm3 = 16.387064;           // in³ to cm³
        private const double M3ToFt3 = 35.314666711658;      // m³ to ft³
        private const double Ft3ToM3 = 1.0 / 35.314666711658; // ft³ to m³
        private const double Cm3ToM3 = 1.0 / 1_000_000;      // cm³ to m³ (1 m³ = 1,000,000 cm³)
        private const double M3ToCm3 = 1_000_000;            // m³ to cm³
        private const double In3ToFt3 = 1.0 / 1728;          // in³ to ft³ (1 ft³ = 1728 in³)
        private const double Ft3ToIn3 = 1728;                // ft³ to in³

        // Existing methods for cross-system conversions
        public static double ConvertCm3ToIn3(double cm3) => cm3 * Cm3ToIn3;
        public static double ConvertM3ToFt3(double m3) => m3 * M3ToFt3;
        public static double ConvertIn3ToCm3(double in3) => in3 * In3ToCm3;
        public static double ConvertFt3ToM3(double ft3) => ft3 * Ft3ToM3;

        // NEW: Same-system scale conversions (Metric)
        public static double ConvertCm3ToM3(double cm3) => cm3 * Cm3ToM3;
        public static double ConvertM3ToCm3(double m3) => m3 * M3ToCm3;

        // NEW: Same-system scale conversions (Imperial)
        public static double ConvertIn3ToFt3(double in3) => in3 * In3ToFt3;
        public static double ConvertFt3ToIn3(double ft3) => ft3 * Ft3ToIn3;

        // Updated generic method
        public enum VolumeUnit { Cm3, M3, In3, Ft3 }
        public enum TargetUnit { Cm3, M3, In3, Ft3 }

        public static double Convert(double value, VolumeUnit from, TargetUnit to)
        {
            return (from, to) switch
            {
                // Cross-system conversions
                (VolumeUnit.Cm3, TargetUnit.In3) => ConvertCm3ToIn3(value),
                (VolumeUnit.M3, TargetUnit.Ft3) => ConvertM3ToFt3(value),
                (VolumeUnit.In3, TargetUnit.Cm3) => ConvertIn3ToCm3(value),
                (VolumeUnit.Ft3, TargetUnit.M3) => ConvertFt3ToM3(value),
                
                // Same-system scale conversions (Metric)
                (VolumeUnit.Cm3, TargetUnit.M3) => ConvertCm3ToM3(value),
                (VolumeUnit.M3, TargetUnit.Cm3) => ConvertM3ToCm3(value),
                
                // Same-system scale conversions (Imperial)
                (VolumeUnit.In3, TargetUnit.Ft3) => ConvertIn3ToFt3(value),
                (VolumeUnit.Ft3, TargetUnit.In3) => ConvertFt3ToIn3(value),
                
                // Same unit (no conversion)
                _ when from.ToString() == to.ToString() => value,
                
                _ => throw new ArgumentException($"Unsupported conversion from {from} to {to}")
            };
        }
    }
}

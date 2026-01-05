namespace NetGenCAD.Core.Measurements
{
    public static class Conversion
    {
        /// <summary>
        /// Intelligently rounds floating-point numbers to eliminate floating-point precision artifacts.
        /// Reduces unnecessary decimal places while maintaining meaningful precision up to 6 decimal places.
        /// Useful after unit conversions (imperial/metric) to minimize compute engine requirements.
        /// </summary>
        private static double CleanFloatingPoint(double value, int maxDecimalPlaces = 6)
        {
            // Handle special cases
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value;

            // For very small numbers, round to zero
            if (Math.Abs(value) < 1e-16)
                return 0;

            // Round to maximum decimal places first
            double rounded = Math.Round(value, maxDecimalPlaces);

            // If rounding changed the value significantly (more than 0.000001), use the rounded value
            if (Math.Abs(value - rounded) > 0.000001)
                return rounded;

            // Special case: collapse binary floating artifacts like 262.89000000000004 mm → 262.89 mm
            // (Extremely close to its 2-decimal representation; keep higher precision if genuinely needed)
            if (maxDecimalPlaces >= 2)
            {
                double twoDecimal = Math.Round(value, 2);
                if (Math.Abs(value - twoDecimal) < 1e-12)
                    return twoDecimal;
            }

            // Check if the rounded value is very close to a simpler representation
            for (int decimals = maxDecimalPlaces - 1; decimals >= 0; decimals--)
            {
                double simplified = Math.Round(value, decimals);
                double difference = Math.Abs(value - simplified);

                // If difference is within floating-point epsilon, use simplified form
                if (difference < Math.Pow(10, -(decimals + 1)) * 0.5)
                    return simplified;
            }

            return rounded;
        }

        // Base level functions - now with automatic floating-point cleaning
        public static double MillimeterToInches(double mm) => CleanFloatingPoint(mm / Inch.Inch.ToMm(1));
        public static double InchesToMillimeter(double inch) => CleanFloatingPoint(inch * Inch.Inch.ToMm(1));

        // Composite functions
        public static double CentimeterToInches(double cm) => CentimeterToMillimeter(MillimeterToInches(cm));
        public static double MeterToInches(double m) => MeterToCentimeter(CentimeterToInches(m));
        public static double CentimeterToMillimeter(double cm) => CleanFloatingPoint(cm * 10);
        public static double MillimeterToCentimeter(double mm) => CleanFloatingPoint(mm / 10);
        public static double MillimeterToMeter(double mm) => CentimeterToMeter(MillimeterToCentimeter(mm));
        public static double MeterToCentimeter(double m) => CleanFloatingPoint(m * 100);
        public static double CentimeterToMeter(double cm) => CleanFloatingPoint(cm / 100);
        public static double MeterToMillimeter(double m) => MeterToCentimeter(CentimeterToMillimeter(m));
        public static double InchesToCentimeter(double inches) => MillimeterToCentimeter(InchesToMillimeter(inches));
        public static double InchesToMeter(double inches) => CentimeterToMeter(InchesToCentimeter(inches));
        public static double FeetToMeter(double feet) => InchesToMeter(FeetToInches(feet));
        public static double InchesToFeet(double inches) => CleanFloatingPoint(inches / 12);
        public static double FeetToInches(double feet) => CleanFloatingPoint(feet * 12);
        public static double MillimeterToFeet(double mm) => InchesToFeet(MillimeterToInches(mm));
        public static double MeterToFeet(double m) => MillimeterToFeet(MeterToMillimeter(m));

        // Non-conversion utility functions
        public static double RoundEdgeHeight(double radius) => CleanFloatingPoint(radius / 50); // Convert a radius in mm to a ratio for rounding edges in OpenSCAD
        public static double RoundFromWidth(double width) => CleanFloatingPoint(width * .05f); // Calculate rounding radius from width of main object
        public static double MinkowskiOffsets(double radius) => CleanFloatingPoint(radius * 2); // Convert a radius in mm to a Minkowski offset in OpenSCAD

        /// <summary>
        /// Intelligently rounds floating-point numbers to eliminate floating-point precision artifacts.
        /// Reduces unnecessary decimal places while maintaining meaningful precision up to 6 decimal places.
        /// Useful for manual cleanup when needed outside of conversions.
        /// 
        /// Examples:
        /// - 120.442499999991123 → 120.4425 (removes trailing noise)
        /// - 25.39999999999 → 25.4 (detects near-integer pattern)
        /// - 3.141592653589793 → 3.141593 (maintains 6 decimal precision)
        /// - 50.0 → 50 (preserves whole numbers)
        /// </summary>
        public static double RoundFloatingPoints(double value, int maxDecimalPlaces = 6)
        {
            return CleanFloatingPoint(value, maxDecimalPlaces);
        }

        /// <summary>
        /// Rounds a value and returns a string representation without trailing zeros.
        /// Useful for generating clean OpenSCAD/STL output.
        /// </summary>
        public static string RoundAndFormatFloatingPoints(double value, int maxDecimalPlaces = 6)
        {
            double rounded = RoundFloatingPoints(value, maxDecimalPlaces);
            return rounded.ToString($"F{maxDecimalPlaces}").TrimEnd('0').TrimEnd('.');
        }

        /// <summary>
        /// Applies intelligent rounding to an entire array of floating-point values.
        /// Useful for rounding coordinate arrays, dimension arrays, etc.
        /// </summary>
        public static double[] RoundFloatingPointsArray(double[] values, int maxDecimalPlaces = 6)
        {
            return values.Select(v => RoundFloatingPoints(v, maxDecimalPlaces)).ToArray();
        }

        /// <summary>
        /// Applies intelligent rounding to a list of floating-point values.
        /// </summary>
        public static List<double> RoundFloatingPointsList(List<double> values, int maxDecimalPlaces = 6)
        {
            return values.Select(v => RoundFloatingPoints(v, maxDecimalPlaces)).ToList();
        }
    }
}

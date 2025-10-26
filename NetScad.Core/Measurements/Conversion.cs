namespace NetScad.Core.Measurements
{
    public static class Conversion
    {
        public static double MillimeterToInches(double mm) => mm / Inch.Inch.ToMm(1);  // Base level function
        public static double CentimeterToInches(double cm) => CentimeterToMillimeter(MillimeterToInches(cm)); // Base + Scale function
        public static double MeterToInches(double m) => MeterToCentimeter(CentimeterToInches(m));
        public static double CentimeterToMillimeter(double cm) => cm * 10; // Scale function
        public static double MillimeterToCentimeter(double mm) => mm / 10; // Scale function
        public static double MillimeterToMeter(double mm) => CentimeterToMeter(MillimeterToCentimeter(mm)); // Scale function
        public static double MeterToCentimeter(double m) => m * 100;
        public static double CentimeterToMeter(double cm) => cm / 100;
        public static double MeterToMillimeter(double m) => MeterToCentimeter(CentimeterToMillimeter(m));
        public static double InchesToMillimeter(double inch) => inch * Inch.Inch.ToMm(1); // Base level function
        public static double InchesToCentimeter(double inches) => MillimeterToCentimeter(InchesToMillimeter(inches));
        public static double InchesToMeter(double inches) => CentimeterToMeter(InchesToCentimeter(inches));
        public static double FeetToMeter(double feet) => InchesToMeter(FeetToInches(feet));
        public static double InchesToFeet(double inches) => inches / 12;  // Base level function
        public static double FeetToInches(double feet) => feet * 12;  // Base level function
        public static double MillimeterToFeet(double mm) => InchesToFeet(MillimeterToInches(mm));
        public static double MeterToFeet(double m) => MillimeterToFeet(MeterToMillimeter(m));
        public static double RoundEdgeHeight(double radius) => radius / 50; // Convert a radius in mm to a ratio for rounding edges in OpenSCAD
        public static double RoundFromWidth(double width) => width * .05f; // Calculate rounding radius from width of main object
        public static double MinkowskiOffsets(double radius) => radius * 2; // Convert a radius in mm to a Minkowski offset in OpenSCAD
    }
}

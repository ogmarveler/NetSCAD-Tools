using System.Text.Json.Serialization;

namespace NetGenCAD.Core.Measurements
{
    public enum FractionalInch
    {
        [JsonInclude]
        Inch64,
        [JsonInclude]
        Inch32,
        [JsonInclude]
        Inch16,
        [JsonInclude]
        Inch8,
        [JsonInclude]
        Inch4,
        [JsonInclude]
        Inch2,
        [JsonInclude]
        Inch1
    }

    public enum Inch
    {
        [JsonInclude]
        Inch
    }

    public static class Imperial
    {
        public static double ToInches(this FractionalInch fraction, int numerator) => numerator / (double)GetDenominator(fraction);

        public static double ToMm(this FractionalInch fraction, int numerator) => fraction.ToInches(numerator) * 25.4;

        public static (int Numerator, int Denominator) FromMm(this FractionalInch fraction, double mm)
        {
            int denominator = GetDenominator(fraction);
            double inches = mm * 0.0393700787; // mm to inches
            int numerator = (int)Math.Round(inches * denominator);
            return (numerator, denominator);
        }

        public static double ToInches(this Inch inch, int numerator) => numerator; // 1 inch = numerator/1

        public static double ToMm(this Inch inch, int numerator) => numerator * 25.4; // inches to mm

        public static (int Numerator, int Denominator) FromMm(this Inch inch, double mm)
        {
            double inches = mm * 0.0393700787; // mm to inches
            int numerator = (int)Math.Round(inches);
            return (numerator, 1);
        }

        private static int GetDenominator(FractionalInch fraction) => fraction switch
        {
            FractionalInch.Inch64 => 64,
            FractionalInch.Inch32 => 32,
            FractionalInch.Inch16 => 16,
            FractionalInch.Inch8 => 8,
            FractionalInch.Inch4 => 4,
            FractionalInch.Inch2 => 2,
            FractionalInch.Inch1 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(fraction))
        };
    }
}

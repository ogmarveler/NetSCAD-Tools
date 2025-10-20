namespace NetScad.Core.Measurements
{
    internal class AwgCable(int awg, double conductorRadiusMm, double insulatedRadiusMm, double conductorRadiusInch, double insulatedRadiusInch)
    {
        public int Awg { get; } = awg;
        public double ConductorRadiusMm { get; } = conductorRadiusMm;
        public double InsulatedRadiusMm { get; } = insulatedRadiusMm;
        public double ConductorRadiusInch { get; } = conductorRadiusInch;
        public double InsulatedRadiusInch { get; } = insulatedRadiusInch;
    }

    internal static class AwgData
    {
        internal static List<AwgCable> Cables { get; } =
        [
            new(22, 0.643, 1.27, 0.0253, 0.0500),
            new(20, 0.812, 1.42, 0.0320, 0.0559),
            new(18, 1.024, 1.68, 0.0403, 0.0661),
            new(16, 1.291, 2.03, 0.0508, 0.0799),
            new(14, 1.628, 2.41, 0.0641, 0.0949),
            new(12, 2.053, 2.95, 0.0808, 0.1161),
            new(10, 2.588, 3.61, 0.1019, 0.1421),
            new(8, 3.264, 4.50, 0.1285, 0.1772),
            new(6, 4.115, 5.69, 0.1620, 0.2240),
            new(4, 5.189, 7.01, 0.2043, 0.2760),
            new(2, 6.544, 8.89, 0.2576, 0.3500),
            new(1, 7.348, 9.78, 0.2893, 0.3850),
            new(1_0, 8.253, 10.8, 0.3249, 0.4252),
            new(2_0, 9.266, 11.9, 0.3648, 0.4685),
            new(3_0, 10.41, 13.2, 0.4096, 0.5197),
            new(4_0, 11.68, 14.7, 0.4598, 0.5787)
        ];
    }
}

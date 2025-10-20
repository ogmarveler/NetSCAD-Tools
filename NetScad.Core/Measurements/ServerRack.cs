namespace NetScad.Core.Measurements
{
    public partial class ServerRack
    {
        public int RackUnits { get; set; }
        public double InnerWidthMm { get; set; }
        public double OuterWidthMm { get; set; }
        public double HeightMm { get; set; }
        public double InnerWidthInches => InnerWidthMm / 25.4;
        public double OuterWidthInches => OuterWidthMm / 25.4;
        public double HeightInches => HeightMm / 25.4;
        
        // Add display property for UI
        public string DisplayName => $"{RackUnits}U Height";
    }

    public static class ServerRackDimensions
    {
        private static readonly Dictionary<int, ServerRack> Dimensions = new()
        {
            { 1, new ServerRack { RackUnits = 1, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 44.45 } },
            { 2, new ServerRack { RackUnits = 2, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 88.9 } },
            { 3, new ServerRack { RackUnits = 3, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 133.35 } },
            { 4, new ServerRack { RackUnits = 4, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 177.8 } },
            { 5, new ServerRack { RackUnits = 5, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 222.25 } },
            { 6, new ServerRack { RackUnits = 6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 266.7 } },
            { 7, new ServerRack { RackUnits = 7, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 311.15 } },
            { 8, new ServerRack { RackUnits = 8, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 355.6 } },
            { 9, new ServerRack { RackUnits = 9, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 400.05 } },
            { 10, new ServerRack { RackUnits = 10, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 444.5 } },
            { 11, new ServerRack { RackUnits = 11, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 488.95 } },
            { 12, new ServerRack { RackUnits = 12, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 533.4 } }
        };

        public static ServerRack? GetByRackUnits(int rackUnits) => 
            Dimensions.TryGetValue(rackUnits, out var dimension) ? dimension : null;
        
        public static IEnumerable<ServerRack> GetAll() => Dimensions.Values;
    }
}
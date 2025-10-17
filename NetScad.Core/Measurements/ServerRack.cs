namespace NetScad.Core.Measurements
{
    public class ServerRack
    {
        public int RackUnits { get; set; }
        public double MountedWidthMm { get; set; } = 0;
        public double InnerWidthMm { get; set; }
        public double OuterWidthMm { get; set; }
        public double HeightMm { get; set; }
        public double MountedWidthInches => MountedWidthMm / 25.4f;
        public double InnerWidthInches => InnerWidthMm / 25.4f;
        public double OuterWidthInches => OuterWidthMm / 25.4f;
        public double HeightInches => HeightMm / 25.4f;
    }

    public static class ServerRackDimensions
    {
        private static readonly Dictionary<int, ServerRack> Dimensions = new()
        {
            { 1, new ServerRack { RackUnits = 1, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 44.45 } },
            { 2, new ServerRack { RackUnits = 2, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 88.9 } },
            { 3, new ServerRack { RackUnits = 3, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 133.35 } },
            { 4, new ServerRack { RackUnits = 4, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 177.8 } },
            { 5, new ServerRack { RackUnits = 5, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 222.25 } },
            { 6, new ServerRack { RackUnits = 6, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 266.7 } },
            { 7, new ServerRack { RackUnits = 7, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 311.15 } },
            { 8, new ServerRack { RackUnits = 8, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 355.6 } },
            { 9, new ServerRack { RackUnits = 9, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 400.05 } },
            { 10, new ServerRack { RackUnits = 10, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 444.5 } },
            { 11, new ServerRack { RackUnits = 11, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 488.95 } },
            { 12, new ServerRack { RackUnits = 12, MountedWidthMm = 482.6, InnerWidthMm = 450, OuterWidthMm = 482.6, HeightMm = 533.4 } }
        };

        public static ServerRack? GetByRackUnits(int rackUnits) => Dimensions.TryGetValue(rackUnits, out var dimension) ? dimension : null;
    }
}
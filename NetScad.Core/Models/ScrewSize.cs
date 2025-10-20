namespace NetScad.Core.Models
{
    public partial class ScrewSize(string name, double screwRadius, double screwHeadRadius, double threadedInsertRadius, double clearanceHoleRadius, double countersunkHeight)
    {
        public string Name { get; } = name;
        public double ScrewRadius { get; } = screwRadius;
        public double ScrewHeadRadius { get; } = screwHeadRadius;
        public double ThreadedInsertRadius { get; } = threadedInsertRadius;
        public double ClearanceHoleRadius { get; } = clearanceHoleRadius;
        public double CountersunkHeight { get; } = countersunkHeight;
    }
}

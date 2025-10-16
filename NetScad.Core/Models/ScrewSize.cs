namespace NetScad.Core.Models
{
    public partial class ScrewSize
    {
        public string Name { get; }
        public double ScrewRadius { get; }
        public double ScrewHeadRadius { get; }
        public double ThreadedInsertRadius { get; }
        public double ClearanceHoleRadius { get; }
        public double CountersunkHeight { get; }

        public ScrewSize(string name, double screwRadius, double screwHeadRadius, double threadedInsertRadius, double clearanceHoleRadius, double countersunkHeight)
        {
            Name = name;
            ScrewRadius = screwRadius;
            ScrewHeadRadius = screwHeadRadius;
            ThreadedInsertRadius = threadedInsertRadius;
            ClearanceHoleRadius = clearanceHoleRadius;
            CountersunkHeight = countersunkHeight;
        }
    }
}

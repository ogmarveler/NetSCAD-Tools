namespace NetScad.Core.Models
{
    public partial class ScrewSize
    {
        public double ScrewRadius { get; }
        public double ScrewHeadRadius { get; }
        public double ThreadedInsertRadius { get; }
        public double ClearanceHoleRadius { get; }
        public double CountersunkHeight { get; }

        public ScrewSize(double screwRadius, double screwHeadRadius, double threadedInsertRadius, double clearanceHoleRadius, double countersunkHeight)
        {
            ScrewRadius = screwRadius;
            ScrewHeadRadius = screwHeadRadius;
            ThreadedInsertRadius = threadedInsertRadius;
            ClearanceHoleRadius = clearanceHoleRadius;
            CountersunkHeight = countersunkHeight;
        }
    }
}

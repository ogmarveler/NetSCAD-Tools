using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public class ScrewSizeService : IScrewSizeService
    {
        public List<ScrewSize> ScrewSizes { get; } = new List<ScrewSize>
    {
        new ScrewSize("M2", 1.0, 2.0, 1.2, 1.15, 1.2),
        new ScrewSize("M2.5", 1.25, 2.5, 1.5, 1.45, 1.5),
        new ScrewSize("M3", 1.5, 3.0, 1.8, 1.75, 1.8),
        new ScrewSize("M4", 2.0, 4.0, 2.4, 2.3, 2.4),
        new ScrewSize("M5", 2.5, 5.0, 3.0, 2.9, 3.0),
        new ScrewSize("M6", 3.0, 6.0, 3.6, 3.5, 3.6),
        new ScrewSize("M8", 4.0, 8.0, 4.8, 4.7, 4.8),
        new ScrewSize("M10", 5.0, 10.0, 6.0, 5.9, 6.0)
    };
    }
}

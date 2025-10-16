using NetScad.Core.Models;

namespace NetScad.Core.Interfaces
{
    public interface IScrewSizeService
    {
        List<ScrewSize>? ScrewSizes { get; }
    }
}

using NetGenCAD.Core.Models;

namespace NetGenCAD.Core.Interfaces
{
    public interface IScrewSizeService
    {
        List<ScrewSize>? ScrewSizes { get; }
    }
}

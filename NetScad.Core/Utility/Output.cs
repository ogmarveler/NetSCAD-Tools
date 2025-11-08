using System.Text;

namespace NetScad.Core.Utility
{
    public class Output
    {
        public static async Task AppendToSCAD(string content, string filePath, CancellationToken cancellationToken = default)
        {
            // Check for update or new insert
            var _updated = UpdateFIle.UpdateFile(filePath, content);

            if (_updated) return;
            await File.AppendAllTextAsync(path: filePath, contents: content, encoding: Encoding.UTF8, cancellationToken: cancellationToken);
        }

        public static async Task WriteToSCAD(string content, string filePath, bool overWrite = false, CancellationToken cancellationToken = default)
        {
            if (!overWrite)
            {
                filePath = filePath.Replace(".scad", $"_{DateTime.Now:yyyyMMddHHmmss}.scad");
            }
            await File.WriteAllBytesAsync(path: filePath, bytes: Encoding.UTF8.GetBytes(content), cancellationToken: cancellationToken);
        }
    }
}
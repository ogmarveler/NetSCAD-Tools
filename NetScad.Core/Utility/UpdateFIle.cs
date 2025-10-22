namespace NetScad.Core.Utility
{
    public static class UpdateFIle
    {
        public static bool UpdateFile(string filePath, string codeBlock)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains(codeBlock)) return false;
                content = content.Replace(codeBlock, codeBlock);
                File.WriteAllText(filePath, content);
                Console.WriteLine("Replacement successful.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public static bool ChangeContentBlockFile(string filePath, string oldCodeBlock, string newCodeBlock)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains(oldCodeBlock)) return false;
                content = content.Replace(oldCodeBlock, newCodeBlock);
                File.WriteAllText(filePath, content);
                Console.WriteLine("Replacement successful.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}

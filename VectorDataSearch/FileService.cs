using System.Text;

namespace VectorDataSearch
{
    public static class FileService
    {
        [System.ComponentModel.Description("Read the full content of a local text file. Use for code files, notes, markdown, logs, etc. Always use absolute or relative path from project root.")]
        public static string ReadFile(string filePath)
        {
            try
            {
                Console.WriteLine($" [TOOL CALLED] ReadFile → {filePath}");

                if (!File.Exists(filePath))
                {
                    // Try relative to project if absolute fails
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    if (File.Exists(fullPath))
                        filePath = fullPath;
                    else
                        return $"File not found: {filePath}";
                }

                string content = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Limit length to avoid blowing up context
                if (content.Length > 5000)
                    content = content.Substring(0, 5000) + "\n\n... [Content truncated - file is very long]";

                return $"File: {filePath}\n\n{content}";
            }
            catch (Exception ex)
            {
                return $"Error reading file '{filePath}': {ex.Message}";
            }
        }

        [System.ComponentModel.Description("List files and folders in a directory")]
        public static string ListDirectory(string directoryPath = ".")
        {
            try
            {
                Console.WriteLine($" [TOOL CALLED] ListDirectory → {directoryPath}");

                if (directoryPath == ".") 
                    directoryPath = Directory.GetCurrentDirectory();

                var files = Directory.GetFiles(directoryPath);
                var dirs = Directory.GetDirectories(directoryPath);

                var sb = new StringBuilder();
                sb.AppendLine($"Directory: {directoryPath}");
                sb.AppendLine($"Folders ({dirs.Length}):");
                foreach (var d in dirs) sb.AppendLine($"  📁 {Path.GetFileName(d)}");
                sb.AppendLine($"Files ({files.Length}):");
                foreach (var f in files) sb.AppendLine($"  📄 {Path.GetFileName(f)}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error listing directory: {ex.Message}";
            }
        }
    }
}
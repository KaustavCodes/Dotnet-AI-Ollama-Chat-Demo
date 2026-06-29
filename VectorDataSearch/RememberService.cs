using System.Text.Json;
using System.Text.Json.Serialization;

namespace VectorDataSearch
{
    public static class RememberService
    {
        private static readonly string _filePath;

        private static List<string> _rememberedItems = new List<string>();

        static RememberService()
        {
            // This puts remember.json in the project root (not in bin folder)
            string projectRoot = FindProjectRoot();
            _filePath = Path.Combine(projectRoot, "remember.json");

            Console.WriteLine($" [RememberService] Using file: {_filePath}");

            LoadFromFile();
        }

        private static string FindProjectRoot()
        {
            // Start from current directory and go up until we find .csproj
            string current = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(current))
            {
                if (Directory.GetFiles(current, "*.csproj").Length > 0)
                {
                    return current;
                }

                var parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }

            // Fallback: Use the directory one level above bin
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
        }

        private static void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var items = JsonSerializer.Deserialize<List<string>>(json);
                    if (items != null)
                        _rememberedItems = items;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [RememberService] Warning: Could not load {_filePath} - {ex.Message}");
            }
        }

        private static void SaveToFile()
        {
            try
            {
                string json = JsonSerializer.Serialize(_rememberedItems, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [RememberService] Error saving to {_filePath}: {ex.Message}");
            }
        }

        [System.ComponentModel.Description("Remember something important for future conversations. Use this when user says 'remember', 'save this', 'don't forget', etc.")]
        public static string AddToRemember(string thingToRemember)
        {
            Console.WriteLine($" [TOOL CALLED] Remember → {thingToRemember}");
            
            _rememberedItems.Add(thingToRemember);
            SaveToFile();

            return $"Remembered: \"{thingToRemember}\"";
        }

        [System.ComponentModel.Description("Get all things I have asked you to remember.")]
        public static string GetAllRememberedItems()
        {
            Console.WriteLine($" [TOOL CALLED] Get All Remembered Items");

            if (_rememberedItems.Count == 0)
                return "You haven't remembered anything yet.";

            return "📋 Remembered Items:\n" + 
                   string.Join("\n", _rememberedItems.Select((item, index) => $"{index + 1}. {item}"));
        }

        [System.ComponentModel.Description("Forget a specific remembered item. You can give the exact text or the item number (e.g. 1).")]
        public static string ForgetItem(string itemToForget)
        {
            Console.WriteLine($" [TOOL CALLED] Forget Item → '{itemToForget}'");

            if (string.IsNullOrWhiteSpace(itemToForget))
                return "Please tell me what to forget.";

            string input = itemToForget.Trim();

            bool removed = false;
            string removedText = input;

            // 1. Exact match
            if (_rememberedItems.Remove(input))
            {
                removed = true;
            }
            // 2. Case-insensitive exact match
            else
            {
                var exactMatch = _rememberedItems.FirstOrDefault(x => 
                    x.Trim().Equals(input, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    removed = _rememberedItems.Remove(exactMatch);
                    removedText = exactMatch;
                }
            }

            // 3. Fuzzy / Contains match (this catches "every day" vs "everyday")
            if (!removed)
            {
                var fuzzyMatch = _rememberedItems.FirstOrDefault(x =>
                    x.Trim().Replace(" ", "").Equals(input.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                    x.Contains(input, StringComparison.OrdinalIgnoreCase) ||
                    input.Contains(x, StringComparison.OrdinalIgnoreCase));

                if (fuzzyMatch != null)
                {
                    removed = _rememberedItems.Remove(fuzzyMatch);
                    removedText = fuzzyMatch;
                }
            }

            // 4. By index number
            if (!removed && int.TryParse(input, out int index) && index > 0 && index <= _rememberedItems.Count)
            {
                removedText = _rememberedItems[index - 1];
                _rememberedItems.RemoveAt(index - 1);
                removed = true;
            }

            if (removed)
            {
                SaveToFile();
                Console.WriteLine($"   ✅ Successfully removed: '{removedText}'");
                return $"✅ Forgotten: \"{removedText}\"";
            }

            // Debug information
            Console.WriteLine($"   ❌ Could not find item. Current memory ({_rememberedItems.Count} items):");
            for (int i = 0; i < _rememberedItems.Count; i++)
            {
                Console.WriteLine($"     {i+1}. \"{ _rememberedItems[i] }\"");
            }

            return $"❌ Could not find \"{itemToForget}\" in remembered items.";
        }

        [System.ComponentModel.Description("Forget all remembered items.")]
        public static string ForgetAll()
        {
            _rememberedItems.Clear();
            SaveToFile();
            return "Cleared all remembered items.";
        }
    }
}
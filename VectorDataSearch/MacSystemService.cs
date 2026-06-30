using System.Diagnostics;

namespace VectorDataSearch
{
    public static class MacSystemService
    {
        // ==================== VOLUME CONTROLS ====================
        [System.ComponentModel.Description("Set the system output volume (0-100)")]
        public static string SetVolume(int volumeLevel)
        {
            Console.WriteLine($" [TOOL CALLED] SetVolume → {volumeLevel}");
            volumeLevel = Math.Clamp(volumeLevel, 0, 100);

            ExecuteAppleScript($"set volume output volume {volumeLevel}");
            return $"✅ Volume set to {volumeLevel}%";
        }

        [System.ComponentModel.Description("Adjust volume (positive = increase, negative = decrease)")]
        public static string AdjustVolume(int change)
        {
            Console.WriteLine($" [TOOL CALLED] AdjustVolume → {change}");
            change = Math.Clamp(change, -50, 50);

            ExecuteAppleScript($"set volume output volume (output volume of (get volume settings) + {change})");
            return $"✅ Volume adjusted by {change}%";
        }

        [System.ComponentModel.Description("Get current system volume")]
        public static string GetCurrentVolume()
        {
            Console.WriteLine($" [TOOL CALLED] GetCurrentVolume");

            string result = ExecuteAppleScript("output volume of (get volume settings)");
            return $"Current volume is {result.Trim()}%";
        }

        // ==================== DARK MODE ====================
        [System.ComponentModel.Description("Turn Dark Mode On or Off")]
        public static string SetDarkMode(bool enable)
        {
            Console.WriteLine($" [TOOL CALLED] SetDarkMode → {enable}");

            string script = enable
                ? @"tell application ""System Events"" to tell appearance preferences to set dark mode to true"
                : @"tell application ""System Events"" to tell appearance preferences to set dark mode to false";

            ExecuteAppleScript(script);
            return enable ? "✅ Dark Mode turned **ON**" : "✅ Dark Mode turned **OFF**";
        }

        // ==================== NIGHT SHIFT ====================
        [System.ComponentModel.Description("Turn Night Shift On or Off")]
        public static string SetNightShift(bool enable)
        {
            Console.WriteLine($" [TOOL CALLED] SetNightShift → {enable}");

            string script = enable
                ? @"tell application ""System Events"" to tell appearance preferences to set night shift enabled to true"
                : @"tell application ""System Events"" to tell appearance preferences to set night shift enabled to false";

            ExecuteAppleScript(script);
            return enable ? "✅ Night Shift turned **ON**" : "✅ Night Shift turned **OFF**";
        }

        // ==================== STATUS ====================
        [System.ComponentModel.Description("Get current Dark Mode and Night Shift status")]
        public static string GetDisplayModeStatus()
        {
            Console.WriteLine($" [TOOL CALLED] GetDisplayModeStatus");

            try
            {
                string darkMode = ExecuteAppleScript(
                    @"tell application ""System Events"" to tell appearance preferences to get dark mode");

                string nightShift = ExecuteAppleScript(
                    @"tell application ""System Events"" to tell appearance preferences to get night shift enabled");

                bool isDark = darkMode.Trim().ToLower() == "true";
                bool isNight = nightShift.Trim().ToLower() == "true";

                return $"**Dark Mode:** {(isDark ? "ON" : "OFF")}\n" +
                       $"**Night Shift:** {(isNight ? "ON" : "OFF")}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error getting status: {ex.Message}");
                return "Failed to retrieve Dark Mode / Night Shift status.";
            }
        }

        // ==================== HELPER (Improved escaping) ====================
        private static string ExecuteAppleScript(string script)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "osascript",
                        // Removed the manually escaped 'Arguments' string
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                // Let .NET handle the argument boundaries and escaping natively
                process.StartInfo.ArgumentList.Add("-e");
                process.StartInfo.ArgumentList.Add(script);

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"   AppleScript Warning: {error}");

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   AppleScript Error: {ex.Message}");
                return "";
            }
        }
    }
}
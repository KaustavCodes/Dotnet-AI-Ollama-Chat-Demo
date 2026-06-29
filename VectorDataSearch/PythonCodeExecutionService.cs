using System.Diagnostics;
using System.Text;

namespace VectorDataSearch
{
    public static class PythonCodeExecutionService
    {
        private static readonly string[] PythonCommands = { "python3", "python", "/usr/bin/python3", "/opt/homebrew/bin/python3" };

        [System.ComponentModel.Description(
            "Execute Python 3 code for advanced math, symbolic computation, quadratic/cubic equations, integrals, optimization, etc. " +
            "You can use math, sympy, numpy if installed. Prefer sympy for exact solutions.")]
        public static async Task<string> ExecutePythonCode(string pythonCode)
        {
            Console.WriteLine($" [TOOL CALLED] ExecutePythonCode");
            Console.WriteLine($"   Code to run:\n{pythonCode}");

            try
            {
                if (string.IsNullOrWhiteSpace(pythonCode) || pythonCode.Length > 4000)
                    return "Error: Code is empty or too long.";

                // Safety checks
                string lower = pythonCode.ToLowerInvariant();
                string[] blocked = { "open(", "os.", "subprocess", "exec(", "eval(", "requests", "urllib", "shutil", "glob" };
                if (blocked.Any(b => lower.Contains(b)))
                    return "Error: Dangerous operations blocked.";

                foreach (var cmd in PythonCommands)
                {
                    try
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = cmd,
                                Arguments = $"-c \"{pythonCode.Replace("\"", "\\\"")}\"",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = Directory.GetCurrentDirectory()
                            }
                        };

                        Console.WriteLine($"   🚀 Running with {cmd}...");

                        process.Start();

                        // Read output with timeout
                        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        await process.WaitForExitAsync(timeoutCts.Token);

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"   ✅ Success using {cmd}");
                            return string.IsNullOrWhiteSpace(output.Trim()) 
                                ? "Code executed successfully (no output)." 
                                : output.Trim();
                        }
                        else if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"   Python Error: {error}");
                            if (error.Contains("No module named"))
                                return $"Missing module: {error}";
                        }
                    }
                    catch { continue; }
                }

                return "Error: Could not execute Python. Make sure Python 3 is installed and accessible.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Execution Failed: {ex.Message}");
                return $"Execution failed: {ex.Message}";
            }
        }
    }
}
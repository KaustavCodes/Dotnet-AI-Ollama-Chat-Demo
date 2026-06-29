using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VectorDataSearch
{
    public static class CodeExecutionService
    {
        private static readonly ScriptOptions _safeOptions = ScriptOptions.Default
            .WithImports("System", "System.Linq", "System.Math", "System.Globalization")
            .WithReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(Math).Assembly
            )
            .WithAllowUnsafe(false);

        private static readonly HashSet<string> _blockedKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "file", "directory", "io", "process", "http", "web", "socket", "assembly",
            "reflection", "activator", "environment", "registry", "thread", "task.run",
            "appdomain", "unsafe", "pointer", "marshal", "native", "dllimport",
            "import ", "def ", "print(", "numpy", "pandas"   // Block Python keywords
        };

        [System.ComponentModel.Description(
            "Execute safe C# code only. Use this for complex calculations, logic, or quadratic equations. " +
            "Must be valid C# syntax. Do not use Python.")]
        public static async Task<string> ExecuteCode(string code)
        {
            Console.WriteLine($" [TOOL CALLED] ExecuteCode");
            Console.WriteLine($"   Received code:\n{code}");

            try
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length > 2000)
                    return "Error: Code is empty or too long.";

                string lower = code.ToLowerInvariant();

                if (_blockedKeywords.Any(k => lower.Contains(k)))
                {
                    Console.WriteLine("   ❌ Blocked: Contains Python keywords or dangerous operations");
                    return "Error: Please use **C#** syntax only. Do not use Python (no 'import', 'def', 'print', numpy, etc.)";
                }

                Console.WriteLine("   🚀 Executing C# code...");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var result = await CSharpScript.EvaluateAsync(code, _safeOptions, cancellationToken: cts.Token);

                string output = result?.ToString() ?? "(null)";

                Console.WriteLine($"   ✅ Success! Result: {output}");

                return output.Length > 1500 ? output.Substring(0, 1500) + "... [truncated]" : output;
            }
            catch (CompilationErrorException ex)
            {
                Console.WriteLine($"   ❌ Compilation Error");
                return $"Compilation Error (use valid C#):\n{string.Join("\n", ex.Diagnostics.Select(d => d.GetMessage()))}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Execution Error: {ex.Message}");
                return $"Execution Error: {ex.Message}";
            }
        }
    }
}
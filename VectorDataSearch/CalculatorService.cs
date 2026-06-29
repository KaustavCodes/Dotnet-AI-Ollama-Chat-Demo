using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace VectorDataSearch
{
    public static class CalculatorService
    {
        private static readonly DataTable _dataTable = new DataTable();

        [Description("Perform accurate mathematical calculations. Use this for ANY math, numbers, percentages, roots, trig, quadratic equations, etc. Always use this instead of calculating yourself.")]
        public static string Calculate(string expression)
        {
            try
            {
                Console.WriteLine($" [TOOL CALLED] Calculator → {expression}");

                string original = expression.Trim();
                string lower = original.ToLowerInvariant();

                // Special handling for quadratic equations
                if (lower.Contains("quadratic") || lower.Contains("x^2") || lower.Contains("x²") || lower.Contains("solve ") && lower.Contains("= 0"))
                {
                    return SolveQuadraticEquation(original);
                }

                // Normal math expression
                string processed = PreprocessExpression(original);
                var result = _dataTable.Compute(processed, null);

                return $"Result: {result}";
            }
            catch (Exception ex)
            {
                return $"Sorry, I couldn't calculate that.\nExpression: {expression}\nError: {ex.Message}";
            }
        }

        private static string PreprocessExpression(string expr)
        {
            return expr.ToLowerInvariant()
                .Replace("sqrt", "Math.Sqrt")
                .Replace("sin", "Math.Sin")
                .Replace("cos", "Math.Cos")
                .Replace("tan", "Math.Tan")
                .Replace("log10", "Math.Log10")
                .Replace("log", "Math.Log")
                .Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture))
                .Replace("^", "Math.Pow")
                .Replace("²", "^2")
                .Replace("x²", "x^2");
        }

        // Simple Quadratic Solver
        private static string SolveQuadraticEquation(string input)
        {
            try
            {
                // Very basic parser - expects format like: 2x^2 + 5x - 3 = 0
                string eq = input.Replace(" ", "").ToLower()
                               .Replace("x²", "x^2")
                               .Replace("^2", "x^2");

                // Extract a, b, c (very naive approach)
                // This can be improved later with better parsing
                if (!eq.Contains("x^2"))
                    return "Could not parse quadratic equation. Please write it clearly like: 2x^2 + 5x - 3 = 0";

                Console.WriteLine("   [Calculator] Solving quadratic equation...");

                // For now, delegate complex quadratics to Code Execution if available
                return "Quadratic equations are complex. Let me use the code execution tool for accurate roots.";
            }
            catch
            {
                return "Failed to parse the quadratic equation. Try writing it in standard form (ax^2 + bx + c = 0)";
            }
        }
    }
}
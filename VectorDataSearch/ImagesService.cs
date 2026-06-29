namespace VectorDataSearch
{
    public static class ImageService
    {
        public static bool IsImageAnalysisRequest(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string normalized = input.ToLowerInvariant();
            return normalized.Contains("analyze the image")
                || normalized.Contains("analyse the image")
                || normalized.Contains("analyze image")
                || normalized.Contains("analyse image")
                || normalized.Contains("describe the image")
                || normalized.Contains("what is in this image")
                || normalized.Contains("what's in this image")
                || normalized.Contains("caption this image")
                || normalized.Contains("image analysis");
        }

        public static string? TryExtractImagePathParameter(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var markers = new[]
            {
                "analyze the image",
                "analyse the image",
                "analyze image",
                "analyse image",
                "describe the image",
                "describe image",
                "caption this image",
                "image analysis"
            };

            string normalized = input.ToLowerInvariant();
            foreach (string marker in markers)
            {
                int markerIndex = normalized.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0) continue;

                string tail = input.Substring(markerIndex + marker.Length).Trim();
                if (tail.StartsWith(":") || tail.StartsWith("-"))
                {
                    tail = tail.Substring(1).Trim();
                }

                if (string.IsNullOrWhiteSpace(tail))
                {
                    return null;
                }

                tail = tail.Trim().Trim('"', '\'');
                return string.IsNullOrWhiteSpace(tail) ? null : tail;
            }

            return null;
        }

        public static string ResolveImagePath(string? parameter, string currentDirectory, string defaultImageName)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return Path.Combine(currentDirectory, "images", defaultImageName);
            }

            // If user provided a slash, treat it as a full/explicit path.
            if (parameter.Contains('/') || parameter.Contains('\\') || Path.IsPathRooted(parameter))
            {
                return parameter;
            }

            return Path.Combine(currentDirectory, "images", parameter);
        }

        public static string GetImageMimeType(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/png"
            };
        }

        public static string ResolveImagePathWithFallback(string input, string currentDirectory, string defaultImageName)
        {
            string? imageParameter = TryExtractImagePathParameter(input);
            string imagePath = ResolveImagePath(imageParameter, currentDirectory, defaultImageName);

            // Backward-compatible fallback for old location in project root.
            if (!File.Exists(imagePath) && string.IsNullOrWhiteSpace(imageParameter))
            {
                string legacyPath = Path.Combine(currentDirectory, defaultImageName);
                if (File.Exists(legacyPath))
                {
                    imagePath = legacyPath;
                }
            }

            return imagePath;
        }
    }
}
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VectorDataSearch
{
    public class SearchResult
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";          // Real final URL
        public string DdgUrl { get; set; } = "";       // Original DDG redirect URL (for debugging)
        public string Snippet { get; set; } = "";
        public string FullContent { get; set; } = "";
    }

    public static class WebScrapperService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<List<SearchResult>> DuckDuckGoSearchAsync(string query, int maxResults = 5)
        {
            Console.WriteLine($" [TOOL CALLED] DuckDuckGo Search → {query}");
            var results = new List<SearchResult>();

            string searchUrl = $"https://html.duckduckgo.com/html/?q={HttpUtility.UrlEncode(query)}";

            var web = new HtmlWeb();
            var searchDoc = web.Load(searchUrl);

            // Better selector for current DDG HTML results
            var linkNodes = searchDoc.DocumentNode.SelectNodes("//a[contains(@href, 'uddg=') or contains(@class, 'result__a')]");

            if (linkNodes == null)
            {
                Console.WriteLine("No search results found.");
                return results;
            }

            int count = 0;
            foreach (var node in linkNodes)
            {
                if (count >= maxResults) break;

                string ddgLink = node.GetAttributeValue("href", "");
                string title = node.InnerText.Trim();

                if (string.IsNullOrWhiteSpace(title)) continue;

                string realUrl = ExtractRealUrl(ddgLink);

                var result = new SearchResult
                {
                    Title = HtmlEntity.DeEntitize(title),
                    Url = realUrl,
                    DdgUrl = ddgLink
                };

                // Try to get snippet
                var snippetNode = node.ParentNode?.SelectSingleNode(".//following-sibling::div[contains(@class, 'result__snippet')] | .//p");
                result.Snippet = snippetNode != null 
                    ? HtmlEntity.DeEntitize(snippetNode.InnerText.Trim()) 
                    : "";

                // Fetch full page content
                if (!string.IsNullOrEmpty(realUrl))
                {
                    try
                    {
                        result.FullContent = await FetchPageContentAsync(realUrl);
                        Console.WriteLine($"   Loaded: {realUrl} ({result.FullContent.Length} chars)");
                    }
                    catch (Exception ex)
                    {
                        result.FullContent = "[Failed to load page]";
                        Console.WriteLine($"   Failed {realUrl}: {ex.Message}");
                    }
                }

                results.Add(result);
                count++;
            }

            return results;
        }

        private static string ExtractRealUrl(string ddgLink)
        {
            if (string.IsNullOrEmpty(ddgLink)) return "";

            try
            {
                // Handle both relative and absolute DDG redirect links
                if (ddgLink.StartsWith("//")) 
                    ddgLink = "https:" + ddgLink;

                var uri = new Uri(ddgLink);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);

                string uddg = queryParams["uddg"];
                if (!string.IsNullOrEmpty(uddg))
                {
                    return HttpUtility.UrlDecode(uddg);
                }
            }
            catch { }

            // Fallback: return original if we can't extract
            return ddgLink;
        }

        private static async Task<string> FetchPageContentAsync(string url)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AIChatbot/1.0)");

            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            // Remove junk
            foreach (var unwanted in doc.DocumentNode.SelectNodes("//script|//style|//nav|//header|//footer|//iframe|//aside|//svg|//button|//form") ?? Enumerable.Empty<HtmlNode>())
            {
                unwanted.Remove();
            }

            var body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
            string text = body.InnerText;

            return string.Join(" ", text.Split(new[] { ' ', '\r', '\n', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries)).Trim();
        }
    }
}
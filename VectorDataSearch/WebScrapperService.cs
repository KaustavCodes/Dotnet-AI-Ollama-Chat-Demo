// This file is part of the VectorDataSearch project.
// Warning: This is a testing / learning project only. 
// Web scraping is fragile and often against website ToS. 
// Never use this in production. Prefer official APIs whenever possible.

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
        public string Url { get; set; } = "";
        public string DdgUrl { get; set; } = "";
        public string Snippet { get; set; } = "";
        public string FullContent { get; set; } = "";
    }

    public static class WebScrapperService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        private static readonly Random _random = new Random();

        static WebScrapperService()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }

        public static async Task<List<SearchResult>> DuckDuckGoSearchAsync(string query, int maxResults = 5)
        {
            Console.WriteLine($" [TOOL CALLED] DuckDuckGo Search → {query}");
            var results = new List<SearchResult>();

            string searchUrl = $"https://html.duckduckgo.com/html/?q={HttpUtility.UrlEncode(query)}";

            var web = new HtmlWeb();
            var searchDoc = web.Load(searchUrl);

            var linkNodes = searchDoc.DocumentNode.SelectNodes("//a[contains(@href, 'uddg=') or contains(@class, 'result__a')]");

            if (linkNodes == null || linkNodes.Count == 0)
            {
                Console.WriteLine("No search results found.");
                return results;
            }

            int count = 0;
            foreach (var node in linkNodes)
            {
                if (count >= maxResults) break;

                string ddgLink = node.GetAttributeValue("href", "");
                string title = HtmlEntity.DeEntitize(node.InnerText.Trim());

                if (string.IsNullOrWhiteSpace(title)) continue;

                string realUrl = ExtractRealUrl(ddgLink);

                var result = new SearchResult
                {
                    Title = title,
                    Url = realUrl,
                    DdgUrl = ddgLink
                };

                // Get snippet
                var snippetNode = node.ParentNode?.SelectSingleNode(".//following-sibling::div[contains(@class,'result__snippet')] | .//p");
                result.Snippet = snippetNode != null 
                    ? HtmlEntity.DeEntitize(snippetNode.InnerText.Trim()) 
                    : "";

                // Fetch full content with retries
                if (!string.IsNullOrEmpty(realUrl))
                {
                    result.FullContent = await FetchPageContentWithRetryAsync(realUrl);
                    Console.WriteLine($"   Loaded: {realUrl} ({result.FullContent.Length} chars)");
                }

                results.Add(result);
                count++;

                await Task.Delay(_random.Next(1000, 2200)); // Polite delay
            }

            return results;
        }

        private static string ExtractRealUrl(string ddgLink)
        {
            if (string.IsNullOrEmpty(ddgLink)) return "";

            try
            {
                if (ddgLink.StartsWith("//")) ddgLink = "https:" + ddgLink;

                var uri = new Uri(ddgLink);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                string uddg = queryParams["uddg"];

                return !string.IsNullOrEmpty(uddg) ? HttpUtility.UrlDecode(uddg) : ddgLink;
            }
            catch { }

            return ddgLink;
        }

        private static async Task<string> FetchPageContentWithRetryAsync(string url, int maxRetries = 2)
        {
            for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
            {
                try
                {
                    // Rotate User-Agent every attempt
                    _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GetRandomUserAgent());

                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string html = await response.Content.ReadAsStringAsync();

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Clean unwanted elements
                        foreach (var unwanted in doc.DocumentNode.SelectNodes(
                            "//script|//style|//nav|//header|//footer|//iframe|//aside|//svg|//button|//form|//noscript|//comment") 
                            ?? Enumerable.Empty<HtmlNode>())
                        {
                            unwanted.Remove();
                        }

                        var body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
                        string text = body.InnerText;

                        return string.Join(" ", text.Split(new[] { ' ', '\r', '\n', '\t' }, 
                            StringSplitOptions.RemoveEmptyEntries)).Trim();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine($" 403 Forbidden on attempt {attempt}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Attempt {attempt} failed: {ex.Message}");
                }

                if (attempt <= maxRetries)
                    await Task.Delay(_random.Next(1500, 3000));
            }

            return "[Content blocked or unavailable - 403/timeout]";
        }

        private static string GetRandomUserAgent()
        {
            var userAgents = new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:138.0) Gecko/20100101 Firefox/138.0",
                "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"
            };

            return userAgents[_random.Next(userAgents.Length)];
        }
    }
}
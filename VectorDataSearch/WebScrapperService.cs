using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VectorDataSearch
{
    public class SearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
    }

    public static class WebScrapperService
    {
        public static List<SearchResult> DuckDuckGoSearch(string query, int maxResults = 5)
        {
            var results = new List<SearchResult>();
            string url = $"https://html.duckduckgo.com/html/?q={HttpUtility.UrlEncode(query)}";

            var web = new HtmlWeb();
            var doc = web.Load(url);

            var resultNodes = doc.DocumentNode.SelectNodes("//div ");

            if (resultNodes != null)
            {
                foreach (var node in resultNodes.Take(maxResults))
                {
                    var titleNode = node.SelectSingleNode(".//a ");
                    var snippetNode = node.SelectSingleNode(".//a[contains(@class, 'result__snippet')]");

                    if (titleNode != null)
                    {
                        results.Add(new SearchResult
                        {
                            Title = titleNode.InnerText.Trim(),
                            Url = titleNode.GetAttributeValue("href", ""),
                            Snippet = snippetNode?.InnerText.Trim() ?? ""
                        });
                    }
                }
            }

            return results;
        }
    }

    
}


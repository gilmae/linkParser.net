using AngleSharp.Html.Parser;
using RestSharp;
using System.Text.RegularExpressions;

namespace LinkParser;
public class Parser
{
    const string paramsRegexPattern = "(\\w+)=['\"]?([^\"']+)['\"]?";
    const string urlRegex = "(?<=<).+?(?=>)";

    private readonly Uri _baseUrl;

    private IList<HttpLink> _links;

    public IList<HttpLink> Links { get { return _links; } }

    internal Parser(Uri baseUrl)
    {
        _baseUrl = baseUrl;
        _links = new List<HttpLink>();
    }

    public static Parser Parse(Uri url)
    {
        RestClient client = new RestClient(url);

        // Handle Redirects
        var request = new RestRequest();
        var response = client.Get(request);
        if (response == null)
        {
            return new Parser(url);
        }
        var header = response.Headers?.FirstOrDefault(h => h.Name?.ToLower() == "rel")?.Value?.ToString() ?? "";
        var body = response.Content ?? "";

        return Parse(url, body, header);
    }

    public static Parser Parse(Uri baseUrl, string body, string header)
    {
        Parser p = new Parser(baseUrl);
        p.FindLinks(header, body);
        return p;
    }

    private void FindLinks(string header, string body)
    {
        ParseHttpLinkHeader(header);
        

        ParseBody(body);
    }

    private void ParseBody(string body)
    {
        var parser = new HtmlParser(new HtmlParserOptions
        {
            IsNotConsumingCharacterReferences = true,
        });
        var doc = parser.ParseDocument(body);
        var bodyLinks = doc
             .QuerySelectorAll("link, body a");

        foreach (var l in bodyLinks)
        {
            string? href = l.GetAttribute("href");
            if (string.IsNullOrEmpty(href))
            {
                continue;
            }
            if (Uri.IsWellFormedUriString(href, UriKind.Relative))
            {
                href = new Uri(_baseUrl, href).ToString();
            }

            HttpLink link = new HttpLink { Url = href };

            link.Rel = l.Attributes["rel"]?.Value??"";
            foreach (var a in l.Attributes) {
                link.Params[a.Name] = a.Value;
            }

            _links.Add(link);
        }
    }

    private void ParseHttpLinkHeader(string header)
    {
        var links = header.Split(',');
        foreach (string l in links)
        {
            string? url = Regex.Match(l, urlRegex, RegexOptions.IgnoreCase)?.Groups[0]?.Value;
            if (url == null)
            {
                continue;
            }

            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                url = new Uri(_baseUrl, url).ToString();
            }
            HttpLink link = new HttpLink { Url = url, InHeaders=true };
            var linkParams = Regex.Matches(l, paramsRegexPattern, RegexOptions.IgnoreCase)
                                    .SelectMany(m =>
                                        m.Captures.Where(c => c is Match && (c as Match)?.Groups?.Count == 3))
                                    .Select(c =>
                        new
                        {
                            Name = (c as Match)?.Groups[1]?.Value,
                            Value = (c as Match)?.Groups[2]?.Value
                        }
                                    );
            link.Rel = linkParams.FirstOrDefault(i => i.Name == "rel")?.Value ?? "";
            foreach (var p in linkParams)
            {
                if (string.IsNullOrEmpty(p.Name))
                {
                    continue;
                }
                if (link.Params.ContainsKey(p.Name))
                {
                    continue;
                }
                link.Params[p.Name] = p.Value ?? "";
            }
            _links.Add(link);
        }
    }

    public class HttpLink
    {
        public string Rel { get; set; } = default!;
        public string Url { get; set; } = default!;
        public Dictionary<string, string> Params { get; set; }
        public bool InHeaders { get; set; }

        public HttpLink()
        {
            Params = new Dictionary<string, string>();
        }
    }
}




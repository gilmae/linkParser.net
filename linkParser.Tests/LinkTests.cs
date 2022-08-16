namespace LinkParser.Tests;

public class LinkTests
{
    private readonly  Uri baseUri = new Uri("http://example.com");

    [Fact]
    public void ParsesWellFormedAnchor()
    {
        var body = "<link href=\"http://example.com/TheBook/chapter2\" rel=\"previous\" title=\"previous chapter\" />";
        
        var p = Parser.Parse(baseUri, body, "");

        Assert.Equal(1, p.Links.Count());
        Assert.Equal("http://example.com/TheBook/chapter2", p.Links[0].Url);
        Assert.Equal("previous", p.Links[0].Rel);
        Assert.Equal("previous chapter", p.Links[0].Title);
    }

    [Fact]
    public void IgnoresMultipleRelAttributes()
    {
        var body = "<link href=\"http://example.com/TheBook/chapter2\" rel=\"previous\" rel=\"invalid\" title=\"previous chapter\" />";

        var p = Parser.Parse(baseUri, body, "");

        Assert.Equal("previous", p.Links[0].Rel);
    }

    [Fact]
    public void IgnoresMultipleTitleAttributes()
    {
        var body = "<link a href=\"http://example.com/TheBook/chapter2\" rel=\"previous\" title=\"previous chapter\" title=\"invalid\" />";

        var p = Parser.Parse(baseUri, body, "");

        Assert.Equal("previous chapter", p.Links[0].Title);

    }

    [Fact]
    public void CanonacalisesUrlFragmentsWithBaseUrl()
    {
        var body = "<link href=\"/TheBook/chapter2\" rel=\"previous\" title=\"previous chapter\" /k>";

        var p = Parser.Parse(baseUri, body, "");

        Assert.Equal("http://example.com/TheBook/chapter2", p.Links[0].Url);
    }
}

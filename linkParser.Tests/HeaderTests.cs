namespace LinkParser.Tests;

public class HeaderTests
{
    private readonly  Uri baseUri = new Uri("http://example.com");

    [Fact]
    public void ParsesWellFormedHeader()
    {
        var header = "<http://example.com/TheBook/chapter2>; rel=\"previous\"; title=\"previous chapter\"";
        
        var p = Parser.Parse(baseUri, "", header);

        Assert.Equal(1, p.Links.Count());
        Assert.Equal("http://example.com/TheBook/chapter2", p.Links[0].Url);
        Assert.Equal("previous", p.Links[0].Rel);
        Assert.Equal("previous chapter", p.Links[0].Title);
    }

    [Fact]
    public void IgnoresMultipleRelAttributes()
    {
        var header = "<http://example.com/TheBook/chapter2>; rel=\"previous\"; rel=\"first\" title=\"previous chapter\"";
        

        var p = Parser.Parse(baseUri, "", header);

        Assert.Equal("previous", p.Links[0].Rel);
    }

    [Fact]
    public void IgnoresMultipleTitleAttributes()
    {
        var header = "<http://example.com/TheBook/chapter2>; rel=\"previous\"; rel=\"first\" title=\"previous chapter\" title=\"invalid\"";
        

        var p = Parser.Parse(baseUri, "", header);

        Assert.Equal("previous chapter", p.Links[0].Title);

    }

    [Fact]
    public void CanonacalisesUrlFragmentsWithBaseUrl()
    {
        var header = "</TheBook/chapter2>; rel=\"previous\"; rel=\"first\" title=\"previous chapter\"";

        var p = Parser.Parse(baseUri, "", header);

        Assert.Equal("http://example.com/TheBook/chapter2", p.Links[0].Url);
    }
}

namespace WordbreakMiddleware.Tests;

public class WordBreakProcessorTests
{
    private WordBreakProcessor CreateProcessor(int minimumCharacters = 10, string wordBreakCharacters = "<wbr>")
    {
        var options = new WordbreakMiddlewareOptions
        {
            MinimumCharacters = minimumCharacters,
            WordBreakCharacters = wordBreakCharacters
        };
        return new WordBreakProcessor(options);
    }
    
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("   ", "   ")]
    public void ProcessText_HandlesEmptyOrWhitespace(string input, string expected)
    {
        var processor = CreateProcessor();
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void ProcessText_HandlesNull()
    {
        var processor = CreateProcessor();
        var result = processor.ProcessText(null!);
        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("short", "short")]
    [InlineData("ShortWord", "ShortWord")]
    [InlineData("ALLCAPS", "ALLCAPS")]
    public void ProcessText_DoesNotProcessShortWords(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System.Net", "System.<wbr>Net")]
    [InlineData("System.IO", "System.IO")] // Total length is 9, less than 10
    [InlineData("A.B.C.D", "A.B.C.D")] // Total length is 7, less than 10
    [InlineData("Longer.Word.Here", "Longer.<wbr>Word.<wbr>Here")] // Total length is 16, more than 10
    public void ProcessText_BreaksAtDotsForLongWords(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System.Net.Http", "System.<wbr>Net.<wbr>Http")]
    [InlineData("Microsoft.Extensions.DependencyInjection", "Microsoft.<wbr>Extensions.<wbr>Dependency<wbr>Injection")] // DependencyInjection is > 10 chars
    [InlineData("System.Collections.Generic.Dictionary", "System.<wbr>Collections.<wbr>Generic.<wbr>Dictionary")]
    public void ProcessText_BreaksAtDotsInLongIdentifiers(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    // Words now always get uppercase breaks if they meet minimum length
    [InlineData("HttpClientHandler", "Http<wbr>Client<wbr>Handler")]
    [InlineData("ServiceCollection", "Service<wbr>Collection")]
    [InlineData("DependencyInjection", "Dependency<wbr>Injection")]
    // Words with dots also get processed
    [InlineData("System.HttpClientHandler", "System.<wbr>Http<wbr>Client<wbr>Handler")]
    [InlineData("MyNamespace.ServiceCollection", "My<wbr>Namespace.<wbr>Service<wbr>Collection")] // MyNamespace is 11 chars, gets uppercase breaks
    public void ProcessText_AlwaysBreaksUppercaseInLongWords(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System.XMLHttpRequest", "System.<wbr>XMLHttp<wbr>Request")] // XMLHttpRequest is 14 chars
    [InlineData("System.IOStream", "System.<wbr>IOStream")] // IOStream is 8 chars, < 10, no uppercase breaks
    [InlineData("System.HTTPSConnection", "System.<wbr>HTTPSConnection")] // HTTPSConnection is 15 chars but no lowercase->uppercase transition
    public void ProcessText_HandlesConsecutiveUppercaseLetters(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System.Net.HttpClient", 10, "System.<wbr>Net.<wbr>Http<wbr>Client")]
    [InlineData("System.Net.HttpClient", 20, "System.<wbr>Net.<wbr>HttpClient")]
    [InlineData("System.Net.HttpClient", 30, "System.Net.HttpClient")]
    public void ProcessText_RespectsMinimumCharactersSetting(string input, int minChars, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: minChars);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("MyLittleContentEngine.IntegrationTests.ExampleProjects.MultipleContentSourceExampleWebApplicationFactory", 
                "My<wbr>Little<wbr>Content<wbr>Engine.<wbr>Integration<wbr>Tests.<wbr>Example<wbr>Projects.<wbr>Multiple<wbr>Content<wbr>Source<wbr>Example<wbr>Web<wbr>Application<wbr>Factory")]
    [InlineData("System.IEquatable<MyLittleContentEngine.Services.Content.TableOfContents.ContentTocItem>", 
                "System.<wbr>IEquatable<My<wbr>Little<wbr>Content<wbr>Engine.<wbr>Services.<wbr>Content.<wbr>Table<wbr>Of<wbr>Contents.<wbr>Content<wbr>Toc<wbr>Item>")]
    public void ProcessText_HandlesComplexRealWorldExamples(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("The System.Net.Http.HttpClient class", "The System.<wbr>Net.<wbr>Http.<wbr>Http<wbr>Client class")]
    [InlineData("Use HttpClientHandler for configuration", "Use Http<wbr>Client<wbr>Handler for configuration")] // Now processes uppercase breaks
    [InlineData("Use System.HttpClientHandler for configuration", "Use System.<wbr>Http<wbr>Client<wbr>Handler for configuration")]
    [InlineData("Multiple words like HttpClient and ServiceCollection here", "Multiple words like Http<wbr>Client and Service<wbr>Collection here")] // Now processes uppercase breaks
    public void ProcessText_ProcessesMultipleWordsInSentences(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System.Net.Http", "­", "System.­Net.­Http")]
    [InlineData("System.HttpClientHandler", "&shy;", "System.&shy;Http&shy;Client&shy;Handler")]
    [InlineData("System.Net.HttpClient", " ", "System. Net. Http Client")]
    public void ProcessText_UsesCustomWordBreakCharacters(string input, string breakChar, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10, wordBreakCharacters: breakChar);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("file.txt", "file.<wbr>txt")]
    [InlineData("index.html", "index.<wbr>html")]
    [InlineData("script.min.js", "script.<wbr>min.<wbr>js")]
    public void ProcessText_HandlesFileExtensions(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 5);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("camelCase", "camelCase")] // 9 chars, < 10, no processing
    [InlineData("PascalCase", "Pascal<wbr>Case")] // 10 chars, gets uppercase breaks
    [InlineData("lowercase", "lowercase")]
    [InlineData("UPPERCASE", "UPPERCASE")]
    [InlineData("namespace.PascalCase", "namespace.<wbr>Pascal<wbr>Case")] // With dot, both segments get uppercase breaks
    public void ProcessText_HandlesVariousCasingStyles(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("System..Net", "System.<wbr>.<wbr>Net")] // System..Net is 11 chars total
    [InlineData("Trailing.", "Trailing.")] // 9 chars, < 10, no processing
    [InlineData(".Leading", ".Leading")] // 8 chars, < 10, no processing
    [InlineData("...Multiple...", ".<wbr>.<wbr>.<wbr>Multiple.<wbr>.<wbr>.")] // 14 chars, gets dot breaks (trailing dots don't get <wbr> after)
    [InlineData("VeryLongWord.txt", "Very<wbr>Long<wbr>Word.<wbr>txt")] // VeryLongWord is 12 chars
    [InlineData("Short.txt", "Short.txt")] // 9 chars, < 10, no processing
    [InlineData("LongerWord", "Longer<wbr>Word")] // 10 chars, gets uppercase breaks
    public void ProcessText_HandlesEdgeCasesWithDots(string input, string expected)
    {
        var processor = CreateProcessor(minimumCharacters: 10);
        var result = processor.ProcessText(input);
        Assert.Equal(expected, result);
    }
}
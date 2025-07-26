using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WordbreakMiddleware.Tests;

public class WordBreakMiddlewareTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public WordBreakMiddlewareTests()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services => { })
            .Configure(app =>
            {
                app.UseWordBreak(options => options.MinimumCharacters = 10);
                app.Run(async context =>
                {
                    var response = context.Request.Query["response"].FirstOrDefault() ?? "";
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(response);
                });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }

    [Fact]
    public async Task Should_Not_Process_Body_Text()
    {
        var html = "<html><body>Short HttpClient text System.Net.Http.HttpClient</body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // Text in body should not be processed
        Assert.Contains("System.Net.Http.HttpClient", content);
        Assert.DoesNotContain("<wbr>", content);
    }

    [Fact]
    public async Task Should_Process_Text_In_Headings()
    {
        var html = "<html><body><h1>The System.Net.Http.HttpClient class</h1></body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("System.<wbr>Net.<wbr>Http.<wbr>HttpClient", content);
    }

    [Fact]
    public async Task Should_Process_Namespaced_Identifiers_In_Headings()
    {
        var html = "<html><body><h2>Use System.Net.Http.HttpClientHandler for configuration</h2></body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("System.<wbr>Net.<wbr>Http.<wbr>HttpClientHandler", content);
    }

    [Fact]
    public async Task Should_Skip_Script_Tags()
    {
        var html = @"<html><body>
            <script>var System.Net.Http = 'test';</script>
            <h3>System.Net.Http.HttpClient</h3>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // Script content should not be processed
        Assert.Contains("var System.Net.Http = 'test'", content);
        // Heading content should be processed
        Assert.Contains("<h3>System.<wbr>Net.<wbr>Http.<wbr>HttpClient</h3>", content);
    }

    [Fact]
    public async Task Should_Skip_Style_Tags()
    {
        var html = @"<html><body>
            <style>.System.Net.Http { color: red; }</style>
            <h4>System.Net.Http text</h4>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains(".System.Net.Http { color: red; }", content);
        Assert.Contains("<h4>System.<wbr>Net.<wbr>Http text</h4>", content);
    }

    [Fact]
    public async Task Should_Skip_Code_Tags()
    {
        var html = @"<html><body>
            <code>System.Net.Http</code>
            <h5>System.Net.Http outside code</h5>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<code>System.Net.Http</code>", content);
        Assert.Contains("<h5>System.<wbr>Net.<wbr>Http outside code</h5>", content);
    }

    [Fact]
    public async Task Should_Skip_Pre_Tags()
    {
        var html = @"<html><body>
            <pre>System.Net.Http example</pre>
            <h6>System.Net.Http outside pre</h6>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<pre>System.Net.Http example</pre>", content);
        Assert.Contains("<h6>System.<wbr>Net.<wbr>Http outside pre</h6>", content);
    }

    [Fact]
    public async Task Should_Not_Process_CamelCase_Without_Dots()
    {
        var html = "<html><body><h1>The httpClientMessageHandler variable</h1></body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // No dots, so no word breaks
        Assert.Contains("httpClientMessageHandler", content);
        Assert.DoesNotContain("<wbr>", content);
    }

    [Fact]
    public async Task Should_Not_Process_Mixed_Case_Without_Dots()
    {
        var html = "<html><body><h2>Use HttpClient2MessageHandler3 class</h2></body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // No dots, so no word breaks
        Assert.Contains("HttpClient2MessageHandler3", content);
        Assert.DoesNotContain("<wbr>", content);
    }

    [Fact]
    public async Task Should_Process_Multiple_Headings()
    {
        var html = @"<html><body>
            <h1>System.Net.Http</h1>
            <h2>Microsoft.Extensions.DependencyInjection</h2>
            <h3>System.IO.FileSystem</h3>
            <p>System.Net.Http in paragraph</p>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<h1>System.<wbr>Net.<wbr>Http</h1>", content);
        Assert.Contains("<h2>Microsoft.<wbr>Extensions.<wbr>DependencyInjection</h2>", content);
        Assert.Contains("<h3>System.<wbr>IO.<wbr>FileSystem</h3>", content);
        Assert.Contains("<p>System.Net.Http in paragraph</p>", content); // Not processed in paragraph
    }

    [Fact]
    public async Task Should_Preserve_Original_Html_Structure()
    {
        var html = @"<html><body>
            <div class=""container"">
                <h2 id=""test"">System.Net.Http text</h2>
            </div>
        </body></html>";
        var response = await _client.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains(@"class=""container""", content);
        Assert.Contains(@"id=""test""", content);
        Assert.Contains("System.<wbr>Net.<wbr>Http", content);
    }

    [Fact]
    public async Task Should_Not_Process_Non_Html_Content()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services => { })
            .Configure(app =>
            {
                app.UseWordBreak();
                app.Run(async context =>
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("HttpClientMessageHandler");
                });
            });

        using var server = new TestServer(builder);
        using var client = server.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("HttpClientMessageHandler", content);
    }
}

public class WordBreakMiddlewareConfigurationTests : IDisposable
{
    private TestServer? _server;
    private HttpClient? _client;

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }

    private void CreateServer(Action<WordbreakMiddlewareOptions> configure)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services => { })
            .Configure(app =>
            {
                var options = new WordbreakMiddlewareOptions()
                {
                    MinimumCharacters = 10
                };
                configure(options);
                
                app.UseWordBreak(options);
                app.Run(async context =>
                {
                    var response = context.Request.Query["response"].FirstOrDefault() ?? "";
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(response);
                });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task Should_Respect_MinimumCharacters_Setting()
    {
        CreateServer(options => options.MinimumCharacters = 30);

        var html = "<html><body><h1>System.Net.Http and Microsoft.Extensions.DependencyInjection.ServiceCollection</h1></body></html>";
        var response = await _client!.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // System.Net.Http is 15 chars, should not be processed
        Assert.Contains("System.Net.Http and", content);
        Assert.DoesNotContain("System.<wbr>Net.<wbr>Http", content);
        
        // Microsoft.Extensions.DependencyInjection.ServiceCollection is > 30 chars, should be processed
        Assert.Contains("Microsoft.<wbr>Extensions.<wbr>DependencyInjection.<wbr>ServiceCollection", content);
    }

    [Fact]
    public async Task Should_Use_Custom_WordBreakCharacters()
    {
        CreateServer(options => options.WordBreakCharacters = "­");

        var html = "<html><body><h1>System.Net.Http</h1></body></html>";
        var response = await _client!.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("System.­Net.­Http", content);
        Assert.DoesNotContain("<wbr>", content);
    }

    [Fact]
    public async Task Should_Process_Non_Html_When_ProcessHtmlOnly_Is_False()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services => { })
            .Configure(app =>
            {
                app.UseWordBreak(options => 
                {
                    options.ProcessHtmlOnly = false;
                    options.MinimumCharacters = 10;
                });
                app.Run(async context =>
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("System.Net.Http");
                });
            });

        using var server = new TestServer(builder);
        using var client = server.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        
        // For non-HTML content with ProcessHtmlOnly=false, word breaks should be inserted
        Assert.Contains("System.<wbr>Net.<wbr>Http", content);
    }

    [Fact]
    public async Task Should_Handle_Empty_Response()
    {
        CreateServer(options => { });
        
        var html = "";
        var response = await _client!.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("<html><head></head><body></body></html>", content);
    }

    [Fact]
    public async Task Should_Handle_Malformed_Html()
    {
        CreateServer(options => { });

        var html = "<h1>System.Net.Http <h2>Another System.Net.Http";
        var response = await _client!.GetAsync($"/?response={Uri.EscapeDataString(html)}");
        var content = await response.Content.ReadAsStringAsync();
        
        // AngleSharp should fix the HTML structure and process headings
        Assert.Contains("System.<wbr>Net.<wbr>Http", content);
        Assert.Contains("</h1>", content);
        Assert.Contains("</h2>", content);
    }
}
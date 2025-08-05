using AngleSharp;
using Microsoft.AspNetCore.Http;

namespace WordbreakMiddleware;

/// <summary>
/// ASP.NET Core middleware that inserts word break opportunities in long words containing dots.
/// </summary>
public class WordBreakMiddleware(RequestDelegate next, WordbreakMiddlewareOptions options)
{
    private readonly IConfiguration _angleSharpConfig = Configuration.Default;
    private readonly WordBreakProcessor _processor = new(options);

    /// <summary>
    /// Processes the HTTP request and response, inserting word break characters in text content.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        if (ShouldProcessResponse(context))
        {
            context.Response.Body = originalBodyStream;
            responseBody.Seek(0, SeekOrigin.Begin);
            
            var text = await new StreamReader(responseBody).ReadToEndAsync();
            var processedText = await ProcessHtmlAsync(text);
            
            await context.Response.WriteAsync(processedText);
        }
        else
        {
            context.Response.Body = originalBodyStream;
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldProcessResponse(HttpContext context)
    {
        if (context.Response.HasStarted || context.Response.StatusCode != 200)
        {
            return false;
        }
        
        var contentType = context.Response.ContentType?.ToLower() ?? "";
        if (contentType.Contains("text/html"))
        {
            return true;
        }
        
        return !options.ProcessHtmlOnly && contentType.Contains("text/");
    }

    private async Task<string> ProcessHtmlAsync(string html)
    {
        if (!options.ProcessHtmlOnly)
        {
            return _processor.ProcessText(html);
        }
        
        var context = BrowsingContext.New(_angleSharpConfig);
        var document = await context.OpenAsync(req => req.Content(html));
        
        // Use CSS selector to find all elements to process
        var selector = options.CssSelector;
        var elements = document.QuerySelectorAll(selector);
        
        // Process each element immediately but safely
        foreach (var element in elements.ToArray()) // ToArray creates a fixed snapshot
        {
            // Check if element only contains text (no HTML tags)
            if (element.InnerHtml.Trim() != element.TextContent.Trim())
                continue;
                
            var processedText = _processor.ProcessText(element.TextContent);
            if (processedText != element.TextContent)
            {
                // Apply changes immediately using innerHTML which is safer
                element.InnerHtml = processedText;
            }
        }
        
        return document.ToHtml();
    }
}
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Http;

namespace WordbreakMiddleware;

/// <summary>
/// ASP.NET Core middleware that inserts word break opportunities in long words containing dots.
/// </summary>
public class WordBreakMiddleware(RequestDelegate next, WordbreakMiddlewareOptions options)
{
    private readonly IConfiguration _angleSharpConfig = Configuration.Default;

    /// <summary>
    /// Processes the HTTP request and response, inserting word break characters in text content.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
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
            return ProcessText(html);
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
            if (element.InnerHtml?.Trim() != element.TextContent?.Trim())
                continue;
                
            var processedText = ProcessText(element.TextContent ?? "");
            if (processedText != element.TextContent)
            {
                // Apply changes immediately using innerHTML which is safer
                element.InnerHtml = processedText;
            }
        }
        
        return document.DocumentElement.OuterHtml;
    }

    

    private string ProcessText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // if we don't even have the minimum characters, no need to go forward
        if (text.Length < options.MinimumCharacters)
            return text;
        
        var span = text.AsSpan();
        var result = new StringBuilder(text.Length + 100); // Pre-allocate with some extra space
        var wordStart = 0;
        
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != ' ') continue;
            // Process the word from wordStart to i
            if (i > wordStart)
            {
                var wordSpan = span.Slice(wordStart, i - wordStart);
                ProcessWord(wordSpan, result);
            }
            result.Append(' ');
            wordStart = i + 1;
        }
        
        // Process the last word if any
        if (wordStart >= span.Length) return result.ToString();
        {
            var wordSpan = span.Slice(wordStart);
            ProcessWord(wordSpan, result);
        }

        return result.ToString();
    }
    
    private void ProcessWord(ReadOnlySpan<char> word, StringBuilder result)
    {
        if (word.Length < options.MinimumCharacters)
        {
            result.Append(word);
            return;
        }
        
        // Only process words with dots
        var dotIndex = word.IndexOf('.');
        if (dotIndex == -1)
        {
            result.Append(word);
            return;
        }
        
        var start = 0;
        
        while (dotIndex != -1)
        {
            // Append text up to and including the dot
            result.Append(word.Slice(start, dotIndex - start + 1));
            
            // Add word break after the dot if there's more content
            if (dotIndex + 1 < word.Length)
            {
                result.Append(options.WordBreakCharacters);
            }
            
            start = dotIndex + 1;
            if (start < word.Length)
            {
                dotIndex = word[start..].IndexOf('.');
                if (dotIndex != -1)
                {
                    dotIndex += start;
                }
            }
            else
            {
                break;
            }
        }
        
        // Append remaining part after last dot
        if (start < word.Length)
        {
            result.Append(word[start..]);
        }
    }
}
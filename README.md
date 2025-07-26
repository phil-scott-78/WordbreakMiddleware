# WordbreakMiddleware

ASP.NET Core middleware that automatically adds word-break opportunities to long words containing dots in HTML content. This helps prevent layout issues with long domain names, file paths, and similar text that would otherwise overflow their containers.

## Installation

```bash
dotnet add package WordbreakMiddleware
```

## Quick Start

Add the middleware to your ASP.NET Core pipeline:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add WordbreakMiddleware to the pipeline
app.UseWordBreakMiddleware();

app.Run();
```

## How It Works

The middleware intercepts HTML responses and inserts `<wbr>` (word break opportunity) tags after dots in long words. For example:

- `verylongdomainname.example.com` becomes `verylongdomainname.<wbr>example.<wbr>com`
- `some.really.long.file.path.txt` becomes `some.<wbr>really.<wbr>long.<wbr>file.<wbr>path.<wbr>txt`

By default, only words with 20+ characters in heading elements (h1-h6) are processed.

## Configuration

Customize the middleware behavior:

```csharp
app.UseWordBreakMiddleware(options =>
{
    // Minimum word length to process (default: 20)
    options.MinimumCharacters = 15;
    
    // Characters to insert at break points (default: "<wbr>")
    options.WordBreakCharacters = "<wbr>";
    
    // Process only HTML content (default: true)
    options.ProcessHtmlOnly = true;
    
    // CSS selector for elements to process (default: "h1, h2, h3, h4, h5, h6")
    options.CssSelector = "h1, h2, h3, h4, h5, h6, .text-break";
});
```

## License

MIT
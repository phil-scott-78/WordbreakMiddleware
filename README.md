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

The middleware intercepts HTML responses and inserts `<wbr>` (word break opportunity) tags in two ways:

1. **After dots in long identifiers**: Breaks at natural namespace/type boundaries
2. **Before uppercase letters in long words**: Breaks PascalCase identifiers for better wrapping

Examples:

- `System.Collections.Generic.Dictionary` becomes `System.<wbr>Collections.<wbr>Generic.<wbr>Dictionary`
- `ICustomerOrderRepository` becomes `ICustomer<wbr>Order<wbr>Repository`
- `GetCustomerByIdAsync` becomes `Get<wbr>Customer<wbr>By<wbr>Id<wbr>Async`
- `Microsoft.AspNetCore.Mvc.ControllerBase` becomes `Microsoft.<wbr>AspNetCore.<wbr>Mvc.<wbr>Controller<wbr>Base`

By default, the middleware targets:
- Words with 20+ characters
- In heading elements (`h1`, `h2`, `h3`, `h4`, `h5`, `h6`) and elements with class `text-break`

## .NET Identifier Examples

Here are common scenarios where long .NET identifiers benefit from word breaking:

### Interface Names
```html
<h2>ICustomerOrderManagementService</h2>
<!-- Renders as: ICustomer<wbr>Order<wbr>Management<wbr>Service -->
```

### Long Method Names
```html
<h3>ValidateCustomerCreditScoreAsync</h3>
<!-- Renders as: Validate<wbr>Customer<wbr>Credit<wbr>Score<wbr>Async -->
```

### Entity Framework DbContext
```html
<h2>ApplicationDbContextModelSnapshot</h2>
<!-- Renders as: Application<wbr>Db<wbr>Context<wbr>Model<wbr>Snapshot -->
```

### Combined Namespace and Type Breaking
```html
<h3>System.ComponentModel.DataAnnotations.ValidationAttribute</h3>
<!-- Renders as: System.<wbr>Component<wbr>Model.<wbr>Data<wbr>Annotations.<wbr>Validation<wbr>Attribute -->
```

### Generic Type Constraints
```html
<h4>IRepository<TEntity> where TEntity : BaseEntityWithTimestamps</h4>
<!-- BaseEntityWithTimestamps renders as: Base<wbr>Entity<wbr>With<wbr>Timestamps -->
```

### ASP.NET Core Middleware Names
```html
<h2>UseAuthenticationMiddleware</h2>
<!-- Renders as: Use<wbr>Authentication<wbr>Middleware -->
```

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
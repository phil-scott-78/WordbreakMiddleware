using WordbreakMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Apply WordBreak middleware based on query parameter configuration
app.Use(async (context, next) =>
{
    var query = context.Request.Query;
    
    // Check if wordbreak should be applied
    if (query.ContainsKey("wordbreak") && query["wordbreak"] == "on")
    {
        var options = new WordbreakMiddlewareOptions();
        
        // Parse configuration from query string
        if (int.TryParse(query["minchars"], out var minChars))
            options.MinimumCharacters = minChars;
        
        // Apply middleware for this request
        var middleware = new WordBreakMiddleware(next, options);
        await middleware.InvokeAsync(context);
    }
    else
    {
        await next(context);
    }
});

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

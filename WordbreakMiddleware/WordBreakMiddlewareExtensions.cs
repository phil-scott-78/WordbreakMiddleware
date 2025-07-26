using Microsoft.AspNetCore.Builder;

namespace WordbreakMiddleware;

/// <summary>
/// Extension methods for adding WordBreakMiddleware to the ASP.NET Core pipeline.
/// </summary>
public static class WordBreakMiddlewareExtensions
{
    /// <summary>
    /// Adds the WordBreakMiddleware to the application's request pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="options">
    /// Optional configuration options for the middleware.
    /// If null, default options are used.
    /// </param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseWordBreak(
        this IApplicationBuilder builder,
        WordbreakMiddlewareOptions? options = null)
    {
        options ??= new WordbreakMiddlewareOptions();
        return builder.UseMiddleware<WordBreakMiddleware>(options);
    }

    /// <summary>
    /// Adds the WordBreakMiddleware to the application's request pipeline with configuration.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configureOptions">An action to configure the middleware options.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseWordBreak(
        this IApplicationBuilder builder,
        Action<WordbreakMiddlewareOptions> configureOptions)
    {
        var options = new WordbreakMiddlewareOptions();
        configureOptions(options);
        return builder.UseMiddleware<WordBreakMiddleware>(options);
    }
}
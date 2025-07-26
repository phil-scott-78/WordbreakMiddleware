namespace WordbreakMiddleware;

/// <summary>
/// Configuration options for the WordBreakMiddleware.
/// </summary>
public class WordbreakMiddlewareOptions
{
    /// <summary>
    /// Gets or sets the minimum number of characters a word must have before word breaks are inserted.
    /// Default is 20.
    /// </summary>
    public int MinimumCharacters { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the characters to insert at word break points.
    /// Default is "&lt;wbr&gt;" (HTML word break opportunity element).
    /// </summary>
    public string WordBreakCharacters { get; set; } = "<wbr>";
    
    /// <summary>
    /// Gets or sets whether to process only HTML content.
    /// When true, only HTML content is processed and text nodes within headings are modified.
    /// When false, all text content is processed as plain text.
    /// Default is true.
    /// </summary>
    public bool ProcessHtmlOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets the CSS selector to identify elements for word break processing.
    /// Default is "h1, h2, h3, h4, h5, h6".
    /// Set to null to use the default selector.
    /// </summary>
    public string CssSelector { get; set; } = "h1, h2, h3, h4, h5, h6, .text-break";
}
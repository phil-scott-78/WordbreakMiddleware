using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WordbreakMiddleware.Demo.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsEnabled { get; set; }
        public int MinimumCharacters { get; set; } = 20;
        
        public void OnGet()
        {
            // Read configuration from query string
            IsEnabled = Request.Query.ContainsKey("wordbreak") && Request.Query["wordbreak"] == "on";
            
            if (int.TryParse(Request.Query["minchars"], out var minChars))
            {
                MinimumCharacters = minChars;
            }
        }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BankingApp; 
using SQLitePCL;
using System.Security.Claims;
namespace TransactionRecords.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    [BindProperty]
    public string? Username { get; set; } = string.Empty;

    [BindProperty]
    public string? Password { get; set; } = string.Empty;

    [TempData]
    public string? LoginMessage { get; set; }



    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        LoginMessage = TempData["LoginMessage"] as string;
    } 

    public void OnPost()
    {
        // TODO
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        Verify V = new Verify();
        Dictionary<string, object> responseObj = V.VerifyAWS(Username, Password);

        // Safely check authentication status first
        if (responseObj.TryGetValue("authenticated", out var authVal) && authVal is bool isAuthenticated && isAuthenticated)
        {
            // Extract CustomerId safely (defaults to "0" if missing or null)
            string customerIdStr = responseObj.TryGetValue("CustomerId", out var idVal) ? idVal?.ToString() ?? "0" : "0";

            // Authentication Successful, create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username), // Added missing comma here
                new Claim("CustomerId", customerIdStr)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity)
            );

            return Redirect("/Home");
        }

        // Handle failed login safely
        LoginMessage = responseObj.TryGetValue("status", out var statusVal) 
            ? statusVal?.ToString() ?? "We're sorry, something went wrong on our end please try again later"
            : "We're sorry, something went wrong on our end please try again later";

        return Page();
    }
}
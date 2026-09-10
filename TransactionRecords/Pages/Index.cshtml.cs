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
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? LoginMessage { get; set; } = string.Empty;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
    public void OnPost()
    {
        // TODO
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
       Verify V = new Verify();
       var responseObj = V.VerifyLocally(Username, Password);

       if((bool)responseObj["authenticated"])
       {
            // Authentication Successful, create user id
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username),
            };

            var ClaimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(ClaimsIdentity));

            return Redirect("/Home");
       }
       LoginMessage = responseObj["status"]?.ToString() ?? "We're sorry, something went wrong on our end please try again later";

       return Page(); 
    }
}
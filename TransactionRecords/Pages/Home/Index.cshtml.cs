using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; 
using SQLitePCL;
using System.Security.Claims;

using BankingApp;
namespace TransactionRecords.Pages.Home;

public class HomeModel : PageModel
{
    private readonly ILogger<HomeModel> _logger;
    [BindProperty]
    public string Amount{ get; set; } = string.Empty;

    [BindProperty]
    public string TransType { get; set; } = string.Empty;

    [BindProperty]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public int CustomerId { get; set; }

    public HomeModel(ILogger<HomeModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // Verify User has authentication cookie set, else send them back to the LogIn Page
        if(User.Identity == null || !User.Identity.IsAuthenticated){
            // send them back to the login page if they lack cookie
            return Redirect("/");
        }

        // Populate Username
        UserName = User.Identity.Name ?? string.Empty;
        
        return Page();
    }
    public void OnPost()
    {
        // TODO
    }

    public void OnPostEnterTransactionRecord()
    {
        
    }
}
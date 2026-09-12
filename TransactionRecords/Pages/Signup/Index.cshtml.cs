using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BankingApp; 
using SQLitePCL;
using System.Security.Claims;
namespace TransactionRecords.Pages;

public class SignupModel : PageModel
{
    private readonly ILogger<SignupModel> _logger;
    [BindProperty]
    public string? Username { get; set; } = string.Empty;

    [BindProperty]
    public string? Password { get; set; } = string.Empty;

    public string? SignupMessage { get; set; } = string.Empty;

    public SignupModel(ILogger<SignupModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        TempData["LoginMessage"] = "Thanks for signing up";
        return RedirectToPage("/Index");
    }

    public void OnPost()
    {
         // TODO
    }
}
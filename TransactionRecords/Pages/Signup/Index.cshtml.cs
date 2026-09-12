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
    public string? FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string? LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; } = string.Empty;

    [BindProperty]
    public string? Password { get; set; } = string.Empty;

    [BindProperty]
    public string? UserName  { get; set; } = string.Empty;

    [BindProperty]
    public string? SignupMessage { get; set; } = string.Empty;

    [TempData]
    public string? LoginMessage { get; set; } = string.Empty;

    public SignupModel(ILogger<SignupModel> logger)
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

    private bool AddNewCustomer(string FirstName, string LastName, string Email, string Password, string UserName)
    {
        // Get last customer id
        SendTransactions STaws =  new SendTransactions();
        int customerid = STaws.GetCustomerIdAsync("last-customer").GetAwaiter().GetResult();

        customerid++; // increase customerid by 1

        // Send payload to the database
        var data = new {
            FirstName = FirstName,
            LastName = LastName,
            CustomerId = customerid,
            Email = Email,
            Password = Password,
            UserName = UserName
        };
        bool isSuccess= STaws.RecordTransaction(FirstName, LastName, customerid, data, "add-new-customer").GetAwaiter().GetResult();
        return isSuccess;
    }

    private string PasswordHash(string Password)
    {
        Verify V = new Verify();
        string HashedPassword = V.GetPassWordHash(Password); // Hash Password from user input form
        return HashedPassword;
    }

    public IActionResult OnPostUserRegistration()
    {
        
        string HashdPword = PasswordHash(Password);

        bool isSuccess = AddNewCustomer(FirstName, LastName, Email, HashdPword, UserName);
        if(isSuccess){
            LoginMessage = "Registration Successful";
            return Redirect("/Home");
        }

        else
            SignupMessage = "Registration Failed";
        return Page();
    }
}
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

    public CustomerResponse? CustomerData {get; set; }

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

        string customerIdClaim = User.FindFirst("CustomerId")?.Value ?? "0";
        if (int.TryParse(customerIdClaim, out int parsedId))
        {
            CustomerId = parsedId;
        }

        var data = new {CustomerId = 14};
        string _path = "get-customer-data";

        // New Object of GetCustomerData Class
        GetCustomerData GTD = new GetCustomerData();
        CustomerData = GTD.FetchData(data, _path).GetAwaiter().GetResult();

        return Page();
    }
    
    public void OnPost()
    {
        // TODO
    }

    public void OnPostEnterTransactionRecord()
    {
        // Create New Object of SendTransactions Class
        SendTransactions ST = new SendTransactions();
        string url_path;

        string customerIdClaim = User.FindFirst("CustomerId")?.Value ?? "0";
        if (int.TryParse(customerIdClaim, out int parsedId))
        {
            CustomerId = parsedId;
        }

        Console.WriteLine($"Yo Yo Customer id is: {CustomerId}");
        var data = new {
            type = TransType,
            amount = Amount,
            CustomerId = CustomerId
        };

        if(TransType == "Deposit")
        {
            url_path = "add-deposit";
        }
        else if(TransType == "Withdrawal")
        {
           url_path = "add-withdrawal";
        }
        else{
            throw new Exception ("No proper url path is set");
        }
        bool recorded = ST.RecordTransaction(data, url_path).GetAwaiter().GetResult();
        if(!recorded)
            throw new Exception ("Could not record transaction, check your code");
    }
}
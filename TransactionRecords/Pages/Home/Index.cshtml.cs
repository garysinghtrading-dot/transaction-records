using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
//using BankingApp; 
using SQLitePCL;
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

    public HomeModel(ILogger<HomeModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
    public void OnPost()
    {
        // TODO
        Console.WriteLine("New transaction record type hit");
    }
}
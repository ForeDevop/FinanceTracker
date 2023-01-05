using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

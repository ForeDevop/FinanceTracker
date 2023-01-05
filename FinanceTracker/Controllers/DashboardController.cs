using FinanceTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        const int DaysOfWeek = 7;
        var startDate = DateTime.Today.AddDays(-DaysOfWeek);
        var endDate = DateTime.Today;

        var selectedTransactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(c => c.Date > startDate && c.Date <= endDate)
            .ToListAsync();

        var totalIncome = selectedTransactions
            .Where(t => t.Category.Type == "Income")
            .Sum(i => i.Amount);
        ViewBag.TotalIncome = totalIncome.ToString("C0");

        var totalExpense = selectedTransactions
            .Where(t => t.Category.Type == "Expense")
            .Sum(e => e.Amount);
        ViewBag.TotalExpense = totalExpense.ToString("C0");

        var balance  = totalIncome - totalExpense;
        ViewBag.Balance = balance.ToString();

        return View();
    }
}
using FinanceTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        #region Income, Expense, Balance
        var totalIncome = selectedTransactions
            .Where(t => t.Category.Type == "Income")
            .Sum(i => i.Amount);
        ViewBag.TotalIncome = totalIncome.ToString("C0");

        var totalExpense = selectedTransactions
            .Where(t => t.Category.Type == "Expense")
            .Sum(e => e.Amount);
        ViewBag.TotalExpense = totalExpense.ToString("C0");

        var balance = totalIncome - totalExpense;
        var culture = CultureInfo.CreateSpecificCulture("ru");
        culture.NumberFormat.CurrencyNegativePattern = 1;
        ViewBag.Balance = string.Format(culture, "{0:C0}", balance);
        #endregion

        var donutChart = selectedTransactions
           .Where(i => i.Category.Type == "Expense")
           .GroupBy(j => j.Category.CategoryId)
           .Select(a => new
           {
               categoryTitleWithIcon = $"{a.First().Category.Icon} {a.First().Category.Title}",
               amount = a.Sum(k => k.Amount),
               text = a.Sum(k => k.Amount).ToString("C0"),
           })
           .ToList();

        ViewBag.DonutChart = donutChart;

        return View();
    }
}
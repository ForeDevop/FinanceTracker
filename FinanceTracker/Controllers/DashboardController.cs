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

        #region Income / Expense Labels
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

        #region Income / Expense Charts
        var donutChart = selectedTransactions
           .Where(i => i.Category.Type == "Expense")
           .GroupBy(j => j.Category.CategoryId)
           .Select(a => new
           {
               categoryTitleWithIcon = $"{a.First().Category.Icon} {a.First().Category.Title}",
               amount = a.Sum(k => k.Amount),
               labelText = a.Sum(k => k.Amount).ToString("C0"),
           })
           .OrderByDescending(l => l.amount)
           .ToList();

        ViewBag.DonutChart = donutChart;

        var incomeSummary = selectedTransactions
            .Where(i => i.Category.Type == "Income")
            .GroupBy(j => j.Date)
            .Select(k => new SplineChart
            {
                Day = k.First().Date.ToString("dd-MMM"),
                Income = k.Sum(l => l.Amount),
            })
            .ToList();

        var expenseSummary = selectedTransactions
            .Where(i => i.Category.Type == "Expense")
            .GroupBy(j => j.Date)
            .Select(k => new SplineChart
            {
                Day = k.First().Date.ToString("dd-MMM"),
                Expense = k.Sum(l => l.Amount),
            })
            .ToList();

        var pastWeek = Enumerable.Range(0, DaysOfWeek)
            .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
            .ToArray();

        var splineChart = from day in pastWeek
                          join income in incomeSummary on day equals income.Day into dayIncomeJoined
                          from income in dayIncomeJoined.DefaultIfEmpty()
                          join expense in incomeSummary on day equals expense.Day into dayExpenseJoined
                          from expense in dayExpenseJoined.DefaultIfEmpty()
                          select new SplineChart
                          {
                              Day = day,
                              Income = income == null ? 0 : income.Income,
                              Expense = expense == null ? 0 : expense.Expense,
                          };
        ViewBag.SplineChart = splineChart; 
        #endregion

        return View();
    }
}

public class SplineChart
{
    public string? Day { get; set; }
    public int Income { get; set; }
    public int Expense { get; set; }
}
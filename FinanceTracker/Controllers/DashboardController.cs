using FinanceTracker.Data;
using FinanceTracker.Models;
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
        var selectedTransactions = await GetSelectedTransactions();

        var totalIncome = GetTotal("Income", selectedTransactions);
        var totalExpense = GetTotal("Expense", selectedTransactions);

        ViewBag.TotalIncome = totalIncome.ToString("C0");
        ViewBag.TotalExpense = totalExpense.ToString("C0");
        ViewBag.Balance = GetBalance(totalIncome, totalExpense);

        ViewBag.SplineChart = SplineChart.GetSplineChart(selectedTransactions);
        ViewBag.DonutChart = DonutChart.GetDonutChart(selectedTransactions);

        return View();
    }

    public async Task<List<Transaction>> GetSelectedTransactions()
    {
        int DaysOfWeek = 7;
        DateTime startDate = DateTime.Today.AddDays(-DaysOfWeek);
        DateTime endDate = DateTime.Today;

        return await _context.Transactions
            .Include(t => t.Category)
            .Where(c => c.Date > startDate && c.Date <= endDate)
            .ToListAsync();
    }

    public int GetTotal(string categoryType, List<Transaction> selectedTransactions)
    {
        return selectedTransactions
            .Where(t => t.Category.Type == categoryType)
            .Sum(i => i.Amount);
    }

    public string GetBalance(int income, int expense)
    {
        var balance = income - expense;
        var culture = CultureInfo.CreateSpecificCulture("ru");

        culture.NumberFormat.CurrencyNegativePattern = 1;
        return string.Format(culture, "{0:C0}", balance);
    }
}
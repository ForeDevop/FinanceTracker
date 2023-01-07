using FinanceTracker.Data;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
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
        const int DaysOfWeek = 7;
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

public class DonutChart
{
    public string? CategoryTitleWithIcon { get; set; }
    public int Amount { get; set; }
    public string? LabelText { get; set; }

    public static List<DonutChart> GetDonutChart(List<Transaction> selectedTransactions)
    {
        return selectedTransactions
           .Where(i => i.Category.Type == "Expense")
           .GroupBy(j => j.Category.CategoryId)
           .Select(a => new DonutChart
           {
               CategoryTitleWithIcon = $"{a.First().Category.Icon} {a.First().Category.Title}",
               Amount = a.Sum(k => k.Amount),
               LabelText = a.Sum(k => k.Amount).ToString("C0"),
           })
           .OrderByDescending(l => l.Amount)
           .ToList();
    }
}

public class SplineChart
{
    public string? Day { get; set; }
    public int Income { get; set; }
    public int Expense { get; set; }

    public static List<SplineChart> GetSplineChartPoints(string categoryType, List<Transaction> selectedTransactions)
    {
        var transactionsByDate = selectedTransactions
            .Where(i => i.Category.Type == categoryType)
            .GroupBy(j => j.Date);

        if (categoryType == "Income")
        {
            return GetIncomeSplineChartPoints(transactionsByDate);
        }
        else
        {
            return GetExpenseSplineChartPoints(transactionsByDate);
        }
    }

    public static List<SplineChart> GetIncomeSplineChartPoints(IEnumerable<IGrouping<DateTime, Transaction>> transactionsByDate)
    {
        return transactionsByDate.Select(k => new SplineChart
        {
            Day = k.First().Date.ToString("dd-MMM"),
            Income = k.Sum(l => l.Amount),
        })
        .ToList();
    }

    public static List<SplineChart> GetExpenseSplineChartPoints(IEnumerable<IGrouping<DateTime, Transaction>> transactionsByDate)
    {
        return transactionsByDate.Select(k => new SplineChart
        {
            Day = k.First().Date.ToString("dd-MMM"),
            Expense = k.Sum(l => l.Amount),
        })
        .ToList();
    }

    public static string[] GetLastWeekCaptions()
    {
        int DaysOfWeek = 7;
        DateTime startDate = DateTime.Today.AddDays(-DaysOfWeek);

        return Enumerable.Range(0, DaysOfWeek)
            .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
            .ToArray();
    }

    public static IEnumerable<SplineChart> GetSplineChart(List<Transaction> selectedTransactions)
    {
        var incomePoints = GetSplineChartPoints("Income", selectedTransactions);
        var expensePoints = GetSplineChartPoints("Expense", selectedTransactions);

        return from day in GetLastWeekCaptions()
               join income in incomePoints on day equals income.Day into dayIncomeJoined
               from income in dayIncomeJoined.DefaultIfEmpty()
               join expense in expensePoints on day equals expense.Day into dayExpenseJoined
               from expense in dayExpenseJoined.DefaultIfEmpty()
               select new SplineChart
               {
                   Day = day,
                   Income = income == null ? 0 : income.Income,
                   Expense = expense == null ? 0 : expense.Expense,
               };
    }
}
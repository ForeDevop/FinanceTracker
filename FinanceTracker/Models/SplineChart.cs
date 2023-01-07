namespace FinanceTracker.Models;

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
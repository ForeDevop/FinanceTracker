namespace FinanceTracker.Models;

public class SplineChart
{
    public string? Day { get; set; }
    public int Income { get; set; }
    public int Expense { get; set; }

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
                   Income = income is null ? 0 : income.Income,
                   Expense = expense is null ? 0 : expense.Expense,
               };
    }

    private static List<SplineChart> GetSplineChartPoints(
        string categoryType, List<Transaction> selectedTransactions)
    {
        var transactionsByDate = selectedTransactions
            .Where(i => i.Category.Type == categoryType)
            .GroupBy(j => j.Date);

        return categoryType == "Income"
            ? GetIncomeSplineChartPoints(transactionsByDate)
            : GetExpenseSplineChartPoints(transactionsByDate);
    }

    private static List<SplineChart> GetIncomeSplineChartPoints(
        IEnumerable<IGrouping<DateTime, Transaction>> transactionsByDate)
    {
        return transactionsByDate.Select(k => new SplineChart
        {
            Day = k.First().Date.ToString("dd-MMM"),
            Income = k.Sum(l => l.Amount),
        })
        .ToList();
    }

    private static List<SplineChart> GetExpenseSplineChartPoints(
        IEnumerable<IGrouping<DateTime, Transaction>> transactionsByDate)
    {
        return transactionsByDate.Select(k => new SplineChart
        {
            Day = k.First().Date.ToString("dd-MMM"),
            Expense = k.Sum(l => l.Amount),
        })
        .ToList();
    }

    private static string[] GetLastWeekCaptions()
    {
        int DaysOfMonth = 30;
        DateTime startDate = DateTime.Today.AddDays(-DaysOfMonth);

        return Enumerable.Range(0, DaysOfMonth)
            .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
            .ToArray();
    }


}
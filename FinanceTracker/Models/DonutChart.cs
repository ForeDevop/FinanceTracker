namespace FinanceTracker.Models;

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
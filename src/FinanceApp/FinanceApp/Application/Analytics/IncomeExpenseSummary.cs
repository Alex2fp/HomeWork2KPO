namespace FinanceApp.Application.Analytics;

public record IncomeExpenseSummary(decimal Income, decimal Expense)
{
    public decimal Difference => Income - Expense;
}

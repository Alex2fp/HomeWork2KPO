namespace FinanceApp.Application.Analytics;

public record CategoryTotal(int CategoryId, string CategoryName, decimal Income, decimal Expense)
{
    public decimal Net => Income - Expense;
}

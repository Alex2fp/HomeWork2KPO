namespace FinanceApp.Application.Analytics;

public record CategoryTotal(Guid CategoryId, string CategoryName, decimal Income, decimal Expense)
{
    public decimal Net => Income - Expense;
}

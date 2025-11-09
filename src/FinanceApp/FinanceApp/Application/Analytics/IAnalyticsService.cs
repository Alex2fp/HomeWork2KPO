using FinanceApp.Domain;

namespace FinanceApp.Application.Analytics;

public interface IAnalyticsService
{
    AccountBalanceSummary GetAccountBalance(int accountId);
    IncomeExpenseSummary GetIncomeExpense(int accountId, DateOnly? from = null, DateOnly? to = null);
    IReadOnlyCollection<CategoryTotal> GetTotalsByCategory(int accountId, DateOnly? from = null, DateOnly? to = null);
}

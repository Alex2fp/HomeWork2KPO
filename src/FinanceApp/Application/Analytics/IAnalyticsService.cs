using FinanceApp.Domain;

namespace FinanceApp.Application.Analytics;

public interface IAnalyticsService
{
    AccountBalanceSummary GetAccountBalance(Guid accountId);
    IncomeExpenseSummary GetIncomeExpense(Guid accountId, DateOnly? from = null, DateOnly? to = null);
    IReadOnlyCollection<CategoryTotal> GetTotalsByCategory(Guid accountId, DateOnly? from = null, DateOnly? to = null);
}

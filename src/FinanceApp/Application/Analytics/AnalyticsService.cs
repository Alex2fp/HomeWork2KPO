using FinanceApp.Application.Repositories;
using FinanceApp.Domain;

namespace FinanceApp.Application.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IFinanceRepository _repository;

    public AnalyticsService(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public AccountBalanceSummary GetAccountBalance(Guid accountId)
    {
        var account = _repository.GetAccount(accountId);
        return new AccountBalanceSummary(account.Id, account.Name, account.Balance);
    }

    public IncomeExpenseSummary GetIncomeExpense(Guid accountId, DateOnly? from = null, DateOnly? to = null)
    {
        var operations = FilterOperations(accountId, from, to);
        var income = operations.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
        var expense = operations.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
        return new IncomeExpenseSummary(income, expense);
    }

    public IReadOnlyCollection<CategoryTotal> GetTotalsByCategory(Guid accountId, DateOnly? from = null, DateOnly? to = null)
    {
        var operations = FilterOperations(accountId, from, to);
        var categories = _repository.GetCategories().ToDictionary(c => c.Id);
        return operations
            .GroupBy(o => o.CategoryId)
            .Select(g =>
            {
                var category = categories[g.Key];
                var income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
                var expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
                return new CategoryTotal(category.Id, category.Name, income, expense);
            })
            .OrderByDescending(t => t.Net)
            .ToArray();
    }

    private IEnumerable<Operation> FilterOperations(Guid accountId, DateOnly? from, DateOnly? to)
    {
        return _repository.GetOperationsForAccount(accountId)
            .Where(o => (!from.HasValue || o.Date >= from) && (!to.HasValue || o.Date <= to));
    }
}

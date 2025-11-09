using System;
using System.Diagnostics;

namespace FinanceApp.Application.Analytics;

public class AnalyticsTimingDecorator : IAnalyticsService
{
    private readonly IAnalyticsService _inner;
    private readonly Action<string> _logger;

    public AnalyticsTimingDecorator(IAnalyticsService inner, Action<string> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public AccountBalanceSummary GetAccountBalance(Guid accountId)
        => Measure(nameof(GetAccountBalance), () => _inner.GetAccountBalance(accountId));

    public IncomeExpenseSummary GetIncomeExpense(Guid accountId, DateOnly? from = null, DateOnly? to = null)
        => Measure(nameof(GetIncomeExpense), () => _inner.GetIncomeExpense(accountId, from, to));

    public IReadOnlyCollection<CategoryTotal> GetTotalsByCategory(Guid accountId, DateOnly? from = null, DateOnly? to = null)
        => Measure(nameof(GetTotalsByCategory), () => _inner.GetTotalsByCategory(accountId, from, to));

    private T Measure<T>(string method, Func<T> callback)
    {
        var sw = Stopwatch.StartNew();
        var result = callback();
        sw.Stop();
        _logger($"Analytics::{method} executed in {sw.ElapsedMilliseconds} ms");
        return result;
    }
}

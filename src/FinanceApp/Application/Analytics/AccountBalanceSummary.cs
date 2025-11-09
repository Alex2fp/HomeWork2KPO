namespace FinanceApp.Application.Analytics;

public record AccountBalanceSummary(Guid AccountId, string AccountName, decimal Balance);

using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Importing;

public abstract class FinanceDataImporter
{
    public FinanceDataSnapshot Import(string content)
    {
        var raw = Parse(content);
        Validate(raw);
        return CreateSnapshot(raw);
    }

    protected abstract RawFinanceData Parse(string content);

    protected virtual void Validate(RawFinanceData data)
    {
        var duplicateAccounts = data.Accounts
            .GroupBy(a => a.Name.Trim().ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateAccounts.Length > 0)
        {
            throw new InvalidOperationException($"Duplicate account names: {string.Join(", ", duplicateAccounts)}");
        }

        var duplicateCategories = data.Categories
            .GroupBy(c => c.Name.Trim().ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateCategories.Length > 0)
        {
            throw new InvalidOperationException($"Duplicate category names: {string.Join(", ", duplicateCategories)}");
        }

        var accountNames = data.Accounts.Select(a => a.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var categoryNames = data.Categories.Select(c => c.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var invalidOperations = data.Operations
            .Where(o => !accountNames.Contains(o.AccountName.Trim()) || !categoryNames.Contains(o.CategoryName.Trim()))
            .Select(o => o.Description ?? "operation")
            .ToArray();

        if (invalidOperations.Length > 0)
        {
            throw new InvalidOperationException("Operations reference unknown accounts or categories");
        }
    }

    protected virtual FinanceDataSnapshot CreateSnapshot(RawFinanceData data)
    {
        var accountMap = new Dictionary<string, BankAccount>(StringComparer.OrdinalIgnoreCase);
        foreach (var account in data.Accounts)
        {
            accountMap[account.Name.Trim()] = new BankAccount(Guid.NewGuid(), account.Name, account.Currency);
        }

        var categoryMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in data.Categories)
        {
            categoryMap[category.Name.Trim()] = new Category(Guid.NewGuid(), category.Name, category.Type);
        }

        var operations = new List<Operation>();
        foreach (var operation in data.Operations)
        {
            var account = accountMap[operation.AccountName.Trim()];
            var category = categoryMap[operation.CategoryName.Trim()];
            var op = new Operation(Guid.NewGuid(), account.Id, category.Id, operation.Type, operation.Amount, operation.Date, operation.Description ?? string.Empty);
            operations.Add(op);
            account.RegisterOperation(op);
        }

        return new FinanceDataSnapshot(
            accountMap.Values.Select(a => a.Clone()).ToArray(),
            categoryMap.Values.Select(c => new Category(c.Id, c.Name, c.Type)).ToArray(),
            operations.Select(o => new Operation(o.Id, o.AccountId, o.CategoryId, o.Type, o.Amount, o.Date, o.Description)).ToArray());
    }

    protected record RawFinanceData(
        IReadOnlyCollection<RawAccount> Accounts,
        IReadOnlyCollection<RawCategory> Categories,
        IReadOnlyCollection<RawOperation> Operations);

    protected record RawAccount(string Name, string Currency);

    protected record RawCategory(string Name, CategoryType Type);

    protected record RawOperation(
        string AccountName,
        string CategoryName,
        OperationType Type,
        decimal Amount,
        DateOnly Date,
        string? Description);
}

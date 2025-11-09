using System.Collections.Generic;
using System.Linq;
using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Repositories;

public class InMemoryFinanceRepository : IFinanceRepository
{
    private readonly Dictionary<Guid, BankAccount> _accounts = new();
    private readonly Dictionary<Guid, Category> _categories = new();
    private readonly Dictionary<Guid, Operation> _operations = new();

    public IReadOnlyCollection<BankAccount> GetAccounts() => _accounts.Values
        .OrderBy(a => a.Name)
        .Select(a => a.Clone())
        .ToArray();

    public BankAccount GetAccount(Guid id)
    {
        if (!_accounts.TryGetValue(id, out var account))
        {
            throw new KeyNotFoundException($"Account {id} not found");
        }

        return account.Clone();
    }

    public BankAccount AddAccount(string name, string currency)
    {
        EnsureUniqueAccountName(name);
        var account = new BankAccount(Guid.NewGuid(), name, currency);
        _accounts.Add(account.Id, account);
        return account.Clone();
    }

    public void RenameAccount(Guid id, string newName)
    {
        EnsureUniqueAccountName(newName, id);
        var account = GetInternalAccount(id);
        account.Rename(newName);
    }

    public void RemoveAccount(Guid id)
    {
        if (!_accounts.Remove(id))
        {
            throw new KeyNotFoundException($"Account {id} not found");
        }

        foreach (var operation in _operations.Values.Where(o => o.AccountId == id).ToList())
        {
            _operations.Remove(operation.Id);
        }
    }

    public IReadOnlyCollection<Category> GetCategories() => _categories.Values
        .OrderBy(c => c.Name)
        .Select(CloneCategory)
        .ToArray();

    public Category GetCategory(Guid id)
    {
        if (!_categories.TryGetValue(id, out var category))
        {
            throw new KeyNotFoundException($"Category {id} not found");
        }

        return CloneCategory(category);
    }

    public Category AddCategory(string name, CategoryType type)
    {
        EnsureUniqueCategoryName(name);
        var category = new Category(Guid.NewGuid(), name, type);
        _categories.Add(category.Id, category);
        return CloneCategory(category);
    }

    public void UpdateCategory(Guid id, string name, CategoryType type)
    {
        EnsureUniqueCategoryName(name, id);
        var category = GetInternalCategory(id);
        category.Rename(name);
        category.ChangeType(type);
    }

    public void RemoveCategory(Guid id)
    {
        if (!_categories.Remove(id))
        {
            throw new KeyNotFoundException($"Category {id} not found");
        }

        foreach (var operation in _operations.Values.Where(o => o.CategoryId == id).ToList())
        {
            RemoveOperation(operation.Id);
        }
    }

    public IReadOnlyCollection<Operation> GetOperations() => _operations.Values
        .OrderBy(o => o.Date)
        .ThenBy(o => o.Description)
        .Select(CloneOperation)
        .ToArray();

    public IReadOnlyCollection<Operation> GetOperationsForAccount(Guid accountId) => _operations.Values
        .Where(o => o.AccountId == accountId)
        .OrderBy(o => o.Date)
        .Select(CloneOperation)
        .ToArray();

    public Operation AddOperation(Guid accountId, Guid categoryId, OperationType type, decimal amount, DateOnly date, string description)
    {
        var account = GetInternalAccount(accountId);
        var category = GetInternalCategory(categoryId);

        ValidateCategoryCompatibility(type, category);

        var operation = new Operation(Guid.NewGuid(), accountId, categoryId, type, amount, date, description);
        _operations.Add(operation.Id, operation);
        account.RegisterOperation(operation);
        return CloneOperation(operation);
    }

    public void RemoveOperation(Guid operationId)
    {
        if (!_operations.TryGetValue(operationId, out var operation))
        {
            throw new KeyNotFoundException($"Operation {operationId} not found");
        }

        var account = GetInternalAccount(operation.AccountId);
        account.RemoveOperation(operation);
        _operations.Remove(operationId);
    }

    public void ResetAccountOperations(Guid accountId)
    {
        var account = GetInternalAccount(accountId);
        account.ResetOperations();
        foreach (var operation in _operations.Values
                     .Where(o => o.AccountId == accountId)
                     .OrderBy(o => o.Date))
        {
            account.RegisterOperation(operation);
        }
    }

    public void ReplaceWithSnapshot(FinanceDataSnapshot snapshot)
    {
        _accounts.Clear();
        _categories.Clear();
        _operations.Clear();

        foreach (var account in snapshot.Accounts)
        {
            _accounts[account.Id] = new BankAccount(account.Id, account.Name, account.Currency);
        }

        foreach (var category in snapshot.Categories)
        {
            _categories[category.Id] = new Category(category.Id, category.Name, category.Type);
        }

        foreach (var operation in snapshot.Operations.OrderBy(o => o.Date))
        {
            var op = new Operation(operation.Id, operation.AccountId, operation.CategoryId, operation.Type, operation.Amount, operation.Date, operation.Description);
            _operations[op.Id] = op;
            var account = GetInternalAccount(op.AccountId);
            account.RegisterOperation(op);
        }
    }

    private BankAccount GetInternalAccount(Guid id)
    {
        if (!_accounts.TryGetValue(id, out var account))
        {
            throw new KeyNotFoundException($"Account {id} not found");
        }

        return account;
    }

    private Category GetInternalCategory(Guid id)
    {
        if (!_categories.TryGetValue(id, out var category))
        {
            throw new KeyNotFoundException($"Category {id} not found");
        }

        return category;
    }

    private static void ValidateCategoryCompatibility(OperationType type, Category category)
    {
        if (category.Type == CategoryType.Universal)
        {
            return;
        }

        if ((type == OperationType.Income && category.Type != CategoryType.Income) ||
            (type == OperationType.Expense && category.Type != CategoryType.Expense))
        {
            throw new InvalidOperationException($"Category '{category.Name}' cannot be used for {type.ToString().ToLowerInvariant()} operations");
        }
    }

    private void EnsureUniqueAccountName(string name, Guid? id = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        if (_accounts.Values.Any(a => a.Name.Trim().ToUpperInvariant() == normalized && a.Id != id))
        {
            throw new InvalidOperationException($"Account with name '{name}' already exists");
        }
    }

    private void EnsureUniqueCategoryName(string name, Guid? id = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        if (_categories.Values.Any(c => c.Name.Trim().ToUpperInvariant() == normalized && c.Id != id))
        {
            throw new InvalidOperationException($"Category with name '{name}' already exists");
        }
    }

    private static Category CloneCategory(Category category) => new(category.Id, category.Name, category.Type);

    private static Operation CloneOperation(Operation operation) => new(
        operation.Id,
        operation.AccountId,
        operation.CategoryId,
        operation.Type,
        operation.Amount,
        operation.Date,
        operation.Description);
}

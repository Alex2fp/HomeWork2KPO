using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Repositories;

public class InMemoryFinanceRepository : IFinanceRepository
{
    private readonly Dictionary<int, BankAccount> _accounts = new();
    private readonly Dictionary<int, Category> _categories = new();
    private readonly Dictionary<int, Operation> _operations = new();
    private int _nextAccountId;
    private int _nextCategoryId;
    private int _nextOperationId;

    public IReadOnlyCollection<BankAccount> GetAccounts() => _accounts.Values
        .OrderBy(a => a.Name)
        .Select(a => a.Clone())
        .ToArray();

    public BankAccount GetAccount(int id)
    {
        if (!_accounts.TryGetValue(id, out var account))
        {
            throw new KeyNotFoundException($"Счет с идентификатором {id} не найден");
        }

        return account.Clone();
    }

    public BankAccount AddAccount(string name, string currency)
    {
        EnsureUniqueAccountName(name);
        var account = new BankAccount(_nextAccountId++, name, currency);
        _accounts.Add(account.Id, account);
        return account.Clone();
    }

    public void RenameAccount(int id, string newName)
    {
        EnsureUniqueAccountName(newName, id);
        var account = GetInternalAccount(id);
        account.Rename(newName);
    }

    public void RemoveAccount(int id)
    {
        if (!_accounts.Remove(id))
        {
            throw new KeyNotFoundException($"Счет с идентификатором {id} не найден");
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

    public Category GetCategory(int id)
    {
        if (!_categories.TryGetValue(id, out var category))
        {
            throw new KeyNotFoundException($"Категория с идентификатором {id} не найдена");
        }

        return CloneCategory(category);
    }

    public Category AddCategory(string name, CategoryType type)
    {
        EnsureUniqueCategoryName(name);
        var category = new Category(_nextCategoryId++, name, type);
        _categories.Add(category.Id, category);
        return CloneCategory(category);
    }

    public void UpdateCategory(int id, string name, CategoryType type)
    {
        EnsureUniqueCategoryName(name, id);
        var category = GetInternalCategory(id);
        category.Rename(name);
        category.ChangeType(type);
    }

    public void RemoveCategory(int id)
    {
        if (!_categories.Remove(id))
        {
            throw new KeyNotFoundException($"Категория с идентификатором {id} не найдена");
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

    public IReadOnlyCollection<Operation> GetOperationsForAccount(int accountId) => _operations.Values
        .Where(o => o.AccountId == accountId)
        .OrderBy(o => o.Date)
        .Select(CloneOperation)
        .ToArray();

    public Operation AddOperation(int accountId, int categoryId, OperationType type, decimal amount, DateOnly date, string description)
    {
        var account = GetInternalAccount(accountId);
        var category = GetInternalCategory(categoryId);

        ValidateCategoryCompatibility(type, category);

        var operation = new Operation(_nextOperationId++, accountId, categoryId, type, amount, date, description);
        _operations.Add(operation.Id, operation);
        account.RegisterOperation(operation);
        return CloneOperation(operation);
    }

    public void RemoveOperation(int operationId)
    {
        if (!_operations.TryGetValue(operationId, out var operation))
        {
            throw new KeyNotFoundException($"Операция с идентификатором {operationId} не найдена");
        }

        var account = GetInternalAccount(operation.AccountId);
        account.RemoveOperation(operation);
        _operations.Remove(operationId);
    }

    public void ResetAccountOperations(int accountId)
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
        _nextAccountId = 0;
        _nextCategoryId = 0;
        _nextOperationId = 0;

        foreach (var account in snapshot.Accounts)
        {
            _accounts[account.Id] = new BankAccount(account.Id, account.Name, account.Currency);
            _nextAccountId = Math.Max(_nextAccountId, account.Id + 1);
        }

        foreach (var category in snapshot.Categories)
        {
            _categories[category.Id] = new Category(category.Id, category.Name, category.Type);
            _nextCategoryId = Math.Max(_nextCategoryId, category.Id + 1);
        }

        foreach (var operation in snapshot.Operations.OrderBy(o => o.Date))
        {
            var op = new Operation(operation.Id, operation.AccountId, operation.CategoryId, operation.Type, operation.Amount, operation.Date, operation.Description);
            _operations[op.Id] = op;
            var account = GetInternalAccount(op.AccountId);
            account.RegisterOperation(op);
            _nextOperationId = Math.Max(_nextOperationId, operation.Id + 1);
        }
    }

    public void MergeWithSnapshot(FinanceDataSnapshot snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        foreach (var account in snapshot.Accounts)
        {
            if (_accounts.TryGetValue(account.Id, out var existingAccount))
            {
                EnsureUniqueAccountName(account.Name, account.Id);
                existingAccount.Rename(account.Name);
            }
            else
            {
                EnsureUniqueAccountName(account.Name);
                _accounts[account.Id] = new BankAccount(account.Id, account.Name, account.Currency);
            }

            _nextAccountId = Math.Max(_nextAccountId, account.Id + 1);
        }

        foreach (var category in snapshot.Categories)
        {
            if (_categories.TryGetValue(category.Id, out var existingCategory))
            {
                EnsureUniqueCategoryName(category.Name, category.Id);
                existingCategory.Rename(category.Name);
                existingCategory.ChangeType(category.Type);
            }
            else
            {
                EnsureUniqueCategoryName(category.Name);
                _categories[category.Id] = new Category(category.Id, category.Name, category.Type);
            }

            _nextCategoryId = Math.Max(_nextCategoryId, category.Id + 1);
        }

        foreach (var operation in snapshot.Operations
                     .OrderBy(o => o.Date)
                     .ThenBy(o => o.Id))
        {
            if (_operations.ContainsKey(operation.Id))
            {
                continue;
            }

            var account = GetInternalAccount(operation.AccountId);
            var category = GetInternalCategory(operation.CategoryId);
            ValidateCategoryCompatibility(operation.Type, category);

            var op = new Operation(
                operation.Id,
                operation.AccountId,
                operation.CategoryId,
                operation.Type,
                operation.Amount,
                operation.Date,
                operation.Description);

            _operations[op.Id] = op;

            if (!account.OperationIds.Contains(op.Id))
            {
                account.RegisterOperation(op);
            }

            _nextOperationId = Math.Max(_nextOperationId, op.Id + 1);
        }
    }

    private BankAccount GetInternalAccount(int id)
    {
        if (!_accounts.TryGetValue(id, out var account))
        {
            throw new KeyNotFoundException($"Счет с идентификатором {id} не найден");
        }

        return account;
    }

    private Category GetInternalCategory(int id)
    {
        if (!_categories.TryGetValue(id, out var category))
        {
            throw new KeyNotFoundException($"Категория с идентификатором {id} не найдена");
        }

        return category;
    }

    private static void ValidateCategoryCompatibility(OperationType type, Category category)
    {
        if ((type == OperationType.Income && category.Type != CategoryType.Income) ||
            (type == OperationType.Expense && category.Type != CategoryType.Expense))
        {
            var operationName = type == OperationType.Income ? "доходными" : "расходными";
            throw new InvalidOperationException($"Категорию '{category.Name}' нельзя использовать для {operationName} операций");
        }
    }

    private void EnsureUniqueAccountName(string name, int? id = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        if (_accounts.Values.Any(a => a.Name.Trim().ToUpperInvariant() == normalized && a.Id != id))
        {
            throw new InvalidOperationException($"Счет с названием '{name}' уже существует");
        }
    }

    private void EnsureUniqueCategoryName(string name, int? id = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        if (_categories.Values.Any(c => c.Name.Trim().ToUpperInvariant() == normalized && c.Id != id))
        {
            throw new InvalidOperationException($"Категория с названием '{name}' уже существует");
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

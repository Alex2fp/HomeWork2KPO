using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Domain;

public class BankAccount : IExportable
{
    private readonly List<Guid> _operationIds;

    public BankAccount(Guid id, string name, string currency)
        : this(id, name, currency, 0m, Enumerable.Empty<Guid>())
    {
    }

    internal BankAccount(Guid id, string name, string currency, decimal balance, IEnumerable<Guid> operationIds)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Account name cannot be empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
        }

        Id = id;
        Name = name.Trim();
        Currency = currency.Trim().ToUpperInvariant();
        Balance = balance;
        _operationIds = operationIds.ToList();
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Currency { get; }
    public decimal Balance { get; private set; }
    public IReadOnlyCollection<Guid> OperationIds => _operationIds.AsReadOnly();

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Account name cannot be empty", nameof(newName));
        }

        Name = newName.Trim();
    }

    public void RegisterOperation(Operation operation)
    {
        if (operation.AccountId != Id)
        {
            throw new InvalidOperationException("Operation does not belong to this account");
        }

        _operationIds.Add(operation.Id);
        Apply(operation);
    }

    public void RemoveOperation(Operation operation)
    {
        _operationIds.Remove(operation.Id);
        Apply(operation, reverse: true);
    }

    public void ResetOperations()
    {
        _operationIds.Clear();
        Balance = 0m;
    }

    public void Apply(Operation operation, bool reverse = false)
    {
        var multiplier = operation.Type == OperationType.Income ? 1 : -1;
        if (reverse)
        {
            multiplier *= -1;
        }

        Balance += multiplier * operation.Amount;
    }

    internal BankAccount Clone() => new(Id, Name, Currency, Balance, _operationIds);

    public void Accept(IFinanceDataExportVisitor visitor)
    {
        visitor.VisitAccount(this);
    }
}

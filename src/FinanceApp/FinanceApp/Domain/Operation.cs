namespace FinanceApp.Domain;

public class Operation : IExportable
{
    public Operation(
        int id,
        int accountId,
        int categoryId,
        OperationType type,
        decimal amount,
        DateOnly date,
        string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        Id = id;
        AccountId = accountId;
        CategoryId = categoryId;
        Type = type;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Date = date;
        Description = description?.Trim() ?? string.Empty;
    }

    public int Id { get; }
    public int AccountId { get; }
    public int CategoryId { get; }
    public OperationType Type { get; }
    public decimal Amount { get; }
    public DateOnly Date { get; }
    public string Description { get; }

    public void Accept(IFinanceDataExportVisitor visitor)
    {
        visitor.VisitOperation(this);
    }
}

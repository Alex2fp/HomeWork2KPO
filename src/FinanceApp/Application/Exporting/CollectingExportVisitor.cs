using System.Collections.Generic;
using FinanceApp.Domain;

namespace FinanceApp.Application.Exporting;

public abstract class CollectingExportVisitor : IFinanceDataExportVisitor
{
    protected readonly List<BankAccount> Accounts = new();
    protected readonly List<Category> Categories = new();
    protected readonly List<Operation> Operations = new();

    public void VisitAccount(BankAccount account) => Accounts.Add(account);
    public void VisitCategory(Category category) => Categories.Add(category);
    public void VisitOperation(Operation operation) => Operations.Add(operation);

    public abstract string Build();
}

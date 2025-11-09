namespace FinanceApp.Domain;

public interface IFinanceDataExportVisitor
{
    void VisitAccount(BankAccount account);
    void VisitCategory(Category category);
    void VisitOperation(Operation operation);
}

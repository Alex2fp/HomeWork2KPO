namespace FinanceApp.Domain;

public interface IExportable
{
    void Accept(IFinanceDataExportVisitor visitor);
}

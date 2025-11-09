using FinanceApp.Application.Analytics;
using FinanceApp.Application.Commands;
using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Facade;

public interface IFinanceFacade
{
    IReadOnlyCollection<BankAccount> GetAccounts();
    BankAccount CreateAccount(string name, string currency);
    void RenameAccount(Guid id, string newName);
    void DeleteAccount(Guid id);

    IReadOnlyCollection<Category> GetCategories();
    Category CreateCategory(string name, CategoryType type);
    void UpdateCategory(Guid id, string name, CategoryType type);
    void DeleteCategory(Guid id);

    IReadOnlyCollection<Operation> GetOperations(Guid accountId);
    Operation CreateOperation(Guid accountId, Guid categoryId, OperationType type, decimal amount, DateOnly date, string description);
    void DeleteOperation(Guid id);

    AccountBalanceSummary GetAccountBalance(Guid accountId);
    IncomeExpenseSummary GetIncomeExpense(Guid accountId, DateOnly? from = null, DateOnly? to = null);
    IReadOnlyCollection<CategoryTotal> GetCategoryTotals(Guid accountId, DateOnly? from = null, DateOnly? to = null);

    void RecalculateBalance(Guid accountId);

    FinanceDataSnapshot Import(string format, string content, bool apply = false);
    string Export(string format);
}

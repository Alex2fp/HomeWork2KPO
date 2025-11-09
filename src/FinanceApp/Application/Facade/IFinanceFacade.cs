using FinanceApp.Application.Analytics;
using FinanceApp.Application.Commands;
using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Facade;

public interface IFinanceFacade
{
    IReadOnlyCollection<BankAccount> GetAccounts();
    BankAccount CreateAccount(string name, string currency);
    void RenameAccount(int id, string newName);
    void DeleteAccount(int id);

    IReadOnlyCollection<Category> GetCategories();
    Category CreateCategory(string name, CategoryType type);
    void UpdateCategory(int id, string name, CategoryType type);
    void DeleteCategory(int id);

    IReadOnlyCollection<Operation> GetOperations(int accountId);
    Operation CreateOperation(int accountId, int categoryId, OperationType type, decimal amount, DateOnly date, string description);
    void DeleteOperation(int id);

    AccountBalanceSummary GetAccountBalance(int accountId);
    IncomeExpenseSummary GetIncomeExpense(int accountId, DateOnly? from = null, DateOnly? to = null);
    IReadOnlyCollection<CategoryTotal> GetCategoryTotals(int accountId, DateOnly? from = null, DateOnly? to = null);

    void RecalculateBalance(int accountId);

    FinanceDataSnapshot ImportFromFile(string path, bool apply = false);
    void ExportToFile(string path);
}

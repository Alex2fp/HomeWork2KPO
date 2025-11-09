using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Repositories;

public interface IFinanceRepository
{
    IReadOnlyCollection<BankAccount> GetAccounts();
    BankAccount GetAccount(int id);
    BankAccount AddAccount(string name, string currency);
    void RenameAccount(int id, string newName);
    void RemoveAccount(int id);

    IReadOnlyCollection<Category> GetCategories();
    Category GetCategory(int id);
    Category AddCategory(string name, CategoryType type);
    void UpdateCategory(int id, string name, CategoryType type);
    void RemoveCategory(int id);

    IReadOnlyCollection<Operation> GetOperations();
    IReadOnlyCollection<Operation> GetOperationsForAccount(int accountId);
    Operation AddOperation(int accountId, int categoryId, OperationType type, decimal amount, DateOnly date, string description);
    void RemoveOperation(int operationId);

    void ResetAccountOperations(int accountId);
    void ReplaceWithSnapshot(FinanceDataSnapshot snapshot);
    void MergeWithSnapshot(FinanceDataSnapshot snapshot);
}

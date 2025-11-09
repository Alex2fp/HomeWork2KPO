using FinanceApp.Application.Data;
using FinanceApp.Domain;

namespace FinanceApp.Application.Repositories;

public interface IFinanceRepository
{
    IReadOnlyCollection<BankAccount> GetAccounts();
    BankAccount GetAccount(Guid id);
    BankAccount AddAccount(string name, string currency);
    void RenameAccount(Guid id, string newName);
    void RemoveAccount(Guid id);

    IReadOnlyCollection<Category> GetCategories();
    Category GetCategory(Guid id);
    Category AddCategory(string name, CategoryType type);
    void UpdateCategory(Guid id, string name, CategoryType type);
    void RemoveCategory(Guid id);

    IReadOnlyCollection<Operation> GetOperations();
    IReadOnlyCollection<Operation> GetOperationsForAccount(Guid accountId);
    Operation AddOperation(Guid accountId, Guid categoryId, OperationType type, decimal amount, DateOnly date, string description);
    void RemoveOperation(Guid operationId);

    void ResetAccountOperations(Guid accountId);
    void ReplaceWithSnapshot(FinanceDataSnapshot snapshot);
}

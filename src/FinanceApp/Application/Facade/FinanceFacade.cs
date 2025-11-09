using FinanceApp.Application.Analytics;
using FinanceApp.Application.Commands;
using FinanceApp.Application.Data;
using FinanceApp.Application.Exporting;
using FinanceApp.Application.Importing;
using FinanceApp.Application.Repositories;
using FinanceApp.Domain;

namespace FinanceApp.Application.Facade;

public class FinanceFacade : IFinanceFacade
{
    private readonly IFinanceRepository _repository;
    private readonly IAnalyticsService _analyticsService;
    private readonly ICommandFactory _commandFactory;
    private readonly IFinanceDataImportService _importService;
    private readonly IFinanceDataExportService _exportService;

    public FinanceFacade(
        IFinanceRepository repository,
        IAnalyticsService analyticsService,
        ICommandFactory commandFactory,
        IFinanceDataImportService importService,
        IFinanceDataExportService exportService)
    {
        _repository = repository;
        _analyticsService = analyticsService;
        _commandFactory = commandFactory;
        _importService = importService;
        _exportService = exportService;
    }

    public IReadOnlyCollection<BankAccount> GetAccounts() => _repository.GetAccounts();

    public BankAccount CreateAccount(string name, string currency) => _repository.AddAccount(name, currency);

    public void RenameAccount(int id, string newName) => _repository.RenameAccount(id, newName);

    public void DeleteAccount(int id) => _repository.RemoveAccount(id);

    public IReadOnlyCollection<Category> GetCategories() => _repository.GetCategories();

    public Category CreateCategory(string name, CategoryType type) => _repository.AddCategory(name, type);

    public void UpdateCategory(int id, string name, CategoryType type) => _repository.UpdateCategory(id, name, type);

    public void DeleteCategory(int id) => _repository.RemoveCategory(id);

    public IReadOnlyCollection<Operation> GetOperations(int accountId) => _repository.GetOperationsForAccount(accountId);

    public Operation CreateOperation(int accountId, int categoryId, OperationType type, decimal amount, DateOnly date, string description)
        => _repository.AddOperation(accountId, categoryId, type, amount, date, description);

    public void DeleteOperation(int id) => _repository.RemoveOperation(id);

    public AccountBalanceSummary GetAccountBalance(int accountId) => _analyticsService.GetAccountBalance(accountId);

    public IncomeExpenseSummary GetIncomeExpense(int accountId, DateOnly? from = null, DateOnly? to = null)
        => _analyticsService.GetIncomeExpense(accountId, from, to);

    public IReadOnlyCollection<CategoryTotal> GetCategoryTotals(int accountId, DateOnly? from = null, DateOnly? to = null)
        => _analyticsService.GetTotalsByCategory(accountId, from, to);

    public void RecalculateBalance(int accountId)
    {
        var command = _commandFactory.CreateRecalculateBalance(accountId);
        command.Execute();
    }

    public FinanceDataSnapshot ImportFromFile(string path, bool apply = false)
    {
        var snapshot = _importService.ImportFromFile(path);
        if (apply)
        {
            _importService.Apply(snapshot);
        }

        return snapshot;
    }

    public void ExportToFile(string path) => _exportService.ExportToFile(path);
}

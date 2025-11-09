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

    public void RenameAccount(Guid id, string newName) => _repository.RenameAccount(id, newName);

    public void DeleteAccount(Guid id) => _repository.RemoveAccount(id);

    public IReadOnlyCollection<Category> GetCategories() => _repository.GetCategories();

    public Category CreateCategory(string name, CategoryType type) => _repository.AddCategory(name, type);

    public void UpdateCategory(Guid id, string name, CategoryType type) => _repository.UpdateCategory(id, name, type);

    public void DeleteCategory(Guid id) => _repository.RemoveCategory(id);

    public IReadOnlyCollection<Operation> GetOperations(Guid accountId) => _repository.GetOperationsForAccount(accountId);

    public Operation CreateOperation(Guid accountId, Guid categoryId, OperationType type, decimal amount, DateOnly date, string description)
        => _repository.AddOperation(accountId, categoryId, type, amount, date, description);

    public void DeleteOperation(Guid id) => _repository.RemoveOperation(id);

    public AccountBalanceSummary GetAccountBalance(Guid accountId) => _analyticsService.GetAccountBalance(accountId);

    public IncomeExpenseSummary GetIncomeExpense(Guid accountId, DateOnly? from = null, DateOnly? to = null)
        => _analyticsService.GetIncomeExpense(accountId, from, to);

    public IReadOnlyCollection<CategoryTotal> GetCategoryTotals(Guid accountId, DateOnly? from = null, DateOnly? to = null)
        => _analyticsService.GetTotalsByCategory(accountId, from, to);

    public void RecalculateBalance(Guid accountId)
    {
        var command = _commandFactory.CreateRecalculateBalance(accountId);
        command.Execute();
    }

    public FinanceDataSnapshot Import(string format, string content, bool apply = false)
    {
        var snapshot = _importService.Import(format, content);
        if (apply)
        {
            _importService.Apply(snapshot);
        }

        return snapshot;
    }

    public string Export(string format) => _exportService.Export(format);
}

using System;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Exporting;

public interface IFinanceDataExportService
{
    string Export(string format);
}

public class FinanceDataExportService : IFinanceDataExportService
{
    private readonly IFinanceRepository _repository;

    public FinanceDataExportService(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public string Export(string format)
    {
        var visitor = CreateVisitor(format);

        foreach (var account in _repository.GetAccounts())
        {
            visitor.VisitAccount(account);
        }

        foreach (var category in _repository.GetCategories())
        {
            visitor.VisitCategory(category);
        }

        foreach (var operation in _repository.GetOperations())
        {
            visitor.VisitOperation(operation);
        }

        return visitor.Build();
    }

    private CollectingExportVisitor CreateVisitor(string format) => format.ToLowerInvariant() switch
    {
        "csv" => new CsvExportVisitor(),
        "json" => new JsonExportVisitor(),
        "yaml" => new YamlExportVisitor(),
        _ => throw new NotSupportedException($"Format '{format}' is not supported")
    };
}

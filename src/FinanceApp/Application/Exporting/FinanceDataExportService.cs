using System;
using System.IO;
using System.Text;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Exporting;

public interface IFinanceDataExportService
{
    void ExportToFile(string path);
}

public class FinanceDataExportService : IFinanceDataExportService
{
    private readonly IFinanceRepository _repository;

    public FinanceDataExportService(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public void ExportToFile(string path)
    {
        var format = GetFormatFromPath(path);
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

        var content = visitor.Build();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content, Encoding.UTF8);
    }

    private CollectingExportVisitor CreateVisitor(string format) => format.ToLowerInvariant() switch
    {
        "csv" => new CsvExportVisitor(),
        "json" => new JsonExportVisitor(),
        "yaml" => new YamlExportVisitor(),
        _ => throw new NotSupportedException($"Format '{format}' is not supported")
    };

    private static string GetFormatFromPath(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new NotSupportedException("Не удалось определить формат файла");
        }

        var format = extension.TrimStart('.').ToLowerInvariant();
        return format switch
        {
            "csv" => "csv",
            "json" => "json",
            "jsony" => "json",
            "yaml" => "yaml",
            "yml" => "yaml",
            _ => throw new NotSupportedException($"Формат '{extension}' не поддерживается")
        };
    }
}

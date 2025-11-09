using System;
using System.IO;
using System.Text;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Exporting;

public interface IFinanceDataExportService
{
    string Export(string folderPath, string format);
}

public class FinanceDataExportService : IFinanceDataExportService
{
    private readonly IFinanceRepository _repository;

    public FinanceDataExportService(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public string Export(string folderPath, string format)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException("Путь к папке не может быть пустым", nameof(folderPath));
        }

        var normalizedFormat = NormalizeFormat(format);
        var extension = GetExtension(normalizedFormat);
        var baseFileName = $"finance_export_{DateTime.Now:yyyyMMdd_HHmmss}";
        var directoryPath = Path.GetFullPath(folderPath);
        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, $"{baseFileName}.{extension}");
        var suffix = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(directoryPath, $"{baseFileName}_{suffix}.{extension}");
            suffix++;
        }

        WriteToFile(filePath, normalizedFormat);
        return filePath;
    }

    private void WriteToFile(string path, string format)
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

        var content = visitor.Build();
        File.WriteAllText(path, content, Encoding.UTF8);
    }

    private static CollectingExportVisitor CreateVisitor(string format) => format switch
    {
        "csv" => new CsvExportVisitor(),
        "json" => new JsonExportVisitor(),
        "yaml" => new YamlExportVisitor(),
        _ => throw new NotSupportedException($"Формат '{format}' не поддерживается")
    };

    private static string NormalizeFormat(string format)
    {
        var normalized = (format ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "csv" => "csv",
            "json" or "jsony" => "json",
            "yaml" or "yml" => "yaml",
            _ => throw new NotSupportedException("Укажите формат из списка: csv, json или yaml")
        };
    }

    private static string GetExtension(string format) => format switch
    {
        "csv" => "csv",
        "json" => "json",
        "yaml" => "yaml",
        _ => format
    };
}

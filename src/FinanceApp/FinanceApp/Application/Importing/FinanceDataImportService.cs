using System;
using System.IO;
using FinanceApp.Application.Data;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Importing;

public interface IFinanceDataImportService
{
    FinanceDataSnapshot ImportFromFile(string path);
    void Apply(FinanceDataSnapshot snapshot);
}

public class FinanceDataImportService : IFinanceDataImportService
{
    private readonly IFinanceDataImporterFactory _factory;
    private readonly IFinanceRepository _repository;

    public FinanceDataImportService(IFinanceDataImporterFactory factory, IFinanceRepository repository)
    {
        _factory = factory;
        _repository = repository;
    }

    public FinanceDataSnapshot ImportFromFile(string path)
    {
        var format = GetFormatFromPath(path);
        var content = File.ReadAllText(path);
        return Import(format, content);
    }

    public void Apply(FinanceDataSnapshot snapshot)
    {
        _repository.ReplaceWithSnapshot(snapshot);
    }

    private FinanceDataSnapshot Import(string format, string content)
    {
        var importer = _factory.Create(format);
        return importer.Import(content);
    }

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

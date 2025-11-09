using System;

namespace FinanceApp.Application.Importing;

public interface IFinanceDataImporterFactory
{
    FinanceDataImporter Create(string format);
}

public class FinanceDataImporterFactory : IFinanceDataImporterFactory
{
    public FinanceDataImporter Create(string format) => format.ToLowerInvariant() switch
    {
        "csv" => new CsvFinanceDataImporter(),
        "json" => new JsonFinanceDataImporter(),
        "yaml" => new YamlFinanceDataImporter(),
        _ => throw new NotSupportedException($"Format '{format}' is not supported")
    };
}

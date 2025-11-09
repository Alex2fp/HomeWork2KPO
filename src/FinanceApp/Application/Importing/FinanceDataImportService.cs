using FinanceApp.Application.Data;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Importing;

public interface IFinanceDataImportService
{
    FinanceDataSnapshot Import(string format, string content);
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

    public FinanceDataSnapshot Import(string format, string content)
    {
        var importer = _factory.Create(format);
        return importer.Import(content);
    }

    public void Apply(FinanceDataSnapshot snapshot)
    {
        _repository.ReplaceWithSnapshot(snapshot);
    }
}

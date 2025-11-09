using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Commands;

public interface ICommandFactory
{
    ICommand CreateRecalculateBalance(int accountId);
}

public class CommandFactory : ICommandFactory
{
    private readonly IFinanceRepository _repository;

    public CommandFactory(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public ICommand CreateRecalculateBalance(int accountId) => new RecalculateBalanceCommand(_repository, accountId);
}

using System;
using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Commands;

public interface ICommandFactory
{
    ICommand CreateRecalculateBalance(Guid accountId);
}

public class CommandFactory : ICommandFactory
{
    private readonly IFinanceRepository _repository;

    public CommandFactory(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public ICommand CreateRecalculateBalance(Guid accountId) => new RecalculateBalanceCommand(_repository, accountId);
}

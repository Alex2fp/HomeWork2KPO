using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Commands;

public class RecalculateBalanceCommand : ICommand
{
    private readonly IFinanceRepository _repository;
    private readonly Guid _accountId;

    public RecalculateBalanceCommand(IFinanceRepository repository, Guid accountId)
    {
        _repository = repository;
        _accountId = accountId;
    }

    public void Execute()
    {
        _repository.ResetAccountOperations(_accountId);
    }
}

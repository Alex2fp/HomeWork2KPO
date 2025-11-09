using FinanceApp.Application.Repositories;

namespace FinanceApp.Application.Commands;

public class RecalculateBalanceCommand : ICommand
{
    private readonly IFinanceRepository _repository;
    private readonly int _accountId;

    public RecalculateBalanceCommand(IFinanceRepository repository, int accountId)
    {
        _repository = repository;
        _accountId = accountId;
    }

    public void Execute()
    {
        _repository.ResetAccountOperations(_accountId);
    }
}

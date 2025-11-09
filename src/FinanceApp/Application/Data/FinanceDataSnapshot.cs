using System.Collections.Generic;
using FinanceApp.Domain;

namespace FinanceApp.Application.Data;

public record FinanceDataSnapshot(
    IReadOnlyCollection<BankAccount> Accounts,
    IReadOnlyCollection<Category> Categories,
    IReadOnlyCollection<Operation> Operations);

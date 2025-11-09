using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FinanceApp.Application.Exporting;

public class CsvExportVisitor : CollectingExportVisitor
{
    public override string Build()
    {
        var builder = new StringBuilder();
        builder.AppendLine("[Accounts]");
        foreach (var account in Accounts.OrderBy(a => a.Name))
        {
            builder.AppendLine($"{account.Name},{account.Currency},{account.Balance.ToString(CultureInfo.InvariantCulture)}");
        }

        builder.AppendLine();
        builder.AppendLine("[Categories]");
        foreach (var category in Categories.OrderBy(c => c.Name))
        {
            builder.AppendLine($"{category.Name},{category.Type}");
        }

        builder.AppendLine();
        builder.AppendLine("[Operations]");
        foreach (var operation in Operations.OrderBy(o => o.Date))
        {
            builder.AppendLine($"{GetAccountName(operation.AccountId)},{GetCategoryName(operation.CategoryId)},{operation.Type},{operation.Amount.ToString(CultureInfo.InvariantCulture)},{operation.Date:dd-MM-yyyy},{operation.Description}");
        }

        return builder.ToString();
    }

    private string GetAccountName(int accountId) => Accounts.First(a => a.Id == accountId).Name;

    private string GetCategoryName(int categoryId) => Categories.First(c => c.Id == categoryId).Name;
}

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FinanceApp.Application.Exporting;

public class YamlExportVisitor : CollectingExportVisitor
{
    public override string Build()
    {
        var builder = new StringBuilder();
        builder.AppendLine("accounts:");
        foreach (var account in Accounts.OrderBy(a => a.Name))
        {
            builder.AppendLine("  - name: " + account.Name);
            builder.AppendLine("    currency: " + account.Currency);
            builder.AppendLine("    balance: " + account.Balance.ToString(CultureInfo.InvariantCulture));
        }

        builder.AppendLine("categories:");
        foreach (var category in Categories.OrderBy(c => c.Name))
        {
            builder.AppendLine("  - name: " + category.Name);
            builder.AppendLine("    type: " + category.Type);
        }

        builder.AppendLine("operations:");
        foreach (var operation in Operations.OrderBy(o => o.Date))
        {
            builder.AppendLine("  - account: " + GetAccountName(operation.AccountId));
            builder.AppendLine("    category: " + GetCategoryName(operation.CategoryId));
            builder.AppendLine("    type: " + operation.Type);
            builder.AppendLine("    amount: " + operation.Amount.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("    date: " + operation.Date.ToString("dd-MM-yyyy"));
            if (!string.IsNullOrWhiteSpace(operation.Description))
            {
                builder.AppendLine("    description: \"" + operation.Description.Replace("\"", "\\\"") + "\"");
            }
        }

        return builder.ToString();
    }

    private string GetAccountName(int accountId) => Accounts.First(a => a.Id == accountId).Name;

    private string GetCategoryName(int categoryId) => Categories.First(c => c.Id == categoryId).Name;
}

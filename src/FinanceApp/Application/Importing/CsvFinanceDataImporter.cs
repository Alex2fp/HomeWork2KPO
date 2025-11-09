using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FinanceApp.Application.Importing;

public class CsvFinanceDataImporter : FinanceDataImporter
{
    protected override RawFinanceData Parse(string content)
    {
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .ToArray();

        var section = string.Empty;
        var accounts = new List<RawAccount>();
        var categories = new List<RawCategory>();
        var operations = new List<RawOperation>();

        foreach (var line in lines)
        {
            if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
            {
                section = line.Trim('[', ']');
                continue;
            }

            if (string.IsNullOrWhiteSpace(section) || line.StartsWith("#"))
            {
                continue;
            }

            var parts = line.Split(',').Select(p => p.Trim()).ToArray();
            switch (section.ToLowerInvariant())
            {
                case "accounts":
                    accounts.Add(new RawAccount(parts[0], parts.Length > 1 ? parts[1] : "RUB"));
                    break;
                case "categories":
                    var type = Enum.Parse<CategoryType>(parts[1], ignoreCase: true);
                    categories.Add(new RawCategory(parts[0], type));
                    break;
                case "operations":
                    var opType = Enum.Parse<OperationType>(parts[2], ignoreCase: true);
                    var amount = decimal.Parse(parts[3], CultureInfo.InvariantCulture);
                    var date = DateOnly.Parse(parts[4], CultureInfo.InvariantCulture);
                    var description = parts.Length > 5 ? parts[5] : string.Empty;
                    operations.Add(new RawOperation(parts[0], parts[1], opType, amount, date, description));
                    break;
            }
        }

        return new RawFinanceData(accounts, categories, operations);
    }
}

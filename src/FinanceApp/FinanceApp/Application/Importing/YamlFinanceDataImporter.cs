using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FinanceApp.Application.Importing;

public class YamlFinanceDataImporter : FinanceDataImporter
{
    protected override RawFinanceData Parse(string content)
    {
        var accounts = new List<Dictionary<string, string>>();
        var categories = new List<Dictionary<string, string>>();
        var operations = new List<Dictionary<string, string>>();

        List<Dictionary<string, string>>? currentSection = null;
        Dictionary<string, string>? currentItem = null;

        foreach (var rawLine in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            {
                continue;
            }

            if (!char.IsWhiteSpace(line[0]))
            {
                var sectionName = line.TrimEnd(':').Trim();
                currentSection = sectionName.ToLowerInvariant() switch
                {
                    "accounts" => accounts,
                    "categories" => categories,
                    "operations" => operations,
                    _ => null
                };
                currentItem = null;
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("-"))
            {
                currentItem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                currentSection?.Add(currentItem);
                continue;
            }

            if (currentItem is null)
            {
                continue;
            }

            var separatorIndex = trimmed.IndexOf(':');
            if (separatorIndex < 0)
            {
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
            currentItem[key] = value;
        }

        var rawAccounts = accounts
            .Select(a => new RawAccount(a.GetValueOrDefault("name", "Unnamed"), a.GetValueOrDefault("currency", "RUB")))
            .ToArray();
        var rawCategories = categories
            .Select(c => new RawCategory(
                c.GetValueOrDefault("name", "Unknown"),
                Enum.Parse<Domain.CategoryType>(c.GetValueOrDefault("type", nameof(Domain.CategoryType.Expense)), true)))
            .ToArray();
        var rawOperations = operations
            .Select(o => new RawOperation(
                o.GetValueOrDefault("account", string.Empty),
                o.GetValueOrDefault("category", string.Empty),
                Enum.Parse<Domain.OperationType>(o.GetValueOrDefault("type", nameof(Domain.OperationType.Expense)), true),
                decimal.Parse(o.GetValueOrDefault("amount", "0"), CultureInfo.InvariantCulture),
                DateOnly.ParseExact(
                    o.GetValueOrDefault("date", DateOnly.FromDateTime(DateTime.Today).ToString("dd-MM-yyyy")),
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture),
                o.GetValueOrDefault("description", string.Empty)))
            .ToArray();

        return new RawFinanceData(rawAccounts, rawCategories, rawOperations);
    }
}

internal static class DictionaryExtensions
{
    public static string GetValueOrDefault(this Dictionary<string, string> dictionary, string key, string defaultValue)
        => dictionary.TryGetValue(key, out var value) ? value : defaultValue;
}

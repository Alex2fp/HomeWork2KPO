using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Application.Importing;

public class JsonFinanceDataImporter : FinanceDataImporter
{
    protected override RawFinanceData Parse(string content)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var model = JsonSerializer.Deserialize<JsonFinanceModel>(content, options)
            ?? throw new InvalidOperationException("Unable to parse JSON finance data");

        return new RawFinanceData(
            model.Accounts.Select(a => new RawAccount(a.Name, a.Currency)).ToArray(),
            model.Categories.Select(c => new RawCategory(c.Name, c.Type)).ToArray(),
            model.Operations.Select(o => new RawOperation(
                o.Account,
                o.Category,
                o.Type,
                o.Amount,
                DateOnly.ParseExact(o.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                o.Description)).ToArray());
    }

    private record JsonFinanceModel(
        JsonAccountModel[] Accounts,
        JsonCategoryModel[] Categories,
        JsonOperationModel[] Operations);

    private record JsonAccountModel(string Name, string Currency);

    private record JsonCategoryModel(string Name, Domain.CategoryType Type);

    private record JsonOperationModel(string Account, string Category, Domain.OperationType Type, decimal Amount, string Date, string? Description);
}

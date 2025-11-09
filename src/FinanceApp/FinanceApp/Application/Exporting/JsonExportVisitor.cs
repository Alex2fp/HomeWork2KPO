using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Application.Exporting;

public class JsonExportVisitor : CollectingExportVisitor
{
    public override string Build()
    {
        var payload = new
        {
            accounts = Accounts.OrderBy(a => a.Name).Select(a => new { a.Id, a.Name, a.Currency, a.Balance }),
            categories = Categories.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name, c.Type }),
            operations = Operations.OrderBy(o => o.Date).Select(o => new
            {
                o.Id,
                o.AccountId,
                o.CategoryId,
                o.Type,
                o.Amount,
                date = o.Date.ToString("dd-MM-yyyy"),
                o.Description
            })
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(payload, options);
    }
}

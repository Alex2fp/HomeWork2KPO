using System;
using FinanceApp.Application.Analytics;
using FinanceApp.Application.Commands;
using FinanceApp.Application.Exporting;
using FinanceApp.Application.Facade;
using FinanceApp.Application.Importing;
using FinanceApp.Application.Repositories;
using FinanceApp.Domain;
using FinanceApp.Presentation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IFinanceRepository, InMemoryFinanceRepository>();
services.AddSingleton<AnalyticsService>();
services.AddSingleton<IAnalyticsService>(provider =>
{
    var inner = provider.GetRequiredService<AnalyticsService>();
    return new AnalyticsTimingDecorator(inner, message => Console.WriteLine($"[METRICS] {message}"));
});
services.AddSingleton<ICommandFactory, CommandFactory>();
services.AddSingleton<IFinanceDataImporterFactory, FinanceDataImporterFactory>();
services.AddSingleton<IFinanceDataImportService, FinanceDataImportService>();
services.AddSingleton<IFinanceDataExportService, FinanceDataExportService>();
services.AddSingleton<IFinanceFacade, FinanceFacade>();
services.AddSingleton<ConsoleApplication>();

var provider = services.BuildServiceProvider();
Seed(provider.GetRequiredService<IFinanceFacade>());
var app = provider.GetRequiredService<ConsoleApplication>();
app.Run();

static void Seed(IFinanceFacade facade)
{
    if (facade.GetAccounts().Count > 0)
    {
        return;
    }

    var account = facade.CreateAccount("Наличные", "RUB");
    var salary = facade.CreateCategory("Зарплата", CategoryType.Income);
    var groceries = facade.CreateCategory("Продукты", CategoryType.Expense);

    facade.CreateOperation(account.Id, salary.Id, OperationType.Income, 120000m, DateOnly.FromDateTime(DateTime.Today.AddDays(-10)), "Основной доход");
    facade.CreateOperation(account.Id, groceries.Id, OperationType.Expense, 4500m, DateOnly.FromDateTime(DateTime.Today.AddDays(-5)), "Супермаркет");
}

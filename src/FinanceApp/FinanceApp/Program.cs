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
    return new AnalyticsTimingDecorator(inner, _ => { });
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
    if (facade.GetCategories().Count > 0)
    {
        return;
    }

    facade.CreateCategory("Зарплата", CategoryType.Income);
    facade.CreateCategory("Ресторан", CategoryType.Expense);
}

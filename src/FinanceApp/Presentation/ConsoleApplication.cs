using System;
using System.Globalization;
using System.Text;
using FinanceApp.Application.Analytics;
using FinanceApp.Application.Facade;
using FinanceApp.Domain;

namespace FinanceApp.Presentation;

public class ConsoleApplication
{
    private readonly IFinanceFacade _facade;

    public ConsoleApplication(IFinanceFacade facade)
    {
        _facade = facade;
    }

    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== HSE Bank — Управление финансами ===");
            Console.WriteLine("1. Счета");
            Console.WriteLine("2. Категории");
            Console.WriteLine("3. Операции");
            Console.WriteLine("4. Аналитика");
            Console.WriteLine("5. Импорт");
            Console.WriteLine("6. Экспорт");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HandleAccounts();
                    break;
                case "2":
                    HandleCategories();
                    break;
                case "3":
                    HandleOperations();
                    break;
                case "4":
                    HandleAnalytics();
                    break;
                case "5":
                    HandleImport();
                    break;
                case "6":
                    HandleExport();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неизвестная команда");
                    break;
            }
        }
    }

    private void HandleAccounts()
    {
        Console.WriteLine("--- Счета ---");
        foreach (var account in _facade.GetAccounts())
        {
            Console.WriteLine($"{account.Id} | {account.Name} | {account.Currency} | Баланс: {account.Balance:0.##}");
        }

        Console.WriteLine("1. Создать счет");
        Console.WriteLine("2. Переименовать счет");
        Console.WriteLine("3. Удалить счет");
        Console.WriteLine("0. Назад");
        Console.Write("Выбор: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Название: ");
                var name = Console.ReadLine() ?? string.Empty;
                Console.Write("Валюта (например, RUB): ");
                var currency = Console.ReadLine() ?? "RUB";
                _facade.CreateAccount(name, currency);
                Console.WriteLine("Счет создан");
                break;
            case "2":
                Console.Write("ID счета: ");
                if (Guid.TryParse(Console.ReadLine(), out var accountId))
                {
                    Console.Write("Новое название: ");
                    var newName = Console.ReadLine() ?? string.Empty;
                    _facade.RenameAccount(accountId, newName);
                    Console.WriteLine("Счет переименован");
                }
                break;
            case "3":
                Console.Write("ID счета: ");
                if (Guid.TryParse(Console.ReadLine(), out var accountToDelete))
                {
                    _facade.DeleteAccount(accountToDelete);
                    Console.WriteLine("Счет удален");
                }
                break;
        }
    }

    private void HandleCategories()
    {
        Console.WriteLine("--- Категории ---");
        foreach (var category in _facade.GetCategories())
        {
            Console.WriteLine($"{category.Id} | {category.Name} | {category.Type}");
        }

        Console.WriteLine("1. Создать категорию");
        Console.WriteLine("2. Изменить категорию");
        Console.WriteLine("3. Удалить категорию");
        Console.WriteLine("0. Назад");
        Console.Write("Выбор: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Название: ");
                var name = Console.ReadLine() ?? string.Empty;
                Console.Write("Тип (Income/Expense/Universal): ");
                var typeString = Console.ReadLine() ?? "Universal";
                var type = Enum.Parse<CategoryType>(typeString, true);
                _facade.CreateCategory(name, type);
                Console.WriteLine("Категория создана");
                break;
            case "2":
                Console.Write("ID категории: ");
                if (Guid.TryParse(Console.ReadLine(), out var categoryId))
                {
                    Console.Write("Новое название: ");
                    var newName = Console.ReadLine() ?? string.Empty;
                    Console.Write("Тип (Income/Expense/Universal): ");
                    typeString = Console.ReadLine() ?? "Universal";
                    type = Enum.Parse<CategoryType>(typeString, true);
                    _facade.UpdateCategory(categoryId, newName, type);
                    Console.WriteLine("Категория обновлена");
                }
                break;
            case "3":
                Console.Write("ID категории: ");
                if (Guid.TryParse(Console.ReadLine(), out var categoryToDelete))
                {
                    _facade.DeleteCategory(categoryToDelete);
                    Console.WriteLine("Категория удалена");
                }
                break;
        }
    }

    private void HandleOperations()
    {
        Console.WriteLine("--- Операции ---");
        var accounts = _facade.GetAccounts();
        foreach (var account in accounts)
        {
            Console.WriteLine($"{account.Id} | {account.Name} | Баланс: {account.Balance:0.##}");
        }

        Console.Write("ID счета для работы: ");
        if (!Guid.TryParse(Console.ReadLine(), out var accountId))
        {
            return;
        }

        foreach (var operation in _facade.GetOperations(accountId))
        {
            Console.WriteLine($"{operation.Id} | {operation.Date:yyyy-MM-dd} | {operation.Type} | {operation.Amount:0.##} | {operation.Description}");
        }

        Console.WriteLine("1. Добавить операцию");
        Console.WriteLine("2. Удалить операцию");
        Console.WriteLine("3. Пересчитать баланс");
        Console.WriteLine("0. Назад");
        Console.Write("Выбор: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("ID категории: ");
                if (!Guid.TryParse(Console.ReadLine(), out var categoryId))
                {
                    return;
                }

                Console.Write("Тип (Income/Expense): ");
                var type = Enum.Parse<OperationType>(Console.ReadLine() ?? "Income", true);
                Console.Write("Сумма: ");
                var amount = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
                Console.Write("Дата (yyyy-MM-dd): ");
                var date = DateOnly.Parse(Console.ReadLine() ?? DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"), CultureInfo.InvariantCulture);
                Console.Write("Описание: ");
                var description = Console.ReadLine() ?? string.Empty;
                _facade.CreateOperation(accountId, categoryId, type, amount, date, description);
                Console.WriteLine("Операция добавлена");
                break;
            case "2":
                Console.Write("ID операции: ");
                if (Guid.TryParse(Console.ReadLine(), out var operationId))
                {
                    _facade.DeleteOperation(operationId);
                    Console.WriteLine("Операция удалена");
                }
                break;
            case "3":
                _facade.RecalculateBalance(accountId);
                Console.WriteLine("Баланс пересчитан");
                break;
        }
    }

    private void HandleAnalytics()
    {
        Console.Write("ID счета: ");
        if (!Guid.TryParse(Console.ReadLine(), out var accountId))
        {
            return;
        }

        Console.Write("Дата от (yyyy-MM-dd, пусто для всех): ");
        var fromInput = Console.ReadLine();
        Console.Write("Дата до (yyyy-MM-dd, пусто для всех): ");
        var toInput = Console.ReadLine();
        DateOnly? from = string.IsNullOrWhiteSpace(fromInput) ? null : DateOnly.Parse(fromInput, CultureInfo.InvariantCulture);
        DateOnly? to = string.IsNullOrWhiteSpace(toInput) ? null : DateOnly.Parse(toInput, CultureInfo.InvariantCulture);

        var balance = _facade.GetAccountBalance(accountId);
        Console.WriteLine($"Баланс: {balance.Balance:0.##}");

        var summary = _facade.GetIncomeExpense(accountId, from, to);
        Console.WriteLine($"Доходы: {summary.Income:0.##} | Расходы: {summary.Expense:0.##} | Разница: {summary.Difference:0.##}");

        Console.WriteLine("Группировка по категориям:");
        foreach (var total in _facade.GetCategoryTotals(accountId, from, to))
        {
            Console.WriteLine($"{total.CategoryName}: +{total.Income:0.##} / -{total.Expense:0.##} = {total.Net:0.##}");
        }
    }

    private void HandleImport()
    {
        Console.Write("Формат (csv/json/yaml): ");
        var format = Console.ReadLine() ?? "json";
        Console.WriteLine("Вставьте данные и оставьте пустую строку для завершения:");
        var builder = new StringBuilder();
        string? line;
        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            builder.AppendLine(line);
        }

        var snapshot = _facade.Import(format, builder.ToString(), apply: true);
        Console.WriteLine($"Импортировано {snapshot.Accounts.Count} счетов, {snapshot.Categories.Count} категорий, {snapshot.Operations.Count} операций");
    }

    private void HandleExport()
    {
        Console.Write("Формат (csv/json/yaml): ");
        var format = Console.ReadLine() ?? "json";
        var result = _facade.Export(format);
        Console.WriteLine("--- Экспорт ---");
        Console.WriteLine(result);
    }
}

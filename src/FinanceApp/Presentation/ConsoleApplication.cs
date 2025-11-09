using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
        var accounts = _facade.GetAccounts();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Счета отсутствуют.");
        }
        else
        {
            foreach (var account in accounts)
            {
                Console.WriteLine($"{account.Id} | {account.Name} | {account.Currency} | Баланс: {account.Balance:0.##}");
            }
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
                var name = ReadRequiredString("Название: ");
                Console.Write("Валюта (например, RUB): ");
                var currencyInput = Console.ReadLine();
                var currency = string.IsNullOrWhiteSpace(currencyInput)
                    ? "RUB"
                    : currencyInput.Trim().ToUpperInvariant();
                try
                {
                    _facade.CreateAccount(name, currency);
                    Console.WriteLine("Счет создан");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось создать счет: {ex.Message}");
                }
                break;
            case "2":
                if (!TryReadId("ID счета (пусто для отмены): ", out var accountId))
                {
                    break;
                }

                var newName = ReadRequiredString("Новое название: ");
                try
                {
                    _facade.RenameAccount(accountId, newName);
                    Console.WriteLine("Счет переименован");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось переименовать счет: {ex.Message}");
                }
                break;
            case "3":
                if (!TryReadId("ID счета (пусто для отмены): ", out var accountToDelete))
                {
                    break;
                }

                try
                {
                    _facade.DeleteAccount(accountToDelete);
                    Console.WriteLine("Счет удален");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось удалить счет: {ex.Message}");
                }
                break;
        }
    }

    private void HandleCategories()
    {
        Console.WriteLine("--- Категории ---");
        var categories = _facade.GetCategories();
        if (categories.Count == 0)
        {
            Console.WriteLine("Категории отсутствуют.");
        }
        else
        {
            foreach (var category in categories)
            {
                Console.WriteLine($"{category.Id} | {category.Name} | {TranslateCategoryType(category.Type)}");
            }
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
                var name = ReadRequiredString("Название: ");
                var type = ReadCategoryType("Тип (доход/расход): ");
                try
                {
                    _facade.CreateCategory(name, type);
                    Console.WriteLine("Категория создана");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось создать категорию: {ex.Message}");
                }
                break;
            case "2":
                if (!TryReadId("ID категории (пусто для отмены): ", out var categoryId))
                {
                    break;
                }

                var newName = ReadRequiredString("Новое название: ");
                var newType = ReadCategoryType("Тип (доход/расход): ");
                try
                {
                    _facade.UpdateCategory(categoryId, newName, newType);
                    Console.WriteLine("Категория обновлена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось обновить категорию: {ex.Message}");
                }
                break;
            case "3":
                if (!TryReadId("ID категории (пусто для отмены): ", out var categoryToDelete))
                {
                    break;
                }

                try
                {
                    _facade.DeleteCategory(categoryToDelete);
                    Console.WriteLine("Категория удалена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось удалить категорию: {ex.Message}");
                }
                break;
        }
    }

    private void HandleOperations()
    {
        Console.WriteLine("--- Операции ---");
        var accounts = _facade.GetAccounts();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Нет счетов. Сначала создайте счет.");
            return;
        }

        foreach (var account in accounts)
        {
            Console.WriteLine($"{account.Id} | {account.Name} | Баланс: {account.Balance:0.##}");
        }

        if (!TryReadId("ID счета для работы (пусто для отмены): ", out var accountId))
        {
            return;
        }

        var accountToWork = accounts.FirstOrDefault(a => a.Id == accountId);
        if (accountToWork is null)
        {
            Console.WriteLine("Счет не найден.");
            return;
        }

        var operations = _facade.GetOperations(accountId);
        if (operations.Count == 0)
        {
            Console.WriteLine("Операции отсутствуют.");
        }
        else
        {
            foreach (var operation in operations)
            {
                var sign = operation.Type == OperationType.Income ? "+" : "-";
                var description = string.IsNullOrWhiteSpace(operation.Description) ? "(без описания)" : operation.Description;
                var typeName = TranslateOperationType(operation.Type);
                Console.WriteLine($"{operation.Id} | {operation.Date:dd-MM-yyyy} | {typeName} | {sign}{operation.Amount:0.##} | {description}");
            }
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
                var categories = _facade.GetCategories();
                if (categories.Count == 0)
                {
                    Console.WriteLine("Категории отсутствуют. Создайте категорию перед добавлением операций.");
                    break;
                }

                Console.WriteLine("Доступные категории:");
                foreach (var category in categories)
                {
                    Console.WriteLine($"{category.Id} | {category.Name} | {TranslateCategoryType(category.Type)}");
                }

                if (!TryReadId("ID категории (пусто для отмены): ", out var categoryId))
                {
                    break;
                }

                var category = categories.FirstOrDefault(c => c.Id == categoryId);
                if (category is null)
                {
                    Console.WriteLine("Категория не найдена.");
                    break;
                }

                var type = category.Type == CategoryType.Income
                    ? OperationType.Income
                    : OperationType.Expense;
                var amount = ReadAmount("Сумма: ");
                var date = ReadDate("Дата (dd-MM-yyyy, пусто для сегодняшней даты): ");
                Console.Write("Описание: ");
                var description = Console.ReadLine() ?? string.Empty;

                try
                {
                    _facade.CreateOperation(accountId, categoryId, type, amount, date, description);
                    Console.WriteLine("Операция добавлена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось добавить операцию: {ex.Message}");
                }
                break;
            case "2":
                if (!TryReadId("ID операции (пусто для отмены): ", out var operationId))
                {
                    break;
                }

                try
                {
                    _facade.DeleteOperation(operationId);
                    Console.WriteLine("Операция удалена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось удалить операцию: {ex.Message}");
                }
                break;
            case "3":
                try
                {
                    _facade.RecalculateBalance(accountId);
                    Console.WriteLine("Баланс пересчитан");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось пересчитать баланс: {ex.Message}");
                }
                break;
        }
    }

    private void HandleAnalytics()
    {
        Console.WriteLine("--- Аналитика ---");
        var accounts = _facade.GetAccounts();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Нет счетов для анализа.");
            return;
        }

        foreach (var account in accounts)
        {
            Console.WriteLine($"{account.Id} | {account.Name} | Баланс: {account.Balance:0.##}");
        }

        if (!TryReadId("ID счета (пусто для отмены): ", out var accountId))
        {
            return;
        }

        var account = accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            Console.WriteLine("Счет не найден.");
            return;
        }

        var from = ReadOptionalDate("Дата от (dd-MM-yyyy, пусто для всех): ");
        var to = ReadOptionalDate("Дата до (dd-MM-yyyy, пусто для всех): ");

        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось получить аналитику: {ex.Message}");
        }
    }

    private void HandleImport()
    {
        while (true)
        {
            Console.Write("Путь к файлу для импорта (пусто для отмены): ");
            var path = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Импорт отменен.");
                return;
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден. Попробуйте еще раз.");
                continue;
            }

            if (!TryGetFormatFromFileName(path, out _))
            {
                Console.WriteLine("Поддерживаются файлы с расширениями .csv, .json, .yaml и .yml.");
                continue;
            }

            try
            {
                var snapshot = _facade.ImportFromFile(path, apply: true);
                Console.WriteLine($"Импортировано {snapshot.Accounts.Count} счетов, {snapshot.Categories.Count} категорий, {snapshot.Operations.Count} операций.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось импортировать данные: {ex.Message}");
            }
        }
    }

    private void HandleExport()
    {
        while (true)
        {
            Console.Write("Путь к папке для сохранения (пусто для отмены): ");
            var folder = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(folder))
            {
                Console.WriteLine("Экспорт отменен.");
                return;
            }

            Console.Write("Формат экспорта (csv/json/yaml): ");
            var formatInput = Console.ReadLine();
            if (!TryParseExportFormat(formatInput, out var format))
            {
                Console.WriteLine("Некорректный формат. Используйте значения csv, json или yaml.");
                continue;
            }

            try
            {
                var filePath = _facade.ExportData(folder, format);
                Console.WriteLine($"Экспорт завершен. Файл сохранен по пути: {filePath}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось выполнить экспорт: {ex.Message}");
            }
        }
    }

    private static bool TryReadId(string prompt, out int id)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                id = default;
                return false;
            }

            if (int.TryParse(input, out id) && id >= 0)
            {
                return true;
            }

            Console.WriteLine("Некорректный ввод. Попробуйте еще раз.");
        }
    }

    private static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = (Console.ReadLine() ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            Console.WriteLine("Значение не может быть пустым. Попробуйте еще раз.");
        }
    }

    private static CategoryType ReadCategoryType(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (TryParseCategoryType(input, out var type))
            {
                return type;
            }

            Console.WriteLine("Некорректный тип категории. Введите \"доход\" или \"расход\".");
        }
    }

    private static bool TryParseCategoryType(string? input, out CategoryType type)
    {
        var normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized) || normalized is "income" or "доход")
        {
            type = CategoryType.Income;
            return true;
        }

        if (normalized is "expense" or "расход")
        {
            type = CategoryType.Expense;
            return true;
        }

        type = default;
        return false;
    }

    private static decimal ReadAmount(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && amount > 0)
            {
                return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            }

            Console.WriteLine("Некорректная сумма. Попробуйте еще раз.");
        }
    }

    private static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return DateOnly.FromDateTime(DateTime.Today);
            }

            if (DateOnly.TryParseExact(input, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            Console.WriteLine("Некорректная дата. Используйте формат dd-MM-yyyy.");
        }
    }

    private static DateOnly? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (DateOnly.TryParseExact(input, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            Console.WriteLine("Некорректная дата. Используйте формат dd-MM-yyyy.");
        }
    }

    private static bool TryGetFormatFromFileName(string path, out string format)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extension))
        {
            format = string.Empty;
            return false;
        }

        return TryParseExportFormat(extension.TrimStart('.'), out format);
    }

    private static bool TryParseExportFormat(string? input, out string format)
    {
        var normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "csv":
                format = "csv";
                return true;
            case "json":
            case "jsony":
                format = "json";
                return true;
            case "yaml":
            case "yml":
                format = "yaml";
                return true;
            default:
                format = string.Empty;
                return false;
        }
    }

    private static string TranslateCategoryType(CategoryType type)
        => type == CategoryType.Income ? "Доход" : "Расход";

    private static string TranslateOperationType(OperationType type)
        => type == OperationType.Income ? "Доход" : "Расход";
}

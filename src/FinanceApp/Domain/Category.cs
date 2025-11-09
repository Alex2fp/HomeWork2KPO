namespace FinanceApp.Domain;

public class Category : IExportable
{
    public Category(Guid id, string name, CategoryType type)
    {
        Id = id;
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Category name cannot be empty", nameof(name));
        Type = type;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public CategoryType Type { get; private set; }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Category name cannot be empty", nameof(newName));
        }

        Name = newName.Trim();
    }

    public void ChangeType(CategoryType type)
    {
        Type = type;
    }

    public void Accept(IFinanceDataExportVisitor visitor)
    {
        visitor.VisitCategory(this);
    }
}

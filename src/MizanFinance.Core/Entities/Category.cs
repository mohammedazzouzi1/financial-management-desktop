using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class Category : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; } = CategoryType.Expense;
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public bool IsSystem { get; set; }
    public string? ColorHex { get; set; }
    public string? Icon { get; set; }
}

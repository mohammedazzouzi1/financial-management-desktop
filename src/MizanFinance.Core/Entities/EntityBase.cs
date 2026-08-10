namespace MizanFinance.Core.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
}

using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class AuditLog : EntityBase
{
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

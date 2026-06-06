using backend.src.Entities.Base;

namespace backend.src.Entities;

public class AuditLog : Entity
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public User? PerformedByUser { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}

using backend.src.Data;
using backend.src.Entities;

namespace backend.src.Repositories;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }
}

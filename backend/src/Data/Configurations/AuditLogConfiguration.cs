using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.OldValues)
            .HasColumnType("text");

        builder.Property(al => al.NewValues)
            .HasColumnType("text");

        builder.HasOne(al => al.PerformedByUser)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

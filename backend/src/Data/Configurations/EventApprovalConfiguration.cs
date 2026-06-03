using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class EventApprovalConfiguration : IEntityTypeConfiguration<EventApproval>
{
    public void Configure(EntityTypeBuilder<EventApproval> builder)
    {
        builder.HasKey(ea => ea.Id);

        builder.HasIndex(ea => new { ea.EventId, ea.UserId });

        builder.HasOne(ea => ea.Event)
            .WithMany(e => e.Approvals)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ea => ea.User)
            .WithMany(u => u.EventApprovals)
            .HasForeignKey(ea => ea.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ea => ea.VoteType)
            .IsRequired();
    }
}

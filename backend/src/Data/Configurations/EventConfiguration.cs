using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Points)
            .IsRequired();

        builder.Property(e => e.Type)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.HasIndex(e => e.GroupId);
        builder.HasIndex(e => e.AffectedUserId);
        builder.HasIndex(e => e.CreatedByUserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.IsPendingRemoval);

        builder.Property(e => e.RemovalVoteDeadline)
            .IsRequired(false);

        builder.HasOne(e => e.Group)
            .WithMany(g => g.Events)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany(u => u.CreatedEvents)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AffectedUser)
            .WithMany(u => u.AffectedEvents)
            .HasForeignKey(e => e.AffectedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

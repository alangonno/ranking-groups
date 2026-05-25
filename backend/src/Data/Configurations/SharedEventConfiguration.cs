using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class SharedEventConfiguration : IEntityTypeConfiguration<SharedEvent>
{
    public void Configure(EntityTypeBuilder<SharedEvent> builder)
    {
        builder.HasKey(se => se.Id);

        builder.Property(se => se.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(se => se.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(se => se.Points)
            .IsRequired();

        builder.Property(se => se.ClosesAt);

        builder.HasIndex(se => se.GroupId);
        builder.HasIndex(se => se.CreatedByUserId);
        builder.HasIndex(se => se.CreatedAt);
        builder.HasIndex(se => se.IsClosed);

        builder.HasOne(se => se.Group)
            .WithMany(g => g.SharedEvents)
            .HasForeignKey(se => se.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(se => se.CreatedByUser)
            .WithMany(u => u.CreatedSharedEvents)
            .HasForeignKey(se => se.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

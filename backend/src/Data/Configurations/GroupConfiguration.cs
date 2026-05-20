using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.Property(g => g.InviteCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(g => g.InviteCode)
            .IsUnique();

        builder.HasOne(g => g.CreatedByUser)
            .WithMany(u => u.OwnedGroups)
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

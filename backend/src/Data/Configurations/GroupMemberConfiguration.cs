using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasKey(gm => gm.Id);

        builder.HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();

        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.User)
            .WithMany(u => u.GroupMembers)
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(gm => gm.Role)
            .IsRequired();

        builder.Property(gm => gm.CurrentScore)
            .HasDefaultValue(0);
    }
}

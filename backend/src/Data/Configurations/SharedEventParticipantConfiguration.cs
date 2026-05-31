using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class SharedEventParticipantConfiguration : IEntityTypeConfiguration<SharedEventParticipant>
{
    public void Configure(EntityTypeBuilder<SharedEventParticipant> builder)
    {
        builder.HasKey(sep => sep.Id);

        builder.HasIndex(sep => new { sep.SharedEventId, sep.UserId })
            .IsUnique();

        builder.HasOne(sep => sep.SharedEvent)
            .WithMany(se => se.Participants)
            .HasForeignKey(sep => sep.SharedEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sep => sep.User)
            .WithMany(u => u.SharedEventParticipations)
            .HasForeignKey(sep => sep.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sep => sep.IsPendingRemoval);
    }
}

using backend.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.src.Data.Configurations;

public class SharedEventParticipantRemovalVoteConfiguration : IEntityTypeConfiguration<SharedEventParticipantRemovalVote>
{
    public void Configure(EntityTypeBuilder<SharedEventParticipantRemovalVote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.SharedEventId, v.ParticipantId, v.UserId })
            .IsUnique();

        builder.HasOne(v => v.SharedEvent)
            .WithMany()
            .HasForeignKey(v => v.SharedEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Participant)
            .WithMany()
            .HasForeignKey(v => v.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.VoteType)
            .IsRequired();
    }
}

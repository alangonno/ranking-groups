using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedEventParticipantRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_pending_removal",
                table: "shared_event_participants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "removal_vote_deadline",
                table: "shared_event_participants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "shared_event_participant_removal_votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_event_participant_removal_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_shared_event_participant_removal_votes_shared_event_partici~",
                        column: x => x.participant_id,
                        principalTable: "shared_event_participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shared_event_participant_removal_votes_shared_events_shared~",
                        column: x => x.shared_event_id,
                        principalTable: "shared_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shared_event_participant_removal_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shared_event_participants_is_pending_removal",
                table: "shared_event_participants",
                column: "is_pending_removal");

            migrationBuilder.CreateIndex(
                name: "IX_shared_event_participant_removal_votes_participant_id",
                table: "shared_event_participant_removal_votes",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_shared_event_participant_removal_votes_shared_event_id_part~",
                table: "shared_event_participant_removal_votes",
                columns: new[] { "shared_event_id", "participant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shared_event_participant_removal_votes_user_id",
                table: "shared_event_participant_removal_votes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_event_participant_removal_votes");

            migrationBuilder.DropIndex(
                name: "IX_shared_event_participants_is_pending_removal",
                table: "shared_event_participants");

            migrationBuilder.DropColumn(
                name: "is_pending_removal",
                table: "shared_event_participants");

            migrationBuilder.DropColumn(
                name: "removal_vote_deadline",
                table: "shared_event_participants");
        }
    }
}

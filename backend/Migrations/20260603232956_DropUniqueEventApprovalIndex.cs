using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class DropUniqueEventApprovalIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_approvals_event_id_user_id",
                table: "event_approvals");

            migrationBuilder.CreateIndex(
                name: "IX_event_approvals_event_id_user_id",
                table: "event_approvals",
                columns: new[] { "event_id", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_approvals_event_id_user_id",
                table: "event_approvals");

            migrationBuilder.CreateIndex(
                name: "IX_event_approvals_event_id_user_id",
                table: "event_approvals",
                columns: new[] { "event_id", "user_id" },
                unique: true);
        }
    }
}

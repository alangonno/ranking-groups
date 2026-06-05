using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationGroupAndEventRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_read",
                table: "notifications");

            migrationBuilder.AddColumn<string>(
                name: "action",
                table: "notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "event_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "group_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "shared_event_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_event_id",
                table: "notifications",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_group_id",
                table: "notifications",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_shared_event_id",
                table: "notifications",
                column: "shared_event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_event_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_group_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_shared_event_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "action",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "event_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "shared_event_id",
                table: "notifications");

            migrationBuilder.AddColumn<bool>(
                name: "is_read",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleOAuthToAdvisorAndGoogleSpaceToMeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "google_event_id",
                table: "meeting",
                newName: "google_space_name");

            migrationBuilder.AddColumn<string>(
                name: "google_access_token",
                table: "advisor",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_email",
                table: "advisor",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_refresh_token",
                table: "advisor",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "google_token_expiry_utc",
                table: "advisor",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "google_access_token",
                table: "advisor");

            migrationBuilder.DropColumn(
                name: "google_email",
                table: "advisor");

            migrationBuilder.DropColumn(
                name: "google_refresh_token",
                table: "advisor");

            migrationBuilder.DropColumn(
                name: "google_token_expiry_utc",
                table: "advisor");

            migrationBuilder.RenameColumn(
                name: "google_space_name",
                table: "meeting",
                newName: "google_event_id");
        }
    }
}

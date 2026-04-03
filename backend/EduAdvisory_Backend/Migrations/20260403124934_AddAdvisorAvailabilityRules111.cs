using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisorAvailabilityRules111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_meeting_request_advisor_availability_availability_id",
                table: "meeting_request");

            migrationBuilder.DropIndex(
                name: "IX_meeting_request_availability_id",
                table: "meeting_request");

            migrationBuilder.DropColumn(
                name: "availability_id",
                table: "meeting_request");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "end_at",
                table: "meeting_request",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "start_at",
                table: "meeting_request",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_at",
                table: "meeting_request");

            migrationBuilder.DropColumn(
                name: "start_at",
                table: "meeting_request");

            migrationBuilder.AddColumn<int>(
                name: "availability_id",
                table: "meeting_request",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_meeting_request_availability_id",
                table: "meeting_request",
                column: "availability_id");

            migrationBuilder.AddForeignKey(
                name: "FK_meeting_request_advisor_availability_availability_id",
                table: "meeting_request",
                column: "availability_id",
                principalTable: "advisor_availability",
                principalColumn: "availability_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

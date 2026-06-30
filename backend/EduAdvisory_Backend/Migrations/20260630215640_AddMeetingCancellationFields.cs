using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingCancellationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "meeting",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancelled_by",
                table: "meeting",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "cancelled_by",
                table: "meeting");
        }
    }
}

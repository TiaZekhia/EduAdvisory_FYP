using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisorAvailabilityRules2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slot_duration_minutes",
                table: "advisor_availability_rule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "slot_duration_minutes",
                table: "advisor_availability_rule",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

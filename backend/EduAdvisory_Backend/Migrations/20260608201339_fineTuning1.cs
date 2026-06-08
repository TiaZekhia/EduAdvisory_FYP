using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class fineTuning1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "advisor_availability_exception_unique",
                table: "advisor_availability_exception");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "end_time",
                table: "advisor_availability_exception",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "start_time",
                table: "advisor_availability_exception",
                type: "interval",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "advisor_availability_exception_advisor_date_idx",
                table: "advisor_availability_exception",
                columns: new[] { "advisor_id", "exception_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "advisor_availability_exception_advisor_date_idx",
                table: "advisor_availability_exception");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "advisor_availability_exception");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "advisor_availability_exception");

            migrationBuilder.CreateIndex(
                name: "advisor_availability_exception_unique",
                table: "advisor_availability_exception",
                columns: new[] { "advisor_id", "exception_date" },
                unique: true);
        }
    }
}

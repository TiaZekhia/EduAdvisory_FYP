using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class fineTuning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advisor_availability_exception",
                columns: table => new
                {
                    exception_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: false),
                    exception_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("advisor_availability_exception_pkey", x => x.exception_id);
                    table.ForeignKey(
                        name: "advisor_availability_exception_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "advisor_availability_exception_unique",
                table: "advisor_availability_exception",
                columns: new[] { "advisor_id", "exception_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advisor_availability_exception");
        }
    }
}

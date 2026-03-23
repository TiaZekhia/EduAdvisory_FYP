using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingRequestAndAvailabilityFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "meeting",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                table: "meeting",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "end_at",
                table: "meeting",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "meeting_link",
                table: "meeting",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "request_id",
                table: "meeting",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "start_at",
                table: "meeting",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "meeting",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "meeting",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "meeting",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "advisor_availability",
                columns: table => new
                {
                    availability_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_booked = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advisor_availability", x => x.availability_id);
                    table.ForeignKey(
                        name: "FK_advisor_availability_advisor_advisor_id",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting_request",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    advisor_id = table.Column<int>(type: "integer", nullable: false),
                    availability_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_request", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_meeting_request_advisor_advisor_id",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meeting_request_advisor_availability_availability_id",
                        column: x => x.availability_id,
                        principalTable: "advisor_availability",
                        principalColumn: "availability_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meeting_request_sis_student_student_id",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advisor_availability_advisor_id",
                table: "advisor_availability",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_request_advisor_id",
                table: "meeting_request",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_request_availability_id",
                table: "meeting_request",
                column: "availability_id");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_request_student_id",
                table: "meeting_request",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meeting_request");

            migrationBuilder.DropTable(
                name: "advisor_availability");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "end_at",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "meeting_link",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "request_id",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "start_at",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "status",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "title",
                table: "meeting");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "meeting");
        }
    }
}

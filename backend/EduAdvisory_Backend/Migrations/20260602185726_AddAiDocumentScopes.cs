using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAiDocumentScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "course_code",
                table: "ai_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "academic_year",
                table: "ai_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "program_code",
                table: "ai_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "ai_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "course_code",
                table: "ai_document_chunks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "academic_year",
                table: "ai_document_chunks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "program_code",
                table: "ai_document_chunks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "ai_document_chunks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "academic_year",
                table: "ai_documents");

            migrationBuilder.DropColumn(
                name: "program_code",
                table: "ai_documents");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "ai_documents");

            migrationBuilder.DropColumn(
                name: "academic_year",
                table: "ai_document_chunks");

            migrationBuilder.DropColumn(
                name: "program_code",
                table: "ai_document_chunks");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "ai_document_chunks");

            migrationBuilder.AlterColumn<string>(
                name: "course_code",
                table: "ai_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "course_code",
                table: "ai_document_chunks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}

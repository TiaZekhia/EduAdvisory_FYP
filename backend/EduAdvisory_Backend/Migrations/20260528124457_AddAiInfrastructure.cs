using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAiInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "ai_chat_sessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_chat_sessions_pkey", x => x.session_id);
                });

            migrationBuilder.CreateTable(
                name: "ai_documents",
                columns: table => new
                {
                    document_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    course_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    semester = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    uploaded_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_documents_pkey", x => x.document_id);
                });

            migrationBuilder.CreateTable(
                name: "ai_retrieval_logs",
                columns: table => new
                {
                    retrieval_log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    session_id = table.Column<int>(type: "integer", nullable: true),
                    question = table.Column<string>(type: "text", nullable: false),
                    retrieved_chunk_ids = table.Column<string>(type: "text", nullable: true),
                    top_similarity_score = table.Column<double>(type: "double precision", nullable: true),
                    plugin_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    response_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_retrieval_logs_pkey", x => x.retrieval_log_id);
                });

            migrationBuilder.CreateTable(
                name: "ai_chat_messages",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_chat_messages_pkey", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_ai_chat_messages_ai_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ai_chat_sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_document_chunks",
                columns: table => new
                {
                    chunk_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<int>(type: "integer", nullable: false),
                    course_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    section_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    chunk_text = table.Column<string>(type: "text", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: true),
                    token_count = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_document_chunks_pkey", x => x.chunk_id);
                    table.ForeignKey(
                        name: "FK_ai_document_chunks_ai_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "ai_documents",
                        principalColumn: "document_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_ai_chat_messages_session_id",
                table: "ai_chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_ai_chat_sessions_student_id",
                table: "ai_chat_sessions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "idx_ai_chunks_course_code",
                table: "ai_document_chunks",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "idx_ai_chunks_document_type",
                table: "ai_document_chunks",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "IX_ai_document_chunks_document_id",
                table: "ai_document_chunks",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "idx_ai_documents_course_type",
                table: "ai_documents",
                columns: new[] { "course_code", "document_type" });

            migrationBuilder.CreateIndex(
                name: "idx_ai_retrieval_logs_student_id",
                table: "ai_retrieval_logs",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_chat_messages");

            migrationBuilder.DropTable(
                name: "ai_document_chunks");

            migrationBuilder.DropTable(
                name: "ai_retrieval_logs");

            migrationBuilder.DropTable(
                name: "ai_chat_sessions");

            migrationBuilder.DropTable(
                name: "ai_documents");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}

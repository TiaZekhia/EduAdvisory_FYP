using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broadcast_message",
                columns: table => new
                {
                    broadcast_message_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("broadcast_message_pkey", x => x.broadcast_message_id);
                    table.ForeignKey(
                        name: "broadcast_message_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation",
                columns: table => new
                {
                    conversation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("conversation_pkey", x => x.conversation_id);
                    table.ForeignKey(
                        name: "conversation_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "conversation_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "broadcast_recipient",
                columns: table => new
                {
                    broadcast_recipient_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    broadcast_message_id = table.Column<int>(type: "integer", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("broadcast_recipient_pkey", x => x.broadcast_recipient_id);
                    table.ForeignKey(
                        name: "broadcast_recipient_broadcast_message_id_fkey",
                        column: x => x.broadcast_message_id,
                        principalTable: "broadcast_message",
                        principalColumn: "broadcast_message_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "broadcast_recipient_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_message",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<int>(type: "integer", nullable: false),
                    sender_user_id = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("chat_message_pkey", x => x.message_id);
                    table.ForeignKey(
                        name: "chat_message_conversation_id_fkey",
                        column: x => x.conversation_id,
                        principalTable: "conversation",
                        principalColumn: "conversation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "chat_message_sender_user_id_fkey",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_broadcast_message_advisor_id",
                table: "broadcast_message",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "broadcast_recipient_message_student_unique",
                table: "broadcast_recipient",
                columns: new[] { "broadcast_message_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_broadcast_recipient_student_id",
                table: "broadcast_recipient",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_conversation_id",
                table: "chat_message",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_sender_user_id",
                table: "chat_message",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "conversation_advisor_student_unique",
                table: "conversation",
                columns: new[] { "advisor_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversation_student_id",
                table: "conversation",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broadcast_recipient");

            migrationBuilder.DropTable(
                name: "chat_message");

            migrationBuilder.DropTable(
                name: "broadcast_message");

            migrationBuilder.DropTable(
                name: "conversation");
        }
    }
}

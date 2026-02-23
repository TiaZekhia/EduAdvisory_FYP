using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduAdvisory_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advisor",
                columns: table => new
                {
                    advisor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("advisor_pkey", x => x.advisor_id);
                });

            migrationBuilder.CreateTable(
                name: "sis_course",
                columns: table => new
                {
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    course_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_course_pkey", x => x.course_code);
                });

            migrationBuilder.CreateTable(
                name: "announcement",
                columns: table => new
                {
                    announcement_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("announcement_pkey", x => x.announcement_id);
                    table.ForeignKey(
                        name: "announcement_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id");
                });

            migrationBuilder.CreateTable(
                name: "sis_student",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    program_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    current_semester = table.Column<int>(type: "integer", nullable: true),
                    current_gpa = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    academic_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    advisor_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_student_pkey", x => x.student_id);
                    table.ForeignKey(
                        name: "sis_student_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id");
                });

            migrationBuilder.CreateTable(
                name: "course_grading_schema",
                columns: table => new
                {
                    grading_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    component_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    weight_percentage = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("course_grading_schema_pkey", x => x.grading_id);
                    table.ForeignKey(
                        name: "course_grading_schema_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                });

            migrationBuilder.CreateTable(
                name: "course_prerequisite",
                columns: table => new
                {
                    course_code = table.Column<string>(type: "character varying(20)", nullable: false),
                    prerequisite_course_code = table.Column<string>(type: "character varying(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_prerequisite", x => new { x.course_code, x.prerequisite_course_code });
                    table.ForeignKey(
                        name: "FK_course_prerequisite_sis_course_course_code",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_prerequisite_sis_course_prerequisite_course_code",
                        column: x => x.prerequisite_course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_guide",
                columns: table => new
                {
                    study_guide_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    program_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    recommended_semester = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("study_guide_pkey", x => x.study_guide_id);
                    table.ForeignKey(
                        name: "study_guide_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                });

            migrationBuilder.CreateTable(
                name: "chatbot_history",
                columns: table => new
                {
                    chatbot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: true),
                    question = table.Column<string>(type: "text", nullable: true),
                    answer = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("chatbot_history_pkey", x => x.chatbot_id);
                    table.ForeignKey(
                        name: "chatbot_history_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "generated_study_plan",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: true),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    planned_semester = table.Column<int>(type: "integer", nullable: true),
                    generation_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("generated_study_plan_pkey", x => x.plan_id);
                    table.ForeignKey(
                        name: "generated_study_plan_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "generated_study_plan_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "meeting",
                columns: table => new
                {
                    meeting_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advisor_id = table.Column<int>(type: "integer", nullable: true),
                    student_id = table.Column<int>(type: "integer", nullable: true),
                    meeting_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    meeting_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("meeting_pkey", x => x.meeting_id);
                    table.ForeignKey(
                        name: "meeting_advisor_id_fkey",
                        column: x => x.advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id");
                    table.ForeignKey(
                        name: "meeting_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "sis_course_assessment",
                columns: table => new
                {
                    assessment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    course_credits = table.Column<int>(type: "integer", nullable: true),
                    absences_count = table.Column<int>(type: "integer", nullable: true),
                    max_absences = table.Column<int>(type: "integer", nullable: true),
                    semester_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    semester_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_course_assessment_pkey", x => x.assessment_id);
                    table.ForeignKey(
                        name: "sis_course_assessment_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "sis_course_assessment_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "sis_current_enrollment",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    semester = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_current_enrollment_pkey", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "sis_current_enrollment_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "sis_current_enrollment_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "sis_student_course_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    semester = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    final_grade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_student_course_history_pkey", x => x.history_id);
                    table.ForeignKey(
                        name: "sis_student_course_history_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "sis_student_course_history_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "sis_student_grades",
                columns: table => new
                {
                    grade_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    component_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    grade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sis_student_grades_pkey", x => x.grade_id);
                    table.ForeignKey(
                        name: "sis_student_grades_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "sis_student_grades_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "student_risk",
                columns: table => new
                {
                    risk_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<int>(type: "integer", nullable: true),
                    course_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    risk_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    calculated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("student_risk_pkey", x => x.risk_id);
                    table.ForeignKey(
                        name: "student_risk_course_code_fkey",
                        column: x => x.course_code,
                        principalTable: "sis_course",
                        principalColumn: "course_code");
                    table.ForeignKey(
                        name: "student_risk_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    keycloak_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    linked_student_id = table.Column<int>(type: "integer", nullable: true),
                    linked_advisor_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.user_id);
                    table.ForeignKey(
                        name: "users_linked_advisor_id_fkey",
                        column: x => x.linked_advisor_id,
                        principalTable: "advisor",
                        principalColumn: "advisor_id");
                    table.ForeignKey(
                        name: "users_linked_student_id_fkey",
                        column: x => x.linked_student_id,
                        principalTable: "sis_student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateIndex(
                name: "advisor_email_key",
                table: "advisor",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcement_advisor_id",
                table: "announcement",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_chatbot_history_student_id",
                table: "chatbot_history",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_grading_schema_course_code",
                table: "course_grading_schema",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_course_prerequisite_prerequisite_course_code",
                table: "course_prerequisite",
                column: "prerequisite_course_code");

            migrationBuilder.CreateIndex(
                name: "IX_generated_study_plan_course_code",
                table: "generated_study_plan",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_generated_study_plan_student_id",
                table: "generated_study_plan",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_advisor_id",
                table: "meeting",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_student_id",
                table: "meeting",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_sis_course_assessment_course_code",
                table: "sis_course_assessment",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_sis_course_assessment_student_id",
                table: "sis_course_assessment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_sis_current_enrollment_course_code",
                table: "sis_current_enrollment",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_sis_current_enrollment_student_id",
                table: "sis_current_enrollment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_sis_student_advisor_id",
                table: "sis_student",
                column: "advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_sis_student_course_history_course_code",
                table: "sis_student_course_history",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_sis_student_course_history_student_id",
                table: "sis_student_course_history",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_sis_student_grades_course_code",
                table: "sis_student_grades",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_sis_student_grades_student_id",
                table: "sis_student_grades",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_risk_course_code",
                table: "student_risk",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_student_risk_student_id",
                table: "student_risk",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_guide_course_code",
                table: "study_guide",
                column: "course_code");

            migrationBuilder.CreateIndex(
                name: "IX_users_linked_advisor_id",
                table: "users",
                column: "linked_advisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_linked_student_id",
                table: "users",
                column: "linked_student_id");

            migrationBuilder.CreateIndex(
                name: "users_username_key",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcement");

            migrationBuilder.DropTable(
                name: "chatbot_history");

            migrationBuilder.DropTable(
                name: "course_grading_schema");

            migrationBuilder.DropTable(
                name: "course_prerequisite");

            migrationBuilder.DropTable(
                name: "generated_study_plan");

            migrationBuilder.DropTable(
                name: "meeting");

            migrationBuilder.DropTable(
                name: "sis_course_assessment");

            migrationBuilder.DropTable(
                name: "sis_current_enrollment");

            migrationBuilder.DropTable(
                name: "sis_student_course_history");

            migrationBuilder.DropTable(
                name: "sis_student_grades");

            migrationBuilder.DropTable(
                name: "student_risk");

            migrationBuilder.DropTable(
                name: "study_guide");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "sis_course");

            migrationBuilder.DropTable(
                name: "sis_student");

            migrationBuilder.DropTable(
                name: "advisor");
        }
    }
}

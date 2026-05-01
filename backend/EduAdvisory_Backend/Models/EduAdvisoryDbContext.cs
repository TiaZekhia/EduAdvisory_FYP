using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

public partial class EduAdvisoryDbContext : DbContext
{
    public EduAdvisoryDbContext()
    {
    }

    public EduAdvisoryDbContext(DbContextOptions<EduAdvisoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advisor> Advisors { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<ChatbotHistory> ChatbotHistories { get; set; }

    public virtual DbSet<CourseGradingSchema> CourseGradingSchemas { get; set; }

    public virtual DbSet<GeneratedStudyPlan> GeneratedStudyPlans { get; set; }

    public virtual DbSet<Meeting> Meetings { get; set; }

    public virtual DbSet<SisCourse> SisCourses { get; set; }

    public virtual DbSet<SisCourseAssessment> SisCourseAssessments { get; set; }

    public virtual DbSet<SisCurrentEnrollment> SisCurrentEnrollments { get; set; }

    public virtual DbSet<SisStudent> SisStudents { get; set; }

    public virtual DbSet<SisStudentCourseHistory> SisStudentCourseHistories { get; set; }

    public virtual DbSet<SisStudentGrade> SisStudentGrades { get; set; }

    public virtual DbSet<StudentRisk> StudentRisks { get; set; }

    public virtual DbSet<StudyGuide> StudyGuides { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }

    public DbSet<MeetingRequest> MeetingRequests { get; set; }

    public DbSet<AdvisorAvailability> AdvisorAvailabilities { get; set; }

    public DbSet<AppGoogleAccount> AppGoogleAccounts { get; set; }

    public DbSet<AdvisorAvailabilityRule> AdvisorAvailabilityRules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=eduadvisory_db;Username=postgres;Password=@#$TIze06@#$");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoursePrerequisite>()
        .HasKey(cp => new { cp.CourseCode, cp.PrerequisiteCourseCode });

        modelBuilder.Entity<CoursePrerequisite>()
            .HasOne(cp => cp.Course)
            .WithMany(c => c.Prerequisites)
            .HasForeignKey(cp => cp.CourseCode)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoursePrerequisite>()
            .HasOne(cp => cp.PrerequisiteCourse)
            .WithMany(c => c.RequiredBy)
            .HasForeignKey(cp => cp.PrerequisiteCourseCode)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Advisor>(entity =>
        {
            entity.HasKey(e => e.AdvisorId).HasName("advisor_pkey");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("announcement_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Advisor).WithMany(p => p.Announcements).HasConstraintName("announcement_advisor_id_fkey");
        });

        modelBuilder.Entity<ChatbotHistory>(entity =>
        {
            entity.HasKey(e => e.ChatbotId).HasName("chatbot_history_pkey");

            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Student).WithMany(p => p.ChatbotHistories).HasConstraintName("chatbot_history_student_id_fkey");
        });

        modelBuilder.Entity<CourseGradingSchema>(entity =>
        {
            entity.HasKey(e => e.GradingId).HasName("course_grading_schema_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.CourseGradingSchemas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("course_grading_schema_course_code_fkey");
        });

        modelBuilder.Entity<GeneratedStudyPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("generated_study_plan_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.GeneratedStudyPlans).HasConstraintName("generated_study_plan_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.GeneratedStudyPlans).HasConstraintName("generated_study_plan_student_id_fkey");
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.MeetingId).HasName("meeting_pkey");

            entity.HasOne(d => d.Advisor).WithMany(p => p.Meetings).HasConstraintName("meeting_advisor_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Meetings).HasConstraintName("meeting_student_id_fkey");
        });

        modelBuilder.Entity<SisCourse>(entity =>
        {
            entity.HasKey(e => e.CourseCode).HasName("sis_course_pkey");

        });

        modelBuilder.Entity<SisCourseAssessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId).HasName("sis_course_assessment_pkey");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.SisCourseAssessments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_course_assessment_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.SisCourseAssessments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_course_assessment_student_id_fkey");
        });

        modelBuilder.Entity<SisCurrentEnrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("sis_current_enrollment_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.SisCurrentEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_current_enrollment_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.SisCurrentEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_current_enrollment_student_id_fkey");
        });

        modelBuilder.Entity<SisStudent>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("sis_student_pkey");

            entity.HasOne(d => d.Advisor).WithMany(p => p.SisStudents).HasConstraintName("sis_student_advisor_id_fkey");
        });

        modelBuilder.Entity<SisStudentCourseHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("sis_student_course_history_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.SisStudentCourseHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_student_course_history_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.SisStudentCourseHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_student_course_history_student_id_fkey");
        });

        modelBuilder.Entity<SisStudentGrade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("sis_student_grades_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.SisStudentGrades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_student_grades_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.SisStudentGrades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sis_student_grades_student_id_fkey");
        });

        modelBuilder.Entity<StudentRisk>(entity =>
        {
            entity.HasKey(e => e.RiskId).HasName("student_risk_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.StudentRisks).HasConstraintName("student_risk_course_code_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentRisks).HasConstraintName("student_risk_student_id_fkey");
        });

        modelBuilder.Entity<StudyGuide>(entity =>
        {
            entity.HasKey(e => e.StudyGuideId).HasName("study_guide_pkey");

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.StudyGuides).HasConstraintName("study_guide_course_code_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.HasOne(d => d.LinkedAdvisor).WithMany(p => p.Users).HasConstraintName("users_linked_advisor_id_fkey");

            entity.HasOne(d => d.LinkedStudent).WithMany(p => p.Users).HasConstraintName("users_linked_student_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

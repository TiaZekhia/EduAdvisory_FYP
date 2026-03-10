import StatCard from "./components/StatCard";
import CourseCard from "./components/CourseCard";
import { useStudentSummary } from "../context/StudentSummaryProvider";
import { useStudentDashboardCourses } from "./hooks/useStudentDashboardCourses";
import { useStudentDashboardStats } from "./hooks/useStudentDashboardStats";
import AlertsBanner from "./components/AlertsBanner";
import { useAlerts } from "../alerts/hooks/useAlerts";
import { useNavigate } from "react-router-dom";
import { PageHero } from "../../../shared/components/PageHero";
import "./DashboardPage.css";
import { Skeleton } from "primereact/skeleton";
import QuickActionCard from "./components/QuickActionCard";
import SectionHeader from "./components/SectionHeader";
import ProgressSummaryCard from "./components/ProgressSummaryCard.js";

export default function DashboardPage() {
  const { alerts, loading: alertsLoading } = useAlerts(3);
  const { summary, loading: summaryLoading } = useStudentSummary();
  const navigate = useNavigate();

  const {
    performance,
    creditsEnrolled,
    loading: coursesLoading,
    error: coursesError,
  } = useStudentDashboardCourses();

  const {
    stats,
    degreeProgress,
    upcomingMeetingsCount,
    loading: statsLoading,
    error: statsError,
  } = useStudentDashboardStats();

  if (summaryLoading || coursesLoading || statsLoading || alertsLoading) {
    return (
      <div className="cp-page">
        <div className="cp-container">
          <Skeleton width="14rem" height="2rem" className="mb-3" />
          <Skeleton width="26rem" height="1.25rem" className="mb-4" />
          <Skeleton height="8rem" className="mb-3" />
          <Skeleton height="8rem" className="mb-3" />
          <Skeleton height="18rem" className="mb-3" />
        </div>
      </div>
    );
  }

  if (!summary) return <div className="p-4 text-danger">No student data.</div>;
  if (coursesError)
    return <div className="p-4 text-danger">{coursesError}</div>;
  if (statsError) return <div className="p-4 text-danger">{statsError}</div>;

  const kpis = [
    {
      key: "gpa",
      title: "Current GPA",
      value: summary.currentGpa ?? "-",
      subtitle:
        summary.academicStatus === "PROBATION" ? "Probation" : "Good standing",
      icon: "pi pi-graduation-cap",
      valueClassName:
        summary.academicStatus === "PROBATION"
          ? "stat-value-danger"
          : "stat-value-success",
    },
    {
      key: "semester",
      title: "Current Semester",
      value: `Semester ${summary.currentSemester}`,
      subtitle: `${creditsEnrolled} credits enrolled`,
      icon: "pi pi-book",
      valueClassName: "stat-value-blue",
    },
    {
      key: "completed",
      title: "Completed Courses",
      value: stats?.completedCourses ?? 0,
      subtitle: `${stats?.creditsEarned ?? 0} credits earned`,
      icon: "pi pi-check-circle",
      valueClassName: "stat-value-success",
    },
    {
      key: "meetings",
      title: "Upcoming Meetings",
      value: upcomingMeetingsCount,
      subtitle: "Scheduled with advisor",
      icon: "pi pi-calendar",
      valueClassName: "stat-value-purple",
    },
  ];

  const quickActions = [
    {
      key: "course-plan",
      title: "View Course Plan",
      subtitle: "See your personalized academic plan",
      icon: "pi pi-calendar",
      iconClassName: "icon-dark",
      to: "/student/course-plan",
    },
    {
      key: "prereq",
      title: "Check Prerequisites",
      subtitle: "Verify course eligibility",
      icon: "pi pi-chart-line",
      iconClassName: "icon-green",
      to: "/student/progress",
    },
    {
      key: "alerts",
      title: "View Alerts",
      subtitle: "See important notifications",
      icon: "pi pi-exclamation-triangle",
      iconClassName: "icon-orange",
      to: "/student/alerts",
    },
  ];

  return (
    <div className="container-fluid p-4">
      {/* Hero */}
      <PageHero
        title="Dashboard"
        badge={`${summary.programCode} • Semester ${summary.currentSemester}`}
        subtitle={
          <>
            Welcome back,{" "}
            <span className="dashboard-name">{summary.fullName}</span> 👋
          </>
        }
      />

      {/* Alerts */}
      <AlertsBanner alerts={alerts} />

      {/* KPI cards */}
      <div className="row g-3 mt-2">
        {kpis.map((kpi) => (
          <div key={kpi.key} className="col-12 col-md-6 col-lg-3">
            <StatCard
              title={kpi.title}
              value={kpi.value}
              subtitle={kpi.subtitle}
              icon={kpi.icon}
              valueClassName={kpi.valueClassName}
            />
          </div>
        ))}
      </div>

      {/* Degree progress */}
      <div className="mt-4">
  <ProgressSummaryCard
    title="Degree Progress"
    subtitle="Track your progress towards graduation"
    current={degreeProgress?.creditsEarned ?? 0}
    total={degreeProgress?.creditsRequired ?? 0}
    percent={degreeProgress?.percentComplete ?? 0}
    stats={{
      passed: stats?.completedCourses ?? 0,
      inProgress: performance.length,
      failed: stats?.failedCourses ?? 0,
    }}
  />
</div>

      {/* Current courses */}
      <div className="mt-4">
        <SectionHeader title={"Current Semester Courses"} subtitle={"Your enrolled courses and performance"}/>

        <div className="d-flex flex-column gap-3 mt-3">
          {performance.map((c) => (
            <CourseCard key={c.courseCode} course={c} />
          ))}
        </div>
      </div>

      {/* Quick actions */}
      <div className="mt-4">
        <h4 className="mb-3">Quick Actions</h4>

        <div className="row g-3">
          {quickActions.map((a) => (
            <div key={a.key} className="col-12 col-md-4">
              <QuickActionCard
                title={a.title}
                subtitle={a.subtitle}
                icon={a.icon}
                iconClassName={a.iconClassName}
                onClick={() => navigate(a.to)}
              />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

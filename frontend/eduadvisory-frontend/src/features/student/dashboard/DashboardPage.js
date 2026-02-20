import StatCard from "./components/StatCard";
import CourseCard from "./components/CourseCard";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { useStudentSummary } from "../context/StudentSummaryProvider";
import { ProgressBar } from "primereact/progressbar";
import { useStudentDashboardCourses } from "./hooks/useStudentDashboardCourses";
import { useStudentDashboardStats } from "./hooks/useStudentDashboardStats";

export default function DashboardPage() {
  const { summary, loading: summaryLoading } = useStudentSummary();

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

if (summaryLoading || coursesLoading || statsLoading) {
  return <div className="p-4">Loading...</div>;
}

if (!summary) return <div className="p-4 text-danger">No student data.</div>;
if (coursesError) return <div className="p-4 text-danger">{coursesError}</div>;
if (statsError) return <div className="p-4 text-danger">{statsError}</div>;

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Dashboard</h2>
        <div className="text-muted">
          {summary.programCode} • Semester {summary.currentSemester}
        </div>
      </div>

      <h3 className="mt-3">Welcome back, {summary.fullName}!</h3>

      {/* KPI cards */}
      <div className="row g-3 mt-2">
  <div className="col-12 col-md-6 col-lg-3">
    <StatCard
      title="Current GPA"
      value={summary.currentGpa ?? "-"}
      subtitle={summary.academicStatus === "PROBATION" ? "Probation" : "Good standing"}
    />
  </div>

  <div className="col-12 col-md-6 col-lg-3">
    <StatCard
      title="Current Semester"
      value={`Semester ${summary.currentSemester}`}
      subtitle={`${creditsEnrolled} credits enrolled`}
    />
  </div>

  <div className="col-12 col-md-6 col-lg-3">
    <StatCard
      title="Completed Courses"
      value={stats?.completedCourses ?? 0}
      subtitle={`${stats?.creditsEarned ?? 0} credits earned`}
    />
  </div>

  <div className="col-12 col-md-6 col-lg-3">
    <StatCard
      title="Upcoming Meetings"
      value={upcomingMeetingsCount}
      subtitle="Scheduled with advisor"
    />
  </div>
</div>

<div className="mt-4">
  <div className="card shadow-sm border-0">
    <div className="card-body">
      <h5 className="card-title mb-1">Degree Progress</h5>
      <div className="text-muted small mb-3">Track your progress towards graduation</div>

      <div className="d-flex justify-content-between mb-2">
        <div className="text-muted small">Credits Completed</div>
        <div className="text-muted small">
          {degreeProgress?.creditsEarned ?? 0} / {degreeProgress?.creditsRequired ?? 0}
        </div>
      </div>

      <ProgressBar value={degreeProgress?.percentComplete ?? 0} />

      <div className="row text-center mt-3">
        <div className="col">
          <div className="fs-4 fw-semibold text-success">{stats?.completedCourses ?? 0}</div>
          <div className="text-muted small">Passed</div>
        </div>
        <div className="col">
          <div className="fs-4 fw-semibold text-primary">{performance.length}</div>
          <div className="text-muted small">In Progress</div>
        </div>
        <div className="col">
          <div className="fs-4 fw-semibold text-danger">{stats?.failedCourses ?? 0}</div>
          <div className="text-muted small">Failed</div>
        </div>
      </div>
    </div>
  </div>
</div>

      {/* Current courses */}
      <div className="mt-4">
        <div className="d-flex justify-content-between align-items-center mb-2">
          <div>
            <h4 className="m-0">Current Semester Courses</h4>
            <div className="text-muted small">Your enrolled courses and performance</div>
          </div>
        </div>

        <div className="d-flex flex-column gap-3">
          {performance.map((c) => (
            <CourseCard key={c.courseCode} course={c} />
          ))}
        </div>
      </div>

      {/* Quick actions */}
      <div className="mt-4">
        <h4 className="mb-2">Quick Actions</h4>
        <div className="row g-3">
          <div className="col-12 col-md-4">
            <Card className="shadow-sm">
              <div className="fw-semibold">View Course Plan</div>
              <div className="text-muted small mt-1">See your personalized academic plan</div>
              <Button className="mt-3" label="Open" icon="pi pi-arrow-right" />
            </Card>
          </div>
          <div className="col-12 col-md-4">
            <Card className="shadow-sm">
              <div className="fw-semibold">Check Prerequisites</div>
              <div className="text-muted small mt-1">Verify course eligibility</div>
              <Button className="mt-3" label="Open" icon="pi pi-arrow-right" />
            </Card>
          </div>
          <div className="col-12 col-md-4">
            <Card className="shadow-sm">
              <div className="fw-semibold">View Alerts</div>
              <div className="text-muted small mt-1">See important notifications</div>
              <Button className="mt-3" label="Open" icon="pi pi-arrow-right" />
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
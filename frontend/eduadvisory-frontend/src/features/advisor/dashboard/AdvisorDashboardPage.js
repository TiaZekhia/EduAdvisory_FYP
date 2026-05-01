import { useEffect, useState } from "react";
import { Skeleton } from "primereact/skeleton";
import { Tag } from "primereact/tag";
import { advisorApi } from "../../../services/advisors/advisorApi";
import { useAdvisorSummary } from "../context/AdvisorSummaryProvider";
import "./AdvisorDashboardPage.css";
import { PageHero } from "../../../shared/components/PageHero";

export default function AdvisorDashboardPage() {
  const { summary } = useAdvisorSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [stats, setStats] = useState(null);
  const [students, setStudents] = useState([]);
  const [recentActivity, setRecentActivity] = useState([]);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      advisorApi.getDashboardSummary(),
      advisorApi.getStudentsOverview?.(),
      advisorApi.getRecentActivity?.(),
    ])
      .then(([statsRes, studentsRes, activityRes]) => {
        setStats(statsRes?.data ?? {});
        setStudents(studentsRes?.data ?? []);
        setRecentActivity(activityRes?.data ?? []);
      })
      .catch((e) => {
        console.error(e);
        setErr(e?.response?.data ?? e?.message ?? "Failed to load advisor dashboard");
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="12rem" height="2rem" className="mb-2" />
        <Skeleton width="22rem" height="1.2rem" className="mb-4" />

        <div className="row g-3 mb-4">
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="8rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="8rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="8rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="8rem" /></div>
        </div>

        <Skeleton height="18rem" className="mb-4" />
        <Skeleton height="16rem" />
      </div>
    );
  }

  if (err) {
    return <div className="p-4 text-danger">{String(err)}</div>;
  }

  return (
    <div className="container-fluid p-4 advisor-dashboard-page">
      <PageHero
              title="Dashboard"
              subtitle={
                <>
                  Overview of your advising activities and student statistics
                </>
              }
            />
      <div className="row g-3">
        <div className="col-12 col-md-6 col-lg-3">
          <AdvisorStatCard
            title="Total Students"
            value={stats?.totalStudents ?? 0}
            subtitle="Active advisees"
            icon="pi pi-users"
            iconClass="icon-dark"
          />
        </div>

        <div className="col-12 col-md-6 col-lg-3">
          <AdvisorStatCard
            title="On Probation"
            value={stats?.onProbation ?? 0}
            subtitle="Requires attention"
            icon="pi pi-exclamation-triangle"
            iconClass="icon-orange"
          />
        </div>

        <div className="col-12 col-md-6 col-lg-3">
          <AdvisorStatCard
            title="Average GPA"
            value={Number(stats?.averageGpa ?? 0).toFixed(2)}
            subtitle="Across all students"
            icon="pi pi-graduation-cap"
            iconClass="icon-green"
          />
        </div>

        <div className="col-12 col-md-6 col-lg-3">
          <AdvisorStatCard
            title="Upcoming Meetings"
            value={stats?.upcomingMeetings ?? 0}
            subtitle="This week"
            icon="pi pi-calendar"
            iconClass="icon-blue"
          />
        </div>
      </div>

      <div className="advisor-section-card mt-4">
        <div className="advisor-section-title">Students Overview</div>
        <div className="advisor-section-subtitle">Quick view of all your advisees</div>

        <div className="d-flex flex-column gap-3 mt-4">
          {students.length === 0 ? (
            <div className="text-muted">No students found.</div>
          ) : (
            students.map((student) => (
              <StudentOverviewRow key={student.studentId} student={student} />
            ))
          )}
        </div>
      </div>

      <div className="advisor-section-card mt-4">
        <div className="advisor-section-title">Recent Activity</div>
        <div className="advisor-section-subtitle">Latest actions and updates</div>

        <div className="mt-4">
          {recentActivity.length === 0 ? (
            <div className="text-muted">No recent activity.</div>
          ) : (
            recentActivity.map((item, index) => (
              <RecentActivityRow
                key={item.id ?? index}
                item={item}
                isLast={index === recentActivity.length - 1}
              />
            ))
          )}
        </div>
      </div>
    </div>
  );
}

function AdvisorStatCard({ title, value, subtitle, icon, iconClass }) {
  return (
    <div className="advisor-stat-card">
      <div className="advisor-stat-top">
        <div className="advisor-stat-title-wrap">
          <i className={`${icon} advisor-stat-icon ${iconClass}`} />
          <span className="advisor-stat-title">{title}</span>
        </div>
      </div>

      <div className="advisor-stat-value">{value}</div>
      <div className="advisor-stat-subtitle">{subtitle}</div>
    </div>
  );
}

function StudentOverviewRow({ student }) {
  const isProbation = student.academicStatus === "PROBATION";
  const gpa = Number(student.gpa ?? 0);

  return (
    <div className="advisor-student-row">
      <div className="advisor-student-left">
        <div className="advisor-student-name-wrap">
          <div className="advisor-student-name">{student.name}</div>

          {isProbation && (
            <Tag value="Probation" severity="danger" rounded />
          )}
        </div>

        <div className="advisor-student-meta">
          {student.studentId} • Semester {student.currentSemester}
        </div>
      </div>

      <div className="advisor-student-right">
        <div className="advisor-student-gpa-label">GPA</div>
        <div
          className={`advisor-student-gpa ${
            gpa >= 3 ? "gpa-good" : gpa < 2.5 ? "gpa-risk" : "gpa-mid"
          }`}
        >
          {gpa.toFixed(1)}
        </div>
      </div>
    </div>
  );
}

function RecentActivityRow({ item, isLast }) {
  return (
    <div className={`advisor-activity-row ${!isLast ? "with-border" : ""}`}>
      <div className="advisor-activity-dot" />
      <div className="advisor-activity-content">
        <div className="advisor-activity-title">{item.title}</div>
        <div className="advisor-activity-time">{item.timeAgo}</div>
      </div>
    </div>
  );
}
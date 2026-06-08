import { useEffect, useState } from "react";
import { Skeleton } from "primereact/skeleton";
import { Tag } from "primereact/tag";
import { advisorApi } from "../../../services/advisors/advisorApi";
import { advisorMeetingsApi } from "../../../services/advisors/advisorMeetingsApi";
import { useAdvisorSummary } from "../context/AdvisorSummaryProvider";
import "./AdvisorDashboardPage.css";
import { PageHero } from "../../../shared/components/PageHero";
import UpcomingMeetingsCalendar from "../../../shared/components/UpcomingMeetingsCalendar";

export default function AdvisorDashboardPage() {
  const { summary } = useAdvisorSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [stats, setStats] = useState(null);
  const [students, setStudents] = useState([]);
  const [recentActivity, setRecentActivity] = useState([]);
  const [upcomingMeetings, setUpcomingMeetings] = useState([]);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      advisorApi.getDashboardSummary(),
      advisorApi.getStudentsOverview?.(),
      advisorApi.getRecentActivity?.(),
      advisorMeetingsApi.getUpcoming(),
    ])
      .then(([statsRes, studentsRes, activityRes, meetingsRes]) => {
        setStats(statsRes?.data ?? {});
        setStudents(studentsRes?.data ?? []);
        setRecentActivity(activityRes?.data ?? []);
        setUpcomingMeetings(
          (meetingsRes?.data ?? []).filter(
            (m) => new Date(m.endAt ?? m.startAt) > new Date()
          )
        );
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
        subtitle={<>Overview of your advising activities and student statistics</>}
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
            value={Number(stats?.averageGpa ?? 0).toFixed(1)}
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

      <UpcomingMeetingsCalendar
        meetings={upcomingMeetings}
        nameField="studentName"
        title="Upcoming Meetings"
        subtitle="Your scheduled advising sessions"
      />

      <StudentOverviewSection students={students} />

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


/* ─── Student Overview Section (with filters) ────────────────── */
function StudentOverviewSection({ students }) {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("ALL");
  const [gpaFilter, setGpaFilter] = useState("ALL");
  const [semesterFilter, setSemesterFilter] = useState("ALL");
  const [programFilter, setProgramFilter] = useState("ALL");

  const semesters = [...new Set(students.map((s) => s.currentSemester))].sort(
    (a, b) => a - b
  );

  const programs = [...new Set(students.map((s) => s.programCode).filter(Boolean))].sort();

  const filtered = students.filter((s) => {
    const q = search.trim().toLowerCase();
    if (
      q &&
      !s.name.toLowerCase().includes(q) &&
      !String(s.studentId).toLowerCase().includes(q)
    )
      return false;

    if (statusFilter !== "ALL") {
      const st = (s.academicStatus ?? "GOOD").toUpperCase();
      if (statusFilter === "PROBATION" && st !== "PROBATION") return false;
      if (statusFilter === "GOOD" && st === "PROBATION") return false;
    }

    const gpa = Number(s.gpa ?? 0);
    if (gpaFilter === "BELOW40" && gpa >= 40) return false;
    if (gpaFilter === "40TO60" && (gpa < 40 || gpa >= 60)) return false;
    if (gpaFilter === "60TO80" && (gpa < 60 || gpa >= 80)) return false;
    if (gpaFilter === "80PLUS" && gpa < 80) return false;

    if (semesterFilter !== "ALL" && s.currentSemester !== Number(semesterFilter))
      return false;

    if (programFilter !== "ALL" && (s.programCode ?? "") !== programFilter)
      return false;

    return true;
  });

  const hasActiveFilters =
    search.trim() ||
    statusFilter !== "ALL" ||
    gpaFilter !== "ALL" ||
    semesterFilter !== "ALL" ||
    programFilter !== "ALL";

  function clearFilters() {
    setSearch("");
    setStatusFilter("ALL");
    setGpaFilter("ALL");
    setSemesterFilter("ALL");
    setProgramFilter("ALL");
  }

  return (
    <div className="advisor-section-card mt-4">
      {/* Header */}
      <div className="students-section-header">
        <div>
          <div className="advisor-section-title">Students Overview</div>
          <div className="advisor-section-subtitle">Quick view of all your advisees</div>
        </div>
        <div className="students-total-badge">
          <i className="pi pi-users" />
          {students.length} students
        </div>
      </div>

      {/* Filter bar */}
      <div className="students-filter-panel mt-3">
        <div className="students-filter-label">
          <i className="pi pi-filter" />
          Filters
        </div>

        <div className="students-filter-controls">
          <div className="students-filter-search-wrap">
            <i className="pi pi-search students-filter-search-icon" />
            <input
              className="students-filter-search"
              placeholder="Name or Student ID…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            {search && (
              <button
                className="students-filter-search-clear"
                onClick={() => setSearch("")}
              >
                <i className="pi pi-times" />
              </button>
            )}
          </div>

          <div className="students-filter-selects">
            <div className="students-filter-select-wrap">
              <i className="pi pi-circle students-filter-select-icon" />
              <select
                className="students-filter-select"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="ALL">All Statuses</option>
                <option value="GOOD">Good Standing</option>
                <option value="PROBATION">Probation</option>
              </select>
            </div>

            <div className="students-filter-select-wrap">
              <i className="pi pi-graduation-cap students-filter-select-icon" />
              <select
                className="students-filter-select"
                value={gpaFilter}
                onChange={(e) => setGpaFilter(e.target.value)}
              >
                <option value="ALL">All GPAs</option>
                <option value="80PLUS">80 and above</option>
                <option value="60TO80">60 – 79</option>
                <option value="40TO60">40 – 59</option>
                <option value="BELOW40">Below 40</option>
              </select>
            </div>

            <div className="students-filter-select-wrap">
              <i className="pi pi-calendar students-filter-select-icon" />
              <select
                className="students-filter-select"
                value={semesterFilter}
                onChange={(e) => setSemesterFilter(e.target.value)}
              >
                <option value="ALL">All Semesters</option>
                {semesters.map((sem) => (
                  <option key={sem} value={sem}>
                    Semester {sem}
                  </option>
                ))}
              </select>
            </div>

            {programs.length > 0 && (
              <div className="students-filter-select-wrap">
                <i className="pi pi-book students-filter-select-icon" />
                <select
                  className="students-filter-select"
                  value={programFilter}
                  onChange={(e) => setProgramFilter(e.target.value)}
                >
                  <option value="ALL">All Programs</option>
                  {programs.map((p) => (
                    <option key={p} value={p}>{p}</option>
                  ))}
                </select>
              </div>
            )}

            {hasActiveFilters && (
              <button className="students-filter-clear" onClick={clearFilters}>
                <i className="pi pi-times" />
                Clear all
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Results count */}
      <div className="students-filter-count">
        {hasActiveFilters ? (
          <>
            <span className="students-count-highlight">{filtered.length}</span>
            {" of "}
            {students.length} students match
          </>
        ) : (
          <>
            {students.length} student{students.length !== 1 ? "s" : ""} total
          </>
        )}
      </div>

      {/* List */}
      <div className="d-flex flex-column gap-3 mt-2">
        {filtered.length === 0 ? (
          <div className="text-muted">No students match the selected filters.</div>
        ) : (
          filtered.map((student) => (
            <StudentOverviewRow key={student.studentId} student={student} />
          ))
        )}
      </div>
    </div>
  );
}

/* ─── Student Row ─────────────────────────────────────────────── */
function StudentOverviewRow({ student }) {
  const isProbation = (student.academicStatus ?? "").toUpperCase() === "PROBATION";
  const gpa = Number(student.gpa ?? 0);

  const gpaColorClass =
    gpa >= 80 ? "gpa-good" : gpa >= 60 ? "gpa-mid" : "gpa-risk";

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
          {student.programCode && (
            <span className="advisor-student-program">{student.programCode}</span>
          )}
        </div>
      </div>

      <div className="advisor-student-right">
        <div className="advisor-student-gpa-label">GPA</div>
        <div className={`advisor-student-gpa ${gpaColorClass}`}>
          {gpa.toFixed(1)}
        </div>
      </div>
    </div>
  );
}

/* ─── Stat Card ───────────────────────────────────────────────── */
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

/* ─── Recent Activity Row ─────────────────────────────────────── */
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

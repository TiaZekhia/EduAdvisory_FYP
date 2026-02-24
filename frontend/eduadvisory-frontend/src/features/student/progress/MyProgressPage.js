import { Card } from "primereact/card";
import { ProgressBar } from "primereact/progressbar";
import { Tag } from "primereact/tag";
import { TabMenu } from "primereact/tabmenu";
import { useMemo, useState } from "react";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { useProgressSummary } from "./hooks/useProgressSummary";
import { useProgressDepartments } from "./hooks/useProgressDepartments";
import { useProgressHistory } from "./hooks/useProgressHistory";
import { useStudyGuideComparison } from "./hooks/useStudyGuideComparison";
import { PageHero } from "../../../shared/components/PageHero";
import "./MyProgressPage.css";

export default function MyProgressPage() {
  const { summary } = useStudentSummary();

  const { data: progress } = useProgressSummary();
  const { data: departments } = useProgressDepartments();
  const { data: history } = useProgressHistory();
  const { data: comparison } = useStudyGuideComparison();

  const semesters = useMemo(() => {
    if (!history) return [];
    // your history semester strings are like "Semester 1", "Semester 2"
    return history.map((h) => ({ label: h.semester, value: h.semester }));
  }, [history]);

  const [activeIndex, setActiveIndex] = useState(0);
  const activeSemester = history?.[activeIndex];

  return (
    <div className="container-fluid p-3 p-md-4">
      {/* Page header */}
      <PageHero
        title="My Progress"
        badge={`${summary?.programCode} • Semester ${summary?.currentSemester}`}
        subtitle="Detailed view of your academic journey and achievements"
      />
      {/* ===== Degree Completion Progress ===== */}
      <Card className="deg-card mb-4">
        <div className="deg-head">
          <div className="deg-title">Degree Completion Progress</div>
          <div className="deg-subtitle">
            Your progress towards{" "}
            <span className="deg-program">{summary?.programCode}</span> degree
          </div>
        </div>

        <div className="deg-progress-row">
          <div className="deg-progress-label">Overall Progress</div>
          <div className="deg-progress-pct">
            {(progress?.percentComplete ?? 0).toFixed?.(1) ??
              progress?.percentComplete ??
              0}
            %
          </div>
        </div>

        <ProgressBar
          value={progress?.percentComplete ?? 0}
          showValue={false}
          className="deg-progressbar"
        />

        {/* KPI tiles */}
        <div className="row g-3 mt-3">
          <KpiTile
            icon="pi pi-bookmark"
            iconClassName="kpi-icon-gold"
            value={progress?.creditsEarned ?? 0}
            label="Credits Earned"
          />
          <KpiTile
            icon="pi pi-clock"
            iconClassName="kpi-icon-blue"
            value={progress?.creditsRemaining ?? 0}
            label="Credits Remaining"
          />
          <KpiTile
            icon="pi pi-check-circle"
            iconClassName="kpi-icon-green"
            value={progress?.coursesPassed ?? 0}
            label="Courses Passed"
          />
          <KpiTile
            icon="pi pi-times-circle"
            iconClassName="kpi-icon-red"
            value={progress?.coursesFailed ?? 0}
            label="Courses Failed"
          />
        </div>

        <div className="deg-divider" />

        {/* Departments */}
        <div className="deg-dept">
          <div className="deg-dept-title">Credits by Department:</div>

          <div className="deg-dept-chips">
            {(departments ?? []).map((d) => (
              <span key={d.department} className="dept-chip">
                <span className="dept-chip-code">{d.department}</span>
                <span className="dept-chip-text">
                  {d.coursesCount} course(s)
                </span>
              </span>
            ))}
          </div>
        </div>
      </Card>

      {/* ===== Course History ===== */}
      <Card className="ch-card mb-4">
        <div className="ch-title">Course History</div>
        <div className="ch-subtitle">All your enrolled courses by semester</div>

        {semesters.length > 0 && (
          <TabMenu
            model={semesters}
            activeIndex={activeIndex}
            onTabChange={(e) => setActiveIndex(e.index)}
            className="ch-tabs"
          />
        )}

        {/* semester summary row */}
        {activeSemester && (
          <div className="ch-summary">
            <div className="ch-summary-item">
              <div className="ch-summary-label">Semester GPA</div>
              <div className="ch-summary-value">
                {activeSemester.semesterGpa}
              </div>
            </div>

            <div className="ch-summary-item">
              <div className="ch-summary-label">Credits</div>
              <div className="ch-summary-value">{activeSemester.credits}</div>
            </div>

            <div className="ch-summary-item">
              <div className="ch-summary-label">Courses</div>
              <div className="ch-summary-value">
                {activeSemester.coursesCount}
              </div>
            </div>
          </div>
        )}

        {/* course rows */}
        <div className="ch-list">
          {(activeSemester?.courses ?? []).map((c) => {
            const failed = c.status === "FAILED";
            return (
              <div key={c.courseCode} className="ch-row">
                <div className="ch-row-left">
                  <div className="ch-course-title">
                    <span className="ch-course-name">
                      {c.courseCode} - {c.courseName}
                    </span>

                    <span className="ch-credit-chip">{c.credits} credits</span>
                  </div>

                  <div className="ch-prereq">
                    Prerequisites:{" "}
                    {c.prerequisites?.length
                      ? c.prerequisites.join(", ")
                      : "None"}
                  </div>
                </div>

                <div className="ch-row-right">
                  <div className="ch-grade-label">Grade</div>
                  <div
                    className={`ch-grade ${failed ? "is-failed" : "is-passed"}`}
                  >
                    {c.finalGrade}/100
                  </div>

                  <Tag
                    value={c.status?.toLowerCase()}
                    severity={failed ? "danger" : "success"}
                    rounded
                    className="ch-status"
                  />
                </div>
              </div>
            );
          })}
        </div>
      </Card>

      {/* ===== Study Guide Comparison ===== */}
      <Card className="sg-card">
        <div className="sg-title">Study Guide Comparison</div>
        <div className="sg-subtitle">
          Your progress compared to the recommended study guide
        </div>

        {(comparison ?? []).map((row) => {
          const pct =
            row.total > 0 ? Math.round((row.completed / row.total) * 100) : 0;

          const sev =
            pct === 100 ? "success" : pct >= 50 ? "secondary" : "danger";

          return (
            <div key={row.recommendedSemester} className="sg-row">
              <div className="sg-row-head">
                <div className="sg-row-title">
                  Semester {row.recommendedSemester}: {row.termName ?? ""}
                </div>

                <Tag
                  value={`${row.completed}/${row.total} completed`}
                  severity={sev}
                  rounded
                  className="sg-tag"
                />
              </div>

              <ProgressBar
                value={pct}
                showValue={false}
                className="sg-progress"
              />
            </div>
          );
        })}
      </Card>
    </div>
  );
}

function KpiTile({ icon, iconClassName = "", value, label }) {
  return (
    <div className="col-12 col-md-6 col-lg-3">
      <div className="kpi-tile">
        <i className={`${icon} kpi-icon ${iconClassName}`} />
        <div className="kpi-value">{value}</div>
        <div className="kpi-label">{label}</div>
      </div>
    </div>
  );
}

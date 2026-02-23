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
      <Card className="shadow-sm border-0 mb-4">
        <div className="fw-semibold fs-5">Degree Completion Progress</div>
        <div className="text-muted mb-3">
          Your progress towards{" "}
          <span className="text-primary">{summary?.programCode}</span> degree
        </div>

        <div className="d-flex justify-content-between align-items-center mb-2">
          <div className="text-muted">Overall Progress</div>
          <div className="text-muted">{progress?.percentComplete ?? 0}%</div>
        </div>

        <ProgressBar value={progress?.percentComplete ?? 0} />

        {/* KPI cards */}
        <div className="row g-3 mt-3">
          <KpiBox value={progress?.creditsEarned ?? 0} label="Credits Earned" />
          <KpiBox
            value={progress?.creditsRemaining ?? 0}
            label="Credits Remaining"
          />
          <KpiBox value={progress?.coursesPassed ?? 0} label="Courses Passed" />
          <KpiBox value={progress?.coursesFailed ?? 0} label="Courses Failed" />
        </div>

        {/* Departments chips */}
        <div className="mt-4">
          <div className="fw-semibold mb-2">Credits by Department:</div>

          <div className="d-flex flex-wrap gap-2">
            {(departments ?? []).map((d) => (
              <Tag
                key={d.department}
                value={`${d.department} ${d.coursesCount} course(s)`}
                rounded
                className="px-3 py-2"
              />
            ))}
          </div>
        </div>
      </Card>

      {/* ===== Course History ===== */}
      <Card className="shadow-sm border-0 mb-4">
        <div className="fw-semibold fs-5">Course History</div>
        <div className="text-muted mb-3">
          All your enrolled courses by semester
        </div>

        {semesters.length > 0 && (
          <TabMenu
            model={semesters}
            activeIndex={activeIndex}
            onTabChange={(e) => setActiveIndex(e.index)}
            className="mb-3"
          />
        )}

        {/* semester summary row */}
        {activeSemester && (
          <div className="p-3 rounded bg-light d-flex justify-content-between align-items-center mb-3">
            <div>
              <div className="text-muted small">Semester GPA</div>
              <div className="fs-3 fw-semibold">
                {activeSemester.semesterGpa}
              </div>
            </div>
            <div>
              <div className="text-muted small text-end">Credits</div>
              <div className="fs-3 fw-semibold text-end">
                {activeSemester.credits}
              </div>
            </div>
            <div>
              <div className="text-muted small text-end">Courses</div>
              <div className="fs-3 fw-semibold text-end">
                {activeSemester.coursesCount}
              </div>
            </div>
          </div>
        )}

        {/* course cards */}
        <div className="d-flex flex-column gap-3">
          {(activeSemester?.courses ?? []).map((c) => (
            <div key={c.courseCode} className="border rounded p-3 bg-white">
              <div className="d-flex justify-content-between">
                <div>
                  <div className="fw-semibold">
                    {c.courseCode} - {c.courseName}{" "}
                    <span className="badge bg-light text-dark border ms-2">
                      {c.credits} credits
                    </span>
                  </div>
                  <div className="text-muted small mt-1">
                    Prerequisites:{" "}
                    {c.prerequisites?.length
                      ? c.prerequisites.join(", ")
                      : "None"}
                  </div>
                </div>

                <div className="text-end">
                  <div className="text-muted small">Grade</div>
                  <div
                    className={`fw-bold ${c.status === "FAILED" ? "text-danger" : "text-success"}`}
                  >
                    {c.finalGrade}%
                  </div>

                  <Tag
                    value={c.status?.toLowerCase()}
                    severity={c.status === "FAILED" ? "danger" : "success"}
                    className="mt-2"
                    rounded
                  />
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>

      {/* ===== Study Guide Comparison ===== */}
      <Card className="shadow-sm border-0">
        <div className="fw-semibold fs-5">Study Guide Comparison</div>
        <div className="text-muted mb-3">
          Your progress compared to the recommended study guide
        </div>

        {(comparison ?? []).map((row) => {
          const pct =
            row.total > 0 ? Math.round((row.completed / row.total) * 100) : 0;
          const sev = pct >= 70 ? "success" : pct >= 40 ? "warning" : "danger";

          return (
            <div key={row.recommendedSemester} className="mb-3">
              <div className="d-flex justify-content-between align-items-center mb-2">
                <div className="fw-semibold">
                  Semester {row.recommendedSemester}
                </div>
                <Tag
                  value={`${row.completed}/${row.total} completed`}
                  severity={sev}
                  rounded
                />
              </div>

              <ProgressBar value={pct} />
            </div>
          );
        })}
      </Card>
    </div>
  );
}

function KpiBox({ value, label }) {
  return (
    <div className="col-12 col-md-6 col-lg-3">
      <div className="border rounded bg-white p-3 text-center h-100">
        <div className="fs-2 fw-semibold">{value}</div>
        <div className="text-muted">{label}</div>
      </div>
    </div>
  );
}

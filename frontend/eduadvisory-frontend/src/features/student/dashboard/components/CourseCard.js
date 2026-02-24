import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { ProgressBar } from "primereact/progressbar";
import "./components.css"
function computeBadge(course) {
  const abs = course.absencesCount ?? 0;
  const max = course.maxAbsences ?? 0;
  const absRatio = max > 0 ? abs / max : 0;

  const mid = course.components?.find((c) => c.componentName === "MIDTERM")?.grade ?? null;

  if ((mid !== null && mid < 50) || absRatio >= 0.7) {
    return { severity: "danger", label: "High Risk" };
  }
  return { severity: "success", label: "On Track" };
}

function fmtScore(v) {
  if (v === null || v === undefined) return "-";
  return `${v}/100`;
}

export default function CourseCard({ course }) {
  const badge = computeBadge(course);

  const comps = course.components ?? [];
  const mid = comps.find((c) => c.componentName === "MIDTERM")?.grade;
  const proj = comps.find((c) => c.componentName === "PROJECT")?.grade;
  const lab = comps.find((c) => c.componentName === "LAB")?.grade;

  const abs = course.absencesCount ?? 0;
  const max = course.maxAbsences ?? 0;
  const absPercent = max > 0 ? Math.min(100, Math.round((abs / max) * 100)) : 0;

  return (
    <Card className="course-card">
      {/* Top row */}
      <div className="course-card-top">
        <div className="course-card-left">
          <div className="course-card-title">
            <span className="course-code">{course.courseCode}</span>
            <span className="course-name">{course.courseName}</span>
          </div>

          <div className="course-meta">
            <span className="course-chip">
              <i className="pi pi-book" /> {course.credits} credits
            </span>

            {max > 0 && (
              <span className="course-chip">
                <i className="pi pi-user-minus" /> Absences {abs}/{max}
              </span>
            )}
          </div>
        </div>

        <Tag severity={badge.severity} value={badge.label} />
      </div>

      {/* Grades grid */}
      <div className="course-metrics">
        <div className="metric-tile">
          <div className="metric-label">Midterm</div>
          <div className="metric-value">{fmtScore(mid)}</div>
        </div>

        <div className="metric-tile">
          <div className="metric-label">Project</div>
          <div className="metric-value">{fmtScore(proj)}</div>
        </div>

        <div className="metric-tile">
          <div className="metric-label">Lab</div>
          <div className="metric-value">{fmtScore(lab)}</div>
        </div>

        <div className="metric-tile">
          <div className="metric-label">Absences</div>
          <div className="metric-value">
            {max > 0 ? `${abs}/${max}` : "-"}
          </div>
        </div>
      </div>

      {/* Optional: absence progress */}
      {max > 0 && (
        <div className="course-abs-bar">
          <div className="course-abs-row">
            <span className="course-abs-label">Absence usage</span>
            <span className="course-abs-label">{absPercent}%</span>
          </div>
          <ProgressBar value={absPercent} showValue={false} className="deg-progressbar" />
        </div>
      )}
    </Card>
  );
}
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";

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

export default function CourseCard({ course }) {
  const badge = computeBadge(course);

  const comps = course.components ?? [];
  const mid = comps.find((c) => c.componentName === "MIDTERM")?.grade;
  const proj = comps.find((c) => c.componentName === "PROJECT")?.grade;
  const lab = comps.find((c) => c.componentName === "LAB")?.grade;

  return (
    <Card className="shadow-sm">
      <div className="d-flex justify-content-between align-items-start">
        <div>
          <div className="fw-semibold">
            {course.courseCode} - {course.courseName}
          </div>
          <div className="text-muted small">{course.credits} credits</div>
        </div>

        <Tag severity={badge.severity} value={badge.label} />
      </div>

      <div className="row mt-3 g-3">
        <div className="col-6 col-md-3">
          <div className="text-muted small">Midterm</div>
          <div className="fw-semibold">{mid ?? "-"}/100</div>
        </div>
        <div className="col-6 col-md-3">
          <div className="text-muted small">Project</div>
          <div className="fw-semibold">{proj ?? "-"}/100</div>
        </div>
        <div className="col-6 col-md-3">
          <div className="text-muted small">Lab</div>
          <div className="fw-semibold">{lab ?? "-"}/100</div>
        </div>
        <div className="col-6 col-md-3">
          <div className="text-muted small">Absences</div>
          <div className="fw-semibold">
            {course.absencesCount ?? 0}/{course.maxAbsences ?? 0}
          </div>
        </div>
      </div>
    </Card>
  );
}
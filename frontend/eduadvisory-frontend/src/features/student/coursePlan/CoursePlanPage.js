import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentCoursePlanApi } from "../../../services/students/studentCoursePlanApi";

export default function CoursePlanPage() {
  const loadPlans = () => {
  setLoading(true);
  setErr("");
  studentCoursePlanApi.getPlans(3)
    .then(res => {
      setPlans(res.data || []);
      setSelectedPlanIndex(0);
    })
    .catch(e => setErr(e?.message || "Failed to generate plans"))
    .finally(() => setLoading(false));
};

useEffect(() => {
  loadPlans();
}, []);

  const { summary } = useStudentSummary();

  const [plans, setPlans] = useState([]);
const [insights, setInsights] = useState(null);
const [selectedPlanIndex, setSelectedPlanIndex] = useState(0);
const [loading, setLoading] = useState(true);
const [err, setErr] = useState("");

const loadInsights = () => {
  setLoading(true);
  setErr("");

  studentCoursePlanApi.getInsights(3)
    .then(res => {
      const p = res.data?.plans || [];
      const ai = res.data?.insights || null;

      setPlans(p);
      setInsights(ai);

      const best = ai?.bestPlanIndex;
      setSelectedPlanIndex(typeof best === "number" ? best : 0);
    })
    .catch(e => setErr(e?.response?.data ?? e?.message ?? "Failed to load insights"))
    .finally(() => setLoading(false));
};

useEffect(() => {
  loadInsights();
}, []);

const selected = useMemo(() => plans[selectedPlanIndex], [plans, selectedPlanIndex]);

const selectedInsight = insights?.planInsights?.find(x => x.planIndex === selectedPlanIndex);

// rank = position when sorting by score desc
const selectedRank = useMemo(() => {
  if (!insights?.planInsights?.length) return null;
  const ordered = [...insights.planInsights].sort((a,b) => b.score - a.score);
  const idx = ordered.findIndex(x => x.planIndex === selectedPlanIndex);
  return idx >= 0 ? idx + 1 : null;
}, [insights, selectedPlanIndex]);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="12rem" height="2rem" className="mb-3" />
        <Skeleton width="30rem" className="mb-4" />
        <Skeleton height="10rem" className="mb-4" />
        <Skeleton height="20rem" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;
  if (!selected) return <div className="p-4">No plans generated.</div>;

  // Screenshot-like KPIs (you can refine numbers later)
  const semestersPlanned = selected.semesters?.length ?? 0;
  const creditsRemaining = selected.metrics?.creditsRemaining ?? 0;
  const coursesRemaining = selected.metrics?.coursesRemaining ?? 0;

  // optional: show progress bar by credits earned vs required if you want — you already have degree-progress endpoint

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Course Plan</h2>
        <div className="text-muted">
          {summary?.programCode ?? "-"} • Semester {summary?.currentSemester ?? "-"}
        </div>
      </div>

      <h3 className="mt-3">My Course Plan</h3>
      <div className="text-muted mb-4">Your personalized academic plan to graduation</div>

      <Card className="shadow-sm border-0 mb-4">
        <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
          <div>
            <div className="fw-semibold fs-5">Academic Roadmap</div>
            <div className="text-muted">Personalized plan based on your progress and requirements</div>
            <div className="text-muted small mt-1">
              Strategy: <span className="fw-semibold">{selected.strategy}</span>
            </div>
          </div>

          <div className="d-flex gap-2">
<Button icon="pi pi-refresh" label="Regenerate" outlined onClick={loadInsights} />
            <Button icon="pi pi-download" label="Export Plan" severity="secondary" disabled />
          </div>
        </div>
        <div className="mt-2">
</div>

{insights && (
  <Card className="shadow-sm border-0 mb-3">
    <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
      <div>
        <div className="fw-semibold">AI Recommendation</div>
        <div className="text-muted">{insights.bestPlanSummary}</div>
      </div>
      <Tag value={`Recommended: Plan ${insights.bestPlanIndex + 1}`} severity="success" />
    </div>
  </Card>
)}

{selectedInsight && (
  <div className="mt-2 d-flex gap-2 flex-wrap">
    {selectedRank && <Tag value={`Rank #${selectedRank}`} severity="info" />}
    <Tag value={`Score ${selectedInsight.score}/100`} severity="info" />
  </div>
)}
{selectedInsight && (
  <Card className="shadow-sm border-0 mb-4">
    <div className="d-flex align-items-center gap-2 mb-2">
      <Tag value={`Score ${selectedInsight.score}/100`} severity="info" />
      <Tag value={`Plan ${selectedPlanIndex + 1}`} />
    </div>
    <div className="text-muted mb-3">{selectedInsight.explanation}</div>

    <div className="row g-3">
      <div className="col-12 col-md-4">
        <div className="fw-semibold mb-2">Pros</div>
        <ul className="mb-0">
          {selectedInsight.pros.map((p,i)=><li key={i}>{p}</li>)}
        </ul>
      </div>
      <div className="col-12 col-md-4">
        <div className="fw-semibold mb-2">Cons</div>
        <ul className="mb-0">
          {selectedInsight.cons.map((c,i)=><li key={i}>{c}</li>)}
        </ul>
      </div>
      <div className="col-12 col-md-4">
        <div className="fw-semibold mb-2">Warnings</div>
        <ul className="mb-0">
          {selectedInsight.warnings.length ? selectedInsight.warnings.map((w,i)=><li key={i}>{w}</li>) : <li>None</li>}
        </ul>
      </div>
    </div>
  </Card>
)}

        {/* KPI row */}
        <div className="row g-3 mt-3">
          <Kpi title="Semesters Planned" value={semestersPlanned} icon="pi pi-calendar" />
          <Kpi title="Courses Remaining" value={coursesRemaining} icon="pi pi-check-circle" />
          <Kpi title="Credits Remaining" value={creditsRemaining} icon="pi pi-clock" />
          <Kpi title="Est. Graduation" value={selected.metrics?.estimatedGraduationTerm ?? "-"} icon="pi pi-info-circle" />
        </div>

        {/* Plan selector chips */}
        <div className="mt-4 d-flex align-items-center gap-2 flex-wrap">
          {plans.map((p, idx) => (
            <Button
              key={p.planId}
              label={`Plan ${idx + 1}`}
              size="small"
              outlined={idx !== selectedPlanIndex}
              onClick={() => setSelectedPlanIndex(idx)}
            />
          ))}
        </div>
      </Card>

      {/* Semesters list (cards like screenshot) */}
      <div className="d-flex flex-column gap-3">
        {selected.semesters.map((sem) => (
          <Card key={`${sem.plannedSemester}-${sem.termLabel}`} className="shadow-sm border-0">
            <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
              <div className="d-flex align-items-center gap-2">
                <div className="rounded-circle bg-light d-flex align-items-center justify-content-center"
                     style={{ width: 40, height: 40 }}>
                  <i className="pi pi-calendar text-muted" />
                </div>
                <div>
                  <div className="fw-semibold">
                    Semester {sem.plannedSemester}: {sem.termLabel}
                  </div>
                  <div className="text-muted small">
                    {sem.coursesCount} course(s) • {sem.totalCredits} credits
                  </div>
                </div>
              </div>

              <Button label="Next Semester" size="small" outlined disabled />
            </div>

            <div className="mt-3 d-flex flex-column gap-3">
              {sem.courses.map((c) => (
                <div key={c.courseCode} className="border rounded-3 p-3 bg-white">
                  <div className="d-flex justify-content-between align-items-start gap-2">
                    <div>
                      <div className="fw-semibold">{c.courseCode} - {c.courseName}</div>
                      <div className="text-muted small">
                        Prerequisites: {c.prerequisites?.length ? c.prerequisites.join(", ") : "None"}
                        {c.isRetake ? <span className="ms-2 text-danger fw-semibold">(Retake)</span> : null}
                      </div>
                    </div>

                    <div className="d-flex align-items-center gap-2">
                      <Tag value={`${c.credits} credits`} />
                      <Tag
                        severity={c.prereqsSatisfiedBeforeThisSemester ? "success" : "danger"}
                        value={c.prereqsSatisfiedBeforeThisSemester ? "eligible" : "blocked"}
                      />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}

function Kpi({ title, value, icon }) {
  return (
    <div className="col-12 col-md-6 col-lg-3">
      <div className="border rounded-3 p-3 bg-white h-100">
        <div className="d-flex align-items-center justify-content-between">
          <div>
            <div className="text-muted small">{title}</div>
            <div className="fs-3 fw-semibold">{value}</div>
          </div>
          <i className={`${icon} fs-2 text-muted`} />
        </div>
      </div>
    </div>
  );
}
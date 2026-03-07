import { useEffect, useMemo, useRef, useState } from "react";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { ProgressSpinner } from "primereact/progressspinner";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentCoursePlanApi } from "../../../services/students/studentCoursePlanApi";
import { PageHero } from "../../../shared/components/PageHero";

import "./CoursePlanPage.css";
import { exportCoursePlanPdf } from "./exportCoursePlanPdf";

export default function CoursePlanPage() {
  const { summary } = useStudentSummary();

  const [plans, setPlans] = useState([]);
  const [selectedPlanIndex, setSelectedPlanIndex] = useState(0);

  // AI insights are optional/slow
  const [insights, setInsights] = useState(null);

  // Page loading is ONLY for initial plans
  const [pageLoading, setPageLoading] = useState(true);

  // AI loading is separate
  const [aiLoading, setAiLoading] = useState(false);

  const [err, setErr] = useState("");

  // PDF export ref (capture this)
  const exportRef = useRef(null);

  const selected = useMemo(
    () => plans[selectedPlanIndex],
    [plans, selectedPlanIndex]
  );

  const selectedInsight = useMemo(() => {
    return insights?.planInsights?.find((x) => x.planIndex === selectedPlanIndex);
  }, [insights, selectedPlanIndex]);

  const selectedRank = useMemo(() => {
    if (!insights?.planInsights?.length) return null;
    const ordered = [...insights.planInsights].sort((a, b) => b.score - a.score);
    const idx = ordered.findIndex((x) => x.planIndex === selectedPlanIndex);
    return idx >= 0 ? idx + 1 : null;
  }, [insights, selectedPlanIndex]);

  // 1) FAST: Load plans to render page
  const loadPlans = () => {
    setErr("");
    setPageLoading(true);

    studentCoursePlanApi
      .getPlans(3)
      .then((res) => {
        const p = res.data || [];
        setPlans(p);
        setSelectedPlanIndex(0);
      })
      .catch((e) => setErr(e?.message || "Failed to load plans"))
      .finally(() => setPageLoading(false));
  };

  // 2) SLOW: Load AI insights only (does not block full page)
  const loadInsights = () => {
    setErr("");
    setAiLoading(true);

    studentCoursePlanApi
      .getInsights(3)
      .then((res) => {
        const p = res.data?.plans || [];
        const ai = res.data?.insights || null;

        // If insights returns improved plans, update them
        if (p?.length) setPlans(p);

        setInsights(ai);

        const best = ai?.bestPlanIndex;
        if (typeof best === "number") setSelectedPlanIndex(best);
      })
      .catch((e) =>
        setErr(e?.response?.data ?? e?.message ?? "Failed to load insights")
      )
      .finally(() => setAiLoading(false));
  };

  // Initial load: plans first, insights after (non-blocking)
  useEffect(() => {
    loadPlans();
  }, []);

  useEffect(() => {
    // After plans are loaded, fetch insights in background
    if (!pageLoading) loadInsights();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageLoading]);

const exportToPdf = () => {
  try {
    setErr("");
    exportCoursePlanPdf({
      selected,
      selectedPlanIndex,
      summary,
      insights,
      selectedInsight,
      selectedRank,
      aiLoading,
    });
  } catch (e) {
    setErr(e?.message || "Failed to export PDF.");
  }
};

  if (pageLoading) {
    return (
      <div className="cp-page">
        <div className="cp-container">
          <Skeleton width="14rem" height="2rem" className="mb-3" />
          <Skeleton width="26rem" height="1.25rem" className="mb-4" />
          <Skeleton height="10rem" className="mb-3" />
          <Skeleton height="18rem" className="mb-3" />
        </div>
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;
  if (!selected) return <div className="p-4">No plans generated.</div>;

  const semestersPlanned = selected.semesters?.length ?? 0;
  const creditsRemaining = selected.metrics?.creditsRemaining ?? 0;
  const coursesRemaining = selected.metrics?.coursesRemaining ?? 0;

  return (
    <div className="cp-page">
      <div className="cp-container">
        <PageHero
          title="Course Plan"
          badge={`${summary.programCode} • Semester ${summary.currentSemester}`}
          subtitle={"Your personalized academic plan to graduation"}
        />

        {/* AI generating banner - shown ONLY for AI */}
        {aiLoading && (
          <Card className="cp-banner-card">
            <div className="cp-banner">
              <ProgressSpinner
                style={{ width: "32px", height: "32px" }}
                strokeWidth="5"
              />
              <div className="cp-banner-text">
                <div className="cp-banner-title">Generating AI insights…</div>
                <div className="cp-banner-sub">
                  This may take a few seconds. Your plan is already available below.
                </div>
              </div>
            </div>
          </Card>
        )}

        {/* Everything inside this ref will be exported */}
        <div ref={exportRef} className="cp-export-area">
          {/* Roadmap header + KPI + plan selector */}
          <Card className="cp-header-card">
            <div className="cp-header-top">
              <div>
                <div className="cp-title">Academic Roadmap</div>
                <div className="cp-subtitle">
                  Personalized plan based on your progress and Computer Science
                  requirements
                </div>
                <div className="cp-strategy">
                  Strategy: <span>{selected.strategy}</span>
                </div>
              </div>

              <div className="cp-actions">
                <Button
                  icon="pi pi-refresh"
                  label="Regenerate AI"
                  outlined
                  onClick={loadInsights}
                  disabled={aiLoading}
                />
                <Button
                  icon="pi pi-download"
                  label="Export Plan"
                  severity="secondary"
                  onClick={exportToPdf}
                  disabled={!selected || pageLoading}
                />
              </div>
            </div>

            <div className="cp-kpi-grid">
              <Kpi title="Semesters Planned" value={semestersPlanned} icon="pi pi-calendar" />
              <Kpi title="Courses Remaining" value={coursesRemaining} icon="pi pi-check-circle" />
              <Kpi title="Credits Remaining" value={creditsRemaining} icon="pi pi-clock" />
              <Kpi
                title="Est. Graduation"
                value={selected.metrics?.estimatedGraduationTerm ?? "-"}
                icon="pi pi-info-circle"
              />
            </div>

            <div className="cp-plan-selector">
              {plans.map((p, idx) => {
                const active = idx === selectedPlanIndex;
                return (
                  <button
                    key={p.planId ?? idx}
                    className={`cp-pill ${active ? "is-active" : ""}`}
                    onClick={() => setSelectedPlanIndex(idx)}
                    type="button"
                  >
                    Plan {idx + 1}
                    {insights?.bestPlanIndex === idx ? (
                      <span className="cp-pill-dot" title="Recommended" />
                    ) : null}
                  </button>
                );
              })}
            </div>
          </Card>

          {/* AI Recommendation (show skeleton while AI loads) */}
          <Card className="cp-reco-card">
            {aiLoading ? (
              <div className="cp-ai-skel">
                <Skeleton width="12rem" height="1.25rem" className="mb-2" />
                <Skeleton width="28rem" height="1rem" />
              </div>
            ) : insights ? (
              <div className="cp-reco">
                <div>
                  <div className="cp-reco-title">AI Recommendation</div>
                  <div className="cp-reco-sub">{insights.bestPlanSummary}</div>
                </div>
                <Tag
                  value={`Recommended: Plan ${insights.bestPlanIndex + 1}`}
                  severity="success"
                />
              </div>
            ) : (
              <div className="cp-ai-empty">
                AI insights are not available yet.
              </div>
            )}
          </Card>

          {/* Selected plan insight */}
          <Card className="cp-insight-card">
            {aiLoading ? (
              <div className="cp-ai-skel">
                <Skeleton width="18rem" height="1.25rem" className="mb-2" />
                <Skeleton width="100%" height="1rem" className="mb-2" />
                <Skeleton width="80%" height="1rem" />
              </div>
            ) : selectedInsight ? (
              <>
                <div className="cp-insight-tags">
                  {selectedRank && <Tag value={`Rank #${selectedRank}`} severity="info" />}
                  <Tag value={`Score ${selectedInsight.score}/100`} severity="info" />
                  <Tag value={`Plan ${selectedPlanIndex + 1}`} />
                </div>

                <div className="cp-insight-expl">{selectedInsight.explanation}</div>

                <div className="cp-insight-cols">
                  <InsightList title="Pros" items={selectedInsight.pros} />
                  <InsightList title="Cons" items={selectedInsight.cons} />
                  <InsightList
                    title="Warnings"
                    items={
                      selectedInsight.warnings?.length ? selectedInsight.warnings : ["None"]
                    }
                  />
                </div>
              </>
            ) : (
              <div className="cp-ai-empty">
                No AI insight for this plan yet.
              </div>
            )}
          </Card>

          {/* semesters */}
          <div className="cp-semesters">
            {selected.semesters?.map((sem, sIdx) => (
              <Card
                key={`${sem.plannedSemester}-${sem.termLabel}-${sIdx}`}
                className="cp-sem-card"
              >
                <div className="cp-sem-head">
                  <div className="cp-sem-left">
                    <div className="cp-sem-icon">
                      <i className="pi pi-calendar" />
                    </div>
                    <div>
                      <div className="cp-sem-title">
                        Semester {sem.plannedSemester}: {sem.termLabel}
                      </div>
                      <div className="cp-sem-sub">
                        {sem.coursesCount} course(s) • {sem.totalCredits} credits
                      </div>
                    </div>
                  </div>

                  <Button label="Next Semester" size="small" outlined disabled />
                </div>

                <div className="cp-course-list">
                  {sem.courses?.map((c, cIdx) => (
                    <div
                      key={`${c.courseCode}-${cIdx}`}
                      className={`cp-course-row ${
                        c.prereqsSatisfiedBeforeThisSemester ? "" : "is-blocked"
                      }`}
                    >
                      <div className="cp-course-main">
                        <div className="cp-course-name">
                          {c.courseCode} — {c.courseName}
                          {c.isRetake ? <span className="cp-retake">(Retake)</span> : null}
                        </div>
                        <div className="cp-course-meta">
                          <span className="cp-meta-label">Prerequisites:</span>{" "}
                          {c.prerequisites?.length ? c.prerequisites.join(", ") : "None"}
                        </div>
                      </div>

                      <div className="cp-course-tags">
                        <Tag value={`${c.credits} credits`} />
                        <Tag
                          severity={c.prereqsSatisfiedBeforeThisSemester ? "success" : "danger"}
                          value={c.prereqsSatisfiedBeforeThisSemester ? "eligible" : "blocked"}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

function Kpi({ title, value, icon }) {
  return (
    <div className="cp-kpi">
      <div className="cp-kpi-icon">
        <i className={icon} />
      </div>
      <div className="cp-kpi-value">{value}</div>
      <div className="cp-kpi-title">{title}</div>
    </div>
  );
}

function InsightList({ title, items }) {
  return (
    <div className="cp-insight-col">
      <div className="cp-insight-col-title">{title}</div>
      <ul className="cp-insight-list">
        {items?.map((x, i) => (
          <li key={i}>{x}</li>
        ))}
      </ul>
    </div>
  );
}
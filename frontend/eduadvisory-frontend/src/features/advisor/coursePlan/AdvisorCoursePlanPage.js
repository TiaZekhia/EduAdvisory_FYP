import { useEffect, useMemo, useRef, useState } from "react";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { ProgressSpinner } from "primereact/progressspinner";

import StudentSelector from "../../../shared/components/StudentSelector";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import { PageHero } from "../../../shared/components/PageHero";
import { advisorCoursePlanApi } from "../../../services/advisors/advisorCoursePlanApi";
import { exportCoursePlanPdf } from "../../student/coursePlan/exportCoursePlanPdf";

import "../../student/coursePlan/CoursePlanPage.css";

const SEMESTERS_PER_PAGE = 3;

function SemesterPagination({ page, totalPages, setPage }) {
  if (totalPages <= 1) return null;
  return (
    <div className="d-flex justify-content-center align-items-center gap-1 mt-3 mb-2">
      <Button
        icon="pi pi-chevron-left"
        text
        size="small"
        disabled={page === 1}
        onClick={() => setPage(page - 1)}
      />
      {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
        <Button
          key={p}
          label={String(p)}
          size="small"
          outlined={page !== p}
          severity="secondary"
          onClick={() => setPage(p)}
          style={{ minWidth: "2rem", padding: "0.25rem 0.5rem" }}
        />
      ))}
      <Button
        icon="pi pi-chevron-right"
        text
        size="small"
        disabled={page === totalPages}
        onClick={() => setPage(page + 1)}
      />
    </div>
  );
}

export default function AdvisorCoursePlanPage() {
  const [students, setStudents] = useState([]);
  const [studentsLoading, setStudentsLoading] = useState(true);

  const [selectedStudent, setSelectedStudent] = useState(null);

  const [plans, setPlans] = useState([]);
  const [selectedPlanIndex, setSelectedPlanIndex] = useState(0);

  const [insights, setInsights] = useState(null);

  const [pageLoading, setPageLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);

  const [err, setErr] = useState("");

  const [semPage, setSemPage] = useState(1);

  const exportRef = useRef(null);

  const selected = useMemo(
    () => plans[selectedPlanIndex],
    [plans, selectedPlanIndex]
  );

  const selectedInsight = useMemo(() => {
    return insights?.planInsights?.find(
      (x) => x.planIndex === selectedPlanIndex
    );
  }, [insights, selectedPlanIndex]);

  const selectedRank = useMemo(() => {
    if (!insights?.planInsights?.length) return null;
    const ordered = [...insights.planInsights].sort((a, b) => b.score - a.score);
    const idx = ordered.findIndex((x) => x.planIndex === selectedPlanIndex);
    return idx >= 0 ? idx + 1 : null;
  }, [insights, selectedPlanIndex]);

  // Reset semester page when plan changes
  useEffect(() => {
    setSemPage(1);
  }, [selectedPlanIndex]);

  const pagedSemesters = useMemo(() => {
    const semesters = selected?.semesters ?? [];
    const start = (semPage - 1) * SEMESTERS_PER_PAGE;
    return semesters.slice(start, start + SEMESTERS_PER_PAGE);
  }, [selected, semPage]);

  const semTotalPages = useMemo(() => {
    const total = selected?.semesters?.length ?? 0;
    return Math.max(1, Math.ceil(total / SEMESTERS_PER_PAGE));
  }, [selected]);

  const selectedStudentId = selectedStudent?.studentId ?? null;
  const selectedStudentName = selectedStudent?.name ?? "Student";
  const selectedStudentSemester = selectedStudent?.currentSemester ?? "-";
  const selectedStudentAcademicStatus = selectedStudent?.academicStatus ?? "-";
  const selectedStudentProgramCode = selectedStudent?.programCode ?? "Program";

  useEffect(() => {
    setErr("");
    setStudentsLoading(true);

    advisorCoursePlanApi
      .getStudentsOverview()
      .then((res) => {
        const normalized = (res.data || []).map((s) => ({
          studentId: s.studentId ?? s.StudentId,
          name: s.name ?? s.Name,
          currentSemester: s.currentSemester ?? s.CurrentSemester,
          gpa: s.gpa ?? s.Gpa,
          academicStatus: s.academicStatus ?? s.AcademicStatus,
          programCode: s.programCode ?? s.ProgramCode,
        }));

        setStudents(normalized);
      })
      .catch((e) => {
        setErr(e?.response?.data ?? e?.message ?? "Failed to load students");
      })
      .finally(() => setStudentsLoading(false));
  }, []);

  const resetPlanState = () => {
    setPlans([]);
    setInsights(null);
    setSelectedPlanIndex(0);
    setPageLoading(false);
    setAiLoading(false);
    setSemPage(1);
  };

  const loadPlans = (studentId) => {
    if (!studentId) return;

    setErr("");
    setPageLoading(true);
    setPlans([]);
    setInsights(null);
    setSelectedPlanIndex(0);
    setSemPage(1);

    advisorCoursePlanApi
      .getPlans(studentId, 3)
      .then((res) => {
        const p = res.data || [];
        setPlans(p);
        setSelectedPlanIndex(0);
      })
      .catch((e) => {
        setErr(e?.response?.data ?? e?.message ?? "Failed to load plans");
      })
      .finally(() => setPageLoading(false));
  };

  const loadInsights = (studentId) => {
    if (!studentId) return;

    setErr("");
    setAiLoading(true);

    advisorCoursePlanApi
      .getInsights(studentId, 3)
      .then((res) => {
        const p = res.data?.plans || [];
        const ai = res.data?.insights || null;

        if (p?.length) setPlans(p);
        setInsights(ai);

        const best = ai?.bestPlanIndex;
        if (typeof best === "number") setSelectedPlanIndex(best);
      })
      .catch((e) => {
        setErr(
          e?.response?.data ?? e?.message ?? "Failed to load AI insights"
        );
      })
      .finally(() => setAiLoading(false));
  };

  const handleStudentChange = (studentId) => {
    const student = students.find(
      (s) => String(s.studentId) === String(studentId)
    );

    setSelectedStudent(student ?? null);
    resetPlanState();

    if (!student?.studentId) return;

    loadPlans(student.studentId);
  };

  useEffect(() => {
    if (!selectedStudentId) return;
    if (pageLoading) return;
    if (!plans.length) return;

    loadInsights(selectedStudentId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageLoading, selectedStudentId, plans.length]);

  const exportToPdf = () => {
    try {
      setErr("");

      const summaryForExport = {
        programCode: selectedStudentProgramCode,
        currentSemester: selectedStudentSemester,
      };

      exportCoursePlanPdf({
        selected,
        selectedPlanIndex,
        summary: summaryForExport,
        insights,
        selectedInsight,
        selectedRank,
        aiLoading,
      });
    } catch (e) {
      setErr(e?.message || "Failed to export PDF.");
    }
  };

  if (studentsLoading) {
    return (
      <div className="cp-page">
        <div className="cp-container">
          <Skeleton width="16rem" height="2rem" className="mb-3" />
          <Skeleton width="26rem" height="1.25rem" className="mb-4" />
          <Skeleton height="10rem" className="mb-3" />
        </div>
      </div>
    );
  }

  return (
    <div className="cp-page">
      <div className="cp-container">
        <PageHero
          title="Plan Generator"
          badge={
            selectedStudent
              ? `${selectedStudentName} • Semester ${selectedStudentSemester}`
              : "Select a student"
          }
          subtitle="Generate personalized course plans based on student status and study guide"
        />

        <PageSectionCard
          className="mb-3"
          title="Select Student"
          subtitle="Choose a student to generate their course plan"
        >
          <StudentSelector
            students={students}
            value={selectedStudentId ?? ""}
            onChange={handleStudentChange}
            loading={studentsLoading}
          />
        </PageSectionCard>

        {err ? <div className="p-3 text-danger">{String(err)}</div> : null}

        {!selectedStudent ? null : pageLoading ? (
          <>
            <Skeleton height="10rem" className="mb-3" />
            <Skeleton height="18rem" className="mb-3" />
          </>
        ) : !selected ? (
          <div className="p-4">No plans generated for this student.</div>
        ) : (
          <>
            {aiLoading && (
              <Card className="cp-banner-card mb-3">
                <div className="cp-banner">
                  <ProgressSpinner
                    style={{ width: "32px", height: "32px" }}
                    strokeWidth="5"
                  />
                  <div className="cp-banner-text">
                    <div className="cp-banner-title">Generating AI insights…</div>
                    <div className="cp-banner-sub">
                      This may take a few seconds. The plan is already available below.
                    </div>
                  </div>
                </div>
              </Card>
            )}

            <div ref={exportRef} className="cp-export-area">
              <Card className="cp-header-card mb-3">
                <div className="cp-header-top">
                  <div>
                    <div className="cp-title">Academic Roadmap</div>
                    <div className="cp-subtitle">
                      Personalized plan for {selectedStudentName} based on progress
                      and program requirements
                    </div>
                    <div className="cp-strategy">
                      Strategy: <span>{selected.strategy}</span>
                    </div>
                    <div className="cp-subtitle" style={{ marginTop: "0.35rem" }}>
                      Status: {selectedStudentAcademicStatus}
                    </div>
                  </div>

                  <div className="cp-actions">
                    <Button
                      icon="pi pi-refresh"
                      label="Regenerate AI"
                      outlined
                      onClick={() => loadInsights(selectedStudentId)}
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
                  <Kpi
                    title="Semesters Planned"
                    value={selected.semesters?.length ?? 0}
                    icon="pi pi-calendar"
                  />
                  <Kpi
                    title="Courses Remaining"
                    value={selected.metrics?.coursesRemaining ?? 0}
                    icon="pi pi-check-circle"
                  />
                  <Kpi
                    title="Credits Remaining"
                    value={selected.metrics?.creditsRemaining ?? 0}
                    icon="pi pi-clock"
                  />
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

              <Card className="cp-reco-card mb-3">
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

              <Card className="cp-insight-card mb-3">
                {aiLoading ? (
                  <div className="cp-ai-skel">
                    <Skeleton width="18rem" height="1.25rem" className="mb-2" />
                    <Skeleton width="100%" height="1rem" className="mb-2" />
                    <Skeleton width="80%" height="1rem" />
                  </div>
                ) : selectedInsight ? (
                  <>
                    <div className="cp-insight-tags">
                      {selectedRank && (
                        <Tag value={`Rank #${selectedRank}`} severity="info" />
                      )}
                      <Tag
                        value={`Score ${selectedInsight.score}/100`}
                        severity="info"
                      />
                      <Tag value={`Plan ${selectedPlanIndex + 1}`} />
                    </div>

                    <div className="cp-insight-expl">
                      {selectedInsight.explanation}
                    </div>

                    <div className="cp-insight-cols">
                      <InsightList title="Pros" items={selectedInsight.pros} />
                      <InsightList title="Cons" items={selectedInsight.cons} />
                      <InsightList
                        title="Warnings"
                        items={
                          selectedInsight.warnings?.length
                            ? selectedInsight.warnings
                            : ["None"]
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

              <div className="cp-semesters">
                {/* Semester pagination info */}
                {(selected.semesters?.length ?? 0) > SEMESTERS_PER_PAGE && (
                  <div className="d-flex justify-content-between align-items-center mb-2 px-1">
                    <span className="text-muted small">
                      Showing semesters {(semPage - 1) * SEMESTERS_PER_PAGE + 1}–
                      {Math.min(semPage * SEMESTERS_PER_PAGE, selected.semesters.length)} of{" "}
                      {selected.semesters.length}
                    </span>
                  </div>
                )}

                {pagedSemesters.map((sem, sIdx) => (
                  <Card
                    key={`${sem.plannedSemester}-${sem.termLabel}-${sIdx}`}
                    className="cp-sem-card mb-3"
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
                              {c.isRetake ? (
                                <span className="cp-retake">(Retake)</span>
                              ) : null}
                            </div>
                            <div className="cp-course-meta">
                              <span className="cp-meta-label">Prerequisites:</span>{" "}
                              {c.prerequisites?.length
                                ? c.prerequisites.join(", ")
                                : "None"}
                            </div>
                          </div>

                          <div className="cp-course-tags">
                            <Tag value={`${c.credits} credits`} />
                            <Tag
                              severity={
                                c.prereqsSatisfiedBeforeThisSemester
                                  ? "success"
                                  : "danger"
                              }
                              value={
                                c.prereqsSatisfiedBeforeThisSemester
                                  ? "eligible"
                                  : "blocked"
                              }
                            />
                          </div>
                        </div>
                      ))}
                    </div>
                  </Card>
                ))}

                <SemesterPagination
                  page={semPage}
                  totalPages={semTotalPages}
                  setPage={setSemPage}
                />
              </div>
            </div>
          </>
        )}
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
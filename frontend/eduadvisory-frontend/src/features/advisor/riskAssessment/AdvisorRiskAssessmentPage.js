import { useEffect, useMemo, useState } from "react";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { ProgressBar } from "primereact/progressbar";

import { PageHero } from "../../../shared/components/PageHero";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import StudentSelector from "../../../shared/components/StudentSelector";
import EmptyStateCard from "../../../shared/components/EmptyStateCard";
import { advisorRiskAssessmentApi } from "../../../services/advisors/advisorRiskAssessmentApi";

import "./advisorRiskAssessmentPage.css";

function tagSeverity(level) {
  if (level === "HIGH") return "danger";
  if (level === "MEDIUM") return "warning";
  return "success";
}

function statCardClass(type) {
  if (type === "high") return "risk-stat-card risk-high";
  if (type === "medium") return "risk-stat-card risk-medium";
  return "risk-stat-card risk-low";
}

function scoreBarClass(level) {
  if (level === "HIGH") return "risk-progress risk-progress-high";
  if (level === "MEDIUM") return "risk-progress risk-progress-medium";
  return "risk-progress risk-progress-low";
}

export default function AdvisorRiskAssessmentPage() {
  const [students, setStudents] = useState([]);
  const [studentsLoading, setStudentsLoading] = useState(true);

  // IMPORTANT: same pattern as AdvisorStudentAnalysisPage
  const [selectedStudentId, setSelectedStudentId] = useState("");

  const [risk, setRisk] = useState(null);
  const [loadingRisk, setLoadingRisk] = useState(false);
  const [err, setErr] = useState("");

  const selectedStudent = useMemo(() => {
    return students.find(
      (s) => String(s.studentId) === String(selectedStudentId)
    ) || null;
  }, [students, selectedStudentId]);

  useEffect(() => {
    setStudentsLoading(true);
    setErr("");

    advisorRiskAssessmentApi
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
        setErr(e?.response?.data ?? e?.message ?? "Failed to load students.");
      })
      .finally(() => setStudentsLoading(false));
  }, []);

  useEffect(() => {
    if (!selectedStudentId) {
      setRisk(null);
      return;
    }

    const loadRisk = async () => {
      try {
        setLoadingRisk(true);
        setErr("");
        setRisk(null);

        const res = await advisorRiskAssessmentApi.getStudentRiskAssessment(
          selectedStudentId
        );

        setRisk(res.data);
      } catch (e) {
        setErr(
          e?.response?.data ?? e?.message ?? "Failed to load risk assessment."
        );
      } finally {
        setLoadingRisk(false);
      }
    };

    loadRisk();
  }, [selectedStudentId]);

  const actionMessage = useMemo(() => {
    if (!risk) return "";
    if ((risk.courseSummary?.high ?? 0) > 0) {
      return `Student has ${risk.courseSummary.high} course(s) at high risk. Immediate intervention recommended.`;
    }
    if ((risk.globalAlertsSummary?.high ?? 0) > 0) {
      return `Student has ${risk.globalAlertsSummary.high} high-priority academic alert(s).`;
    }
    if (risk.riskLevel === "HIGH") {
      return "This student is currently classified as high risk.";
    }
    return "";
  }, [risk]);

  return (
    <div className="advisor-risk-page">
      <div className="advisor-risk-container">
        <PageHero
          title="Risk Assessment"
          subtitle="Analyze student performance to identify risk of failing courses"
          badge={
            selectedStudent
              ? `${selectedStudent.name} • GPA ${selectedStudent.gpa ?? "-"}`
              : "Select a student"
          }
        />

        <PageSectionCard
          title="Select Student"
          subtitle="Choose a student to assess their risk levels"
          className="mb-4"
        >
          <StudentSelector
            students={students}
            value={selectedStudentId}
            onChange={setSelectedStudentId}
            loading={studentsLoading}
          />
        </PageSectionCard>

        {err ? <div className="advisor-risk-error">{String(err)}</div> : null}

        {!selectedStudentId ? null : loadingRisk ? (
          <div className="advisor-risk-loading">
            <Skeleton height="7rem" className="mb-3" />
            <Skeleton height="12rem" className="mb-3" />
            <Skeleton height="16rem" className="mb-3" />
          </div>
        ) : !risk ? (
          <EmptyStateCard
            title="No risk assessment available for this student"
            icon="pi pi-chart-line"
          />
        ) : (
          <>
            <PageSectionCard
              title={`Risk Summary for ${risk.studentName}`}
              className="mb-4"
            >
              <div className="risk-summary-grid">
                <div className={statCardClass("high")}>
                  <div className="risk-stat-value">{risk.courseSummary?.high ?? 0}</div>
                  <div className="risk-stat-label">High Risk</div>
                </div>

                <div className={statCardClass("medium")}>
                  <div className="risk-stat-value">{risk.courseSummary?.medium ?? 0}</div>
                  <div className="risk-stat-label">Medium Risk</div>
                </div>

                <div className={statCardClass("low")}>
                  <div className="risk-stat-value">{risk.courseSummary?.low ?? 0}</div>
                  <div className="risk-stat-label">Low Risk</div>
                </div>
              </div>
            </PageSectionCard>

            {actionMessage ? (
              <div className="advisor-risk-banner">
                <div className="advisor-risk-banner-title">Action Required:</div>
                <div>{actionMessage}</div>
              </div>
            ) : null}

            <PageSectionCard
              title="Overall Student Risk"
              subtitle="Student-level academic and progression risk indicators"
              className="mb-4"
            >
              <div className="overall-risk-head">
                <div>
                  <div className="overall-risk-title">{risk.studentName}</div>
                  <div className="overall-risk-sub">
                    Program {risk.programCode} • Semester {risk.currentSemester}
                  </div>
                </div>

                <Tag
                  value={`${risk.riskLevel} RISK`}
                  severity={tagSeverity(risk.riskLevel)}
                />
              </div>

              <div className="overall-risk-score-row">
                <span>Overall Risk Score</span>
                <span>{risk.riskScore}/100</span>
              </div>

              <ProgressBar
                value={risk.riskScore}
                showValue={false}
                className={scoreBarClass(risk.riskLevel)}
              />

              <div className="overall-reasons">
                <div className="section-mini-title">Main Reasons</div>
                <ul>
                  {(risk.mainReasons || []).map((reason, idx) => (
                    <li key={idx}>{reason}</li>
                  ))}
                </ul>
              </div>

              <div className="overall-factors-grid">
                <FactorBox title="Course Portfolio" value={risk.factors?.coursePortfolioRiskScore} />
                <FactorBox title="GPA" value={risk.factors?.gpaRiskScore} />
                <FactorBox title="Academic Status" value={risk.factors?.academicStatusRiskScore} />
                <FactorBox title="Credit Delay" value={risk.factors?.creditDelayRiskScore} />
                <FactorBox
                  title="Current Semester Missing"
                  value={risk.factors?.currentSemesterMissingRiskScore}
                />
              </div>

              <div className="overall-factors-meta">
                <div>Current GPA: {risk.factors?.currentGpa ?? "-"}</div>
                <div>Academic Status: {risk.factors?.academicStatus ?? "-"}</div>
                <div>Expected Credits By Now: {risk.factors?.expectedCreditsByNow ?? 0}</div>
                <div>Earned Credits: {risk.factors?.earnedCredits ?? 0}</div>
                <div>Delayed Credits: {risk.factors?.delayedCredits ?? 0}</div>
                <div>
                  Missing Current Semester Courses:{" "}
                  {risk.factors?.currentSemesterMissingCoursesCount ?? 0}
                </div>
              </div>

              <div className="overall-recommendation">
                <strong>Recommendation:</strong> {risk.recommendedAction}
              </div>
            </PageSectionCard>

            <PageSectionCard
              title="Global Alerts"
              subtitle="Only non-course-specific academic alerts are shown here"
              className="mb-4"
            >
              {!risk.globalAlertsSummary?.alerts?.length ? (
                <EmptyStateCard
                  title="No global alerts"
                  icon="pi pi-check-circle"
                />
              ) : (
                <div className="risk-alerts-list">
                  {risk.globalAlertsSummary.alerts.map((alert, idx) => (
                    <div key={idx} className="risk-alert-item">
                      <div>
                        <div className="risk-alert-title">{alert.title}</div>
                        <div className="risk-alert-message">{alert.message}</div>
                      </div>
                      <Tag
                        value={alert.severity}
                        severity={tagSeverity(alert.severity)}
                      />
                    </div>
                  ))}
                </div>
              )}
            </PageSectionCard>

            <div className="course-risk-list">
              {(risk.courseAssessments || []).map((course, idx) => (
                <PageSectionCard
                  key={`${course.courseCode}-${idx}`}
                  className="mb-4"
                >
                  <div className="course-risk-head">
                    <div>
                      <div className="course-risk-title">
                        {course.courseCode} - {course.courseName}
                      </div>
                      <div className="course-risk-sub">
                        Current semester performance analysis
                      </div>
                    </div>

                    <Tag
                      value={`${course.riskLevel} RISK`}
                      severity={tagSeverity(course.riskLevel)}
                    />
                  </div>

                  <div className="course-risk-score-row">
                    <span>Risk Score</span>
                    <span>{course.riskScore}/100</span>
                  </div>

                  <ProgressBar
                    value={course.riskScore}
                    showValue={false}
                    className={scoreBarClass(course.riskLevel)}
                  />

                  <div className="section-mini-title">Risk Factors</div>

                  {!course.riskFactors?.length ? (
                    <div className="course-risk-empty">
                      No significant risk factors detected.
                    </div>
                  ) : (
                    <div className="course-factor-list">
                      {course.riskFactors.map((factor, factorIdx) => (
                        <div key={factorIdx} className="course-factor-item">
                          <div>
                            <div className="course-factor-title">{factor.title}</div>
                            <div className="course-factor-detail">{factor.detail}</div>
                          </div>

                          <Tag
                            value={factor.severity}
                            severity={tagSeverity(factor.severity)}
                          />
                        </div>
                      ))}
                    </div>
                  )}

                  <div className="section-mini-title">Components</div>
                  <div className="course-components-list">
                    {(course.components || []).map((comp, compIdx) => (
                      <div key={compIdx} className="course-component-item">
                        <span>{comp.componentName}</span>
                        <span>
                          Grade: {comp.grade ?? "-"} • Weight: {comp.weightPercentage}% • Risk:{" "}
                          {comp.riskScore}/100
                        </span>
                      </div>
                    ))}
                  </div>

                  <div className="course-recommendation">
                    <strong>Recommendation:</strong> {course.recommendation}
                  </div>
                </PageSectionCard>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function FactorBox({ title, value }) {
  return (
    <div className="factor-box">
      <div className="factor-box-value">{value ?? 0}</div>
      <div className="factor-box-title">{title}</div>
    </div>
  );
}
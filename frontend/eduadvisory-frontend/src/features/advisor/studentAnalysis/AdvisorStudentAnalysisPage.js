import { useEffect, useState } from "react";
import { PageHero } from "../../../shared/components/PageHero";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import EmptyStateCard from "../../../shared/components/EmptyStateCard";
import { Loading } from "../../../shared/components/Loading";
import StudentSelector from "../../../shared/components/StudentSelector";

import {
  getAdvisorStudentsOverview,
  getAdvisorStudentAnalysis,
} from "../../../services/advisors/advisorStudentAnalysisApi";

export default function AdvisorStudentAnalysisPage() {
  const [students, setStudents] = useState([]);
  const [selectedStudentId, setSelectedStudentId] = useState("");
  const [analysis, setAnalysis] = useState(null);
  const [loadingStudents, setLoadingStudents] = useState(true);
  const [loadingAnalysis, setLoadingAnalysis] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadStudents = async () => {
      try {
        setLoadingStudents(true);
        setError("");
        const data = await getAdvisorStudentsOverview();
        setStudents(data || []);
      } catch (err) {
        setError("Failed to load students.");
      } finally {
        setLoadingStudents(false);
      }
    };

    loadStudents();
  }, []);

  useEffect(() => {
    if (!selectedStudentId) {
      setAnalysis(null);
      return;
    }

    const loadAnalysis = async () => {
      try {
        setLoadingAnalysis(true);
        setError("");
        const data = await getAdvisorStudentAnalysis(selectedStudentId);
        setAnalysis(data);
      } catch (err) {
        setError("Failed to load student analysis.");
      } finally {
        setLoadingAnalysis(false);
      }
    };

    loadAnalysis();
  }, [selectedStudentId]);

  return (
    <div className="p-4">
      <PageHero
        title="Student Analysis"
        subtitle="Analyze student progress and compare with study guide requirements"
      />

      <PageSectionCard
        title="Select Student"
        subtitle="Choose a student to analyze their academic progress"
      >
        <StudentSelector
          students={students}
          value={selectedStudentId}
          onChange={setSelectedStudentId}
          loading={loadingStudents}
        />
      </PageSectionCard>

      {error ? (
        <div className="alert alert-danger mt-3">{error}</div>
      ) : null}

      {loadingAnalysis ? <Loading text="Analyzing student..." /> : null}

      {!loadingAnalysis && analysis && (
        <>
          <PageSectionCard>
            <div className="fw-bold fs-5">{analysis.studentName}</div>
            <div className="mt-2">
              is{" "}
              <span
                className={`fw-bold ${
                  analysis.isOnTrack ? "text-success" : "text-danger"
                }`}
              >
                {analysis.isOnTrack ? "ON TRACK" : "NOT ON TRACK"}
              </span>
            </div>
          </PageSectionCard>

          <PageSectionCard title="Progress Overview">
            <div className="row g-3">
              <div className="col-12 col-md-3">
                <strong>Current Semester</strong>
                <div>Semester {analysis.currentSemester}</div>
              </div>

              <div className="col-12 col-md-3">
                <strong>Completed Credits</strong>
                <div>
                  {analysis.completedCredits}/{analysis.totalProgramCredits}
                </div>
              </div>

              <div className="col-12 col-md-3">
                <strong>GPA</strong>
                <div>{analysis.currentGpa}</div>
              </div>

              <div className="col-12 col-md-3">
                <strong>Status</strong>
                <div>
                  {analysis.academicStatus === "PROBATION"
                    ? "Probation"
                    : "Good Standing"}
                </div>
              </div>
            </div>
          </PageSectionCard>

          <PageSectionCard
            title="Current Semester Enrollment"
            subtitle="Courses enrolled for this semester"
          >
            {analysis.currentEnrollment?.length === 0 ? (
              <EmptyStateCard
                icon="pi pi-book"
                title="No enrolled courses"
                text="This student is not enrolled in any courses this semester."
              />
            ) : (
              <div className="d-flex flex-column gap-3">
                {analysis.currentEnrollment.map((course) => (
                  <div
                    key={course.courseCode}
                    className="border rounded-4 p-3 d-flex justify-content-between align-items-center"
                  >
                    <div>
                      <div className="fw-semibold">
                        {course.courseCode} - {course.courseName}
                      </div>
                      <div className="text-muted small">
                        {course.credits} credits
                      </div>
                    </div>

                    <span className="badge text-bg-light">{course.status}</span>
                  </div>
                ))}
              </div>
            )}
          </PageSectionCard>

          <PageSectionCard
            title="Failed Courses"
            subtitle="Courses that need attention"
          >
            {analysis.failedCourses?.length === 0 ? (
              <EmptyStateCard
                icon="pi pi-check-circle"
                title="No failed courses"
                text="This student does not currently have failed courses requiring attention."
              />
            ) : (
              <div className="d-flex flex-column gap-3">
                {analysis.failedCourses.map((course) => (
                  <div
                    key={course.courseCode}
                    className="border border-danger-subtle bg-danger-subtle rounded-4 p-3 d-flex justify-content-between align-items-center"
                  >
                    <div>
                      <div className="fw-semibold">
                        {course.courseCode} - {course.courseName}
                      </div>
                      <div className="text-muted small">
                        Failed in {course.semester || "previous semester"}
                      </div>
                    </div>

                    <span
                      className={`badge ${
                        course.retakeStatus === "NOT_RETAKEN"
                          ? "text-bg-danger"
                          : "text-bg-warning"
                      }`}
                    >
                      {course.retakeStatus.replaceAll("_", " ")}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </PageSectionCard>

          <PageSectionCard
            title="Missing Courses"
            subtitle="Courses that should have been taken"
          >
            {analysis.missingCourses?.length === 0 ? (
              <EmptyStateCard
                icon="pi pi-check"
                title="No missing courses"
                text="This student is currently aligned with the study guide."
              />
            ) : (
              <div className="d-flex flex-column gap-3">
                {analysis.missingCourses.map((course) => (
                  <div
                    key={course.courseCode}
                    className="border border-warning-subtle bg-warning-subtle rounded-4 p-3"
                  >
                    <div className="d-flex justify-content-between align-items-start gap-3 flex-wrap">
                      <div>
                        <div className="fw-semibold">
                          {course.courseCode} - {course.courseName}
                        </div>
                        <div className="text-muted small mt-1">
                          {course.reason}
                        </div>
                        {course.prerequisites?.length > 0 && (
                          <div className="text-muted small mt-1">
                            Prerequisites: {course.prerequisites.join(", ")}
                          </div>
                        )}
                      </div>

                      <span
                        className={`badge ${
                          course.priority === "HIGH"
                            ? "text-bg-danger"
                            : course.priority === "MEDIUM"
                            ? "text-bg-dark"
                            : "text-bg-secondary"
                        }`}
                      >
                        {course.priority} PRIORITY
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </PageSectionCard>
        </>
      )}
    </div>
  );
}
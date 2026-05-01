import apiClient from "../apiClient";

export const advisorRiskAssessmentApi = {
  getStudentsOverview: () =>
    apiClient.get("/advisors/me/students-overview"),

  getStudentRiskAssessment: (studentId) =>
    apiClient.get(`/advisors/me/students/${studentId}/risk-assessment`),
};
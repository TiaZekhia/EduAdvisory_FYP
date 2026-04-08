import apiClient from "../apiClient";

export const advisorCoursePlanApi = {
  getStudentsOverview: () =>
    apiClient.get("/advisors/me/students-overview"),

  getPlans: (studentId, count = 3) =>
    apiClient.get(`/advisors/me/students/${studentId}/course-plan/plans?count=${count}`),

  getInsights: (studentId, count = 3) =>
    apiClient.get(`/advisors/me/students/${studentId}/course-plan/insights?count=${count}`),
};
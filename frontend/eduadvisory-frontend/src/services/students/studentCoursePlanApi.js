import apiClient from "../apiClient";

export const studentCoursePlanApi = {
  getPlans: (count = 3) =>
    apiClient.get(`/students/me/course-plan/plans?count=${count}`),

  getSummary: () =>
    apiClient.get(`/students/me/course-plan/summary`),

  exportPlan: (planId) =>
    apiClient.get(`/students/me/course-plan/export?planId=${planId}`, {
      responseType: "blob",
    }),
};
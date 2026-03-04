import apiClient from "../apiClient";

export const studentCoursePlanApi = {
  getPlans: (count = 3) =>
    apiClient.get(`/students/me/course-plan/plans?count=${count}`),

  getInsights: (count = 3) =>
    apiClient.get(`/students/me/course-plan/insights?count=${count}`),
};
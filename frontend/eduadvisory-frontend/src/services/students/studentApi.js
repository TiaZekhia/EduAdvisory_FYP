import apiClient from "../apiClient";

export const studentApi = {
  getMySummary: () => apiClient.get("/students/me/summary"),
  getMyCurrentEnrollment: () => apiClient.get("/students/me/current-enrollment"),
  getMyCurrentCoursesPerformance: () =>
    apiClient.get("/students/me/current-courses/performance"),
  getMyAnalysis: () => apiClient.get("/StudentAnalysis/me"),
  getMyStats: () => apiClient.get("/students/me/stats"),
  getMyDegreeProgress: () => apiClient.get("/students/me/degree-progress"),
  getMyUpcomingMeetingsCount: () => apiClient.get("/students/me/meetings/upcoming/count"),
};
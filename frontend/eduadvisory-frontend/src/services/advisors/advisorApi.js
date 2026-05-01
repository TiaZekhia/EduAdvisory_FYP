import apiClient from "../apiClient";

export const advisorApi = {
  getMySummary: () => apiClient.get("/Advisors/me/summary"),
  getDashboardSummary: () => apiClient.get("/Advisors/me/dashboard/summary"),
  getUpcomingMeetings: (limit = 5) =>
    apiClient.get(`/Advisors/me/meetings/upcoming?limit=${limit}`),
  getPastMeetings: (limit = 10) =>
    apiClient.get(`/Advisors/me/meetings/past?limit=${limit}`),
  getMessagesSummary: () => apiClient.get("/Advisors/me/messages/summary"),
  getMessages: (limit = 20) =>
    apiClient.get(`/Advisors/me/messages?limit=${limit}`),

  getStudentsOverview: () => apiClient.get("/Advisors/me/students-overview"),
  getRecentActivity: (limit = 10) =>
    apiClient.get(`/Advisors/me/recent-activity?limit=${limit}`),
};
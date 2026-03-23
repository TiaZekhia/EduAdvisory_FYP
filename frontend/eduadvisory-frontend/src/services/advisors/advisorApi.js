import apiClient from "../apiClient";

export const advisorApi = {
  getMySummary: () => apiClient.get("/advisors/me/summary"),
  getDashboardSummary: () => apiClient.get("/advisors/me/dashboard/summary"),
  getUpcomingMeetings: (limit = 5) =>
    apiClient.get(`/advisors/me/meetings/upcoming?limit=${limit}`),
  getPastMeetings: (limit = 10) =>
    apiClient.get(`/advisors/me/meetings/past?limit=${limit}`),
  getMessagesSummary: () => apiClient.get("/advisors/me/messages/summary"),
  getMessages: (limit = 20) =>
    apiClient.get(`/advisors/me/messages?limit=${limit}`),
};
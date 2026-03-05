import apiClient from "../apiClient";

export const studentMeetingsApi = {
  getSummary: () => apiClient.get("students/me/meetings/summary"),
  getUpcoming: (limit = 3) => apiClient.get(`students/me/meetings/upcoming?limit=${limit}`),
  getPast: (limit = 10) => apiClient.get(`students/me/meetings/past?limit=${limit}`),
  getAdvisor: () => apiClient.get("students/me/advisor"),
};
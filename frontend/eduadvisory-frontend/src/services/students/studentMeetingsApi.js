import apiClient from "../apiClient";

// export const studentMeetingsApi = {
//   getSummary: () => apiClient.get("students/me/meetings/summary"),
//   getUpcoming: (limit = 3) => apiClient.get(`students/me/meetings/upcoming?limit=${limit}`),
//   getPast: (limit = 10) => apiClient.get(`students/me/meetings/past?limit=${limit}`),
//   getAdvisor: () => apiClient.get("students/me/advisor"),
// };

export const studentMeetingsApi = {
  getAdvisor: () => apiClient.get("/student-meetings/my/advisor"),
  getAdvisorAvailability: () => apiClient.get("/student-meetings/my/advisor-availability"),
  getRequests: () => apiClient.get("/student-meetings/my/requests"),
  createRequest: (payload) => apiClient.post("/student-meetings/my/requests", payload),
  cancelRequest: (id) => apiClient.delete(`/student-meetings/my/requests/${id}`),
  getUpcoming: () => apiClient.get("/student-meetings/my/upcoming"),
  getHistory: () => apiClient.get("/student-meetings/my/history"),
};
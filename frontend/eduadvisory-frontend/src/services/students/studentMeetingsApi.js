import apiClient from "../apiClient";

export const studentMeetingsApi = {
  getAdvisor: () => apiClient.get("/student-meetings/my/advisor"),
  getAdvisorCalendar: (date) =>
    apiClient.get(`/student-meetings/my/advisor-calendar?date=${date}`),

  getRequests: () => apiClient.get("/student-meetings/my/requests"),
  createRequest: (payload) => apiClient.post("/student-meetings/my/requests", payload),
  cancelRequest: (id) => apiClient.delete(`/student-meetings/my/requests/${id}`),

  getUpcoming: () => apiClient.get("/student-meetings/my/upcoming"),
  getHistory: () => apiClient.get("/student-meetings/my/history"),
};
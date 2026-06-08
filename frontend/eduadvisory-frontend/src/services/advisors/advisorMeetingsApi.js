import apiClient from "../apiClient";

export const advisorMeetingsApi = {
  getWeeklyAvailability: () => apiClient.get("/advisor-meetings/weekly-availability"),
  createWeeklyAvailability: (payload) => apiClient.post("/advisor-meetings/weekly-availability", payload),
  deleteWeeklyAvailability: (id) => apiClient.delete(`/advisor-meetings/weekly-availability/${id}`),

  getPendingRequests: () => apiClient.get("/advisor-meetings/requests/pending"),
  respondToRequest: (id, payload) =>
    apiClient.post(`/advisor-meetings/requests/${id}/respond`, payload),

  getUpcoming: () => apiClient.get("/advisor-meetings/upcoming"),
  getHistory: () => apiClient.get("/advisor-meetings/history"),

  getExceptions: () => apiClient.get("/advisor-meetings/exceptions"),
  addException: (payload) => apiClient.post("/advisor-meetings/exceptions", payload),
  removeException: (id) => apiClient.delete(`/advisor-meetings/exceptions/${id}`),
};
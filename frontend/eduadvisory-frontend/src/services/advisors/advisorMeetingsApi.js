import apiClient from "../apiClient";

export const advisorMeetingsApi = {
  getAvailability: () => apiClient.get("/advisor-meetings/availability"),
  createAvailability: (payload) => apiClient.post("/advisor-meetings/availability", payload),
  deleteAvailability: (id) => apiClient.delete(`/advisor-meetings/availability/${id}`),

  getPendingRequests: () => apiClient.get("/advisor-meetings/requests/pending"),
  respondToRequest: (id, payload) =>
    apiClient.post(`/advisor-meetings/requests/${id}/respond`, payload),

  getUpcoming: () => apiClient.get("/advisor-meetings/upcoming"),
  getHistory: () => apiClient.get("/advisor-meetings/history"),
};
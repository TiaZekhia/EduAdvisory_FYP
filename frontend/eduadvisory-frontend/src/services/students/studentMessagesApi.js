import apiClient from "../apiClient";

export const studentMessagesApi = {
  getSummary: () => apiClient.get("/students/me/messages/summary"),
  getMessages: (limit = 20) => apiClient.get(`/students/me/messages?limit=${limit}`),
  getAdvisor: () => apiClient.get("students/me/advisor"),
};
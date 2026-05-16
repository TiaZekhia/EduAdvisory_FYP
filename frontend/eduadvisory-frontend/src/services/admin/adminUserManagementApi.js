import apiClient from "../apiClient";

export const adminUserManagementApi = {
  getUsers: () => apiClient.get("/admin/users"),
  getAvailableLinks: (role) =>
    apiClient.get(`/admin/users/available-links?role=${encodeURIComponent(role)}`),
  createUser: (payload) => apiClient.post("/admin/users", payload),
  updateUser: (userId, payload) => apiClient.put(`/admin/users/${userId}`, payload),
  deactivateUser: (userId) => apiClient.put(`/admin/users/${userId}/deactivate`),
  reactivateUser: (userId) => apiClient.put(`/admin/users/${userId}/reactivate`),
};
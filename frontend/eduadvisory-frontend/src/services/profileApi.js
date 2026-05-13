import apiClient from "./apiClient";

export const profileApi = {
  getMyProfile: () => apiClient.get("/profile/me"),
};

import apiClient from "../services/apiClient";

export const sharedGoogleAuthApi = {
  getStatus: () => apiClient.get("/shared-google-auth/status"),
  getConnectUrl: () => apiClient.get("/shared-google-auth/connect"),
  disconnect: () => apiClient.post("/shared-google-auth/disconnect"),
};
import axios from "axios";
import keycloak from "../keycloak";

const apiClient = axios.create({
  baseURL: `${process.env.REACT_APP_API_URL}/api`,
});

apiClient.interceptors.request.use(async (config) => {
  try {
    await keycloak.updateToken(5);
    if (keycloak.token) {
      config.headers.Authorization = `Bearer ${keycloak.token}`;
    }
  } catch (e) {
    console.error("Token refresh failed:", e);
    keycloak.login();
  }
  return config;
});

export default apiClient;
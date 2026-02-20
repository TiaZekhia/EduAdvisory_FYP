import axios from "axios";
import keycloak from "../keycloak";

const api = axios.create({
  baseURL: "http://localhost:5267/api",
});

api.interceptors.request.use(
  async (config) => {
    try {
      // Update the token before each request
      await keycloak.updateToken(5); // Refresh if expires in 5 seconds
      
      if (keycloak.token) {
        config.headers.Authorization = `Bearer ${keycloak.token}`;
        console.log("✅ Token attached to request");
        console.log("Token preview:", keycloak.token.substring(0, 50) + "...");
      } else {
        console.log("❌ No token available!");
      }
    } catch (error) {
      console.error("❌ Failed to refresh token:", error);
      keycloak.login();
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default api;
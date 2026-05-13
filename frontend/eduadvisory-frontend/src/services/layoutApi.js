import apiClient from "./apiClient";

export const layoutApi = {
  getUnreadMessagesCount: () => apiClient.get("/chat/unread-count"),
};

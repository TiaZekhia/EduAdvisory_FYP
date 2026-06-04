import apiClient from "../apiClient";

export const sendStudentAiMessage = async ({ message, sessionId, courseCode }) => {
  const response = await apiClient.post("/student-ai/chat", {
    message,
    sessionId,
    courseCode,
  });

  return response.data;
};
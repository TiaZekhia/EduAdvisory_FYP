import apiClient from "../apiClient";
import keycloak from "../../keycloak";

export const sendStudentAiMessage = async ({ message, sessionId, courseCode }) => {
  const response = await apiClient.post("/student-ai/chat", {
    message,
    sessionId,
    courseCode,
  });

  return response.data;
};

export const sendStudentAiMessageStream = async ({
  message,
  sessionId,
  courseCode,
  onToken,
  onMetadata,
  onError,
  onDone,
}) => {
  try {
    await keycloak.updateToken(5);
  } catch {
    keycloak.login();
    return;
  }

  const response = await fetch(
    `${process.env.REACT_APP_API_URL}/api/student-ai/chat/stream`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${keycloak.token}`,
      },
      body: JSON.stringify({ message, sessionId, courseCode }),
    }
  );

  if (!response.ok) {
    onError?.("Sorry, I could not stream the response right now.");
    return;
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    // SSE events are separated by double newline
    const events = buffer.split("\n\n");
    buffer = events.pop() ?? "";

    for (const eventBlock of events) {
      const lines = eventBlock.split("\n");
      let eventName = "";
      let dataStr = "";

      for (const line of lines) {
        if (line.startsWith("event: ")) {
          eventName = line.slice(7).trim();
        } else if (line.startsWith("data: ")) {
          dataStr = line.slice(6).trim();
        }
      }

      if (!eventName || !dataStr) continue;

      try {
        const data = JSON.parse(dataStr);
        if (eventName === "token") onToken?.(data.content);
        else if (eventName === "metadata") onMetadata?.(data);
        else if (eventName === "error") onError?.(data.message);
        else if (eventName === "done") onDone?.();
      } catch {
        // ignore malformed SSE data
      }
    }
  }
};
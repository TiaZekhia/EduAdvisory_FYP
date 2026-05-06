const API_BASE_URL = "http://localhost:5267/api";

function authHeaders(token) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

export async function getConversations(token) {
  const res = await fetch(`${API_BASE_URL}/chat/conversations`, {
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to load conversations");

  return res.json();
}

export async function startConversation(token, studentId) {
  const res = await fetch(`${API_BASE_URL}/chat/conversations/start`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify({ studentId }),
  });

  if (!res.ok) throw new Error("Failed to start conversation");

  return res.json();
}

export async function getMessages(token, conversationId) {
  const res = await fetch(`${API_BASE_URL}/chat/conversations/${conversationId}/messages`, {
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to load messages");

  return res.json();
}

export async function sendMessage(token, conversationId, content) {
  const res = await fetch(`${API_BASE_URL}/chat/messages`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify({
      conversationId,
      content,
    }),
  });

  if (!res.ok) throw new Error("Failed to send message");

  return res.json();
}

export async function markConversationAsRead(token, conversationId) {
  const res = await fetch(`${API_BASE_URL}/chat/conversations/${conversationId}/read`, {
    method: "PUT",
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to mark messages as read");
}

export async function startConversationWithMyAdvisor(token) {
  const res = await fetch(`${API_BASE_URL}/chat/conversations/my-advisor`, {
    method: "POST",
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to start conversation with advisor");

  return res.json();
}

export async function getAdvisorStudents(token) {
  const res = await fetch(`${API_BASE_URL}/chat/advisor/students`, {
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to load advisor students");

  return res.json();
}

export async function sendMessageWithFiles(token, conversationId, content, files) {
  const formData = new FormData();

  formData.append("ConversationId", conversationId);
  formData.append("Content", content || "");

  files
    .filter((file) => file && file.size > 0)
    .forEach((file) => {
      formData.append("Files", file);
    });

  const res = await fetch(`${API_BASE_URL}/chat/messages/with-files`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
  });

  if (!res.ok) throw new Error("Failed to send message with files");

  return res.json();
}

export async function editMessage(token, messageId, content) {
  const res = await fetch(`${API_BASE_URL}/chat/messages/${messageId}`, {
    method: "PUT",
    headers: authHeaders(token),
    body: JSON.stringify({ content }),
  });

  if (!res.ok) throw new Error("Failed to edit message");

  return res.json();
}

export async function deleteMessage(token, messageId) {
  const res = await fetch(`${API_BASE_URL}/chat/messages/${messageId}`, {
    method: "DELETE",
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to delete message");
}
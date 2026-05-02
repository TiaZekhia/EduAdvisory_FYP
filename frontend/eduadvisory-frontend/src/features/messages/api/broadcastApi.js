const API_BASE_URL = "http://localhost:5267/api";

function authHeaders(token) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

export async function getBroadcasts(token) {
  const res = await fetch(`${API_BASE_URL}/broadcasts`, {
    headers: authHeaders(token),
  });

  if (!res.ok) throw new Error("Failed to load broadcasts");

  return res.json();
}

export async function createBroadcast(token, title, content, studentIds = []) {
  const res = await fetch(`${API_BASE_URL}/broadcasts`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify({
      title,
      content,
      studentIds,
    }),
  });

  if (!res.ok) throw new Error("Failed to create broadcast");

  return res.json();
}

export async function markBroadcastAsRead(token, broadcastMessageId) {
  const res = await fetch(
    `${API_BASE_URL}/broadcasts/${broadcastMessageId}/read`,
    {
      method: "PUT",
      headers: authHeaders(token),
    }
  );

  if (!res.ok) throw new Error("Failed to mark broadcast as read");
}
import * as signalR from "@microsoft/signalr";

let connection = null;

export function createChatConnection(token) {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5267/hubs/chat", {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

export function getChatConnection() {
  return connection;
}

export async function startChatConnection(token) {
  if (!connection) {
    connection = createChatConnection(token);
  }

  if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start();
    console.log("SignalR connected");
  }

  return connection;
}

export async function stopChatConnection() {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}
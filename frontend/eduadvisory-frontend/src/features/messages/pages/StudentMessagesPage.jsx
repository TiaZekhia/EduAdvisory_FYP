import { useEffect, useState } from "react";
import ConversationList from "../components/ConversationList";
import ChatWindow from "../components/ChatWindow";
import BroadcastInbox from "../components/BroadcastInbox";
import {
  getConversations,
  getMessages,
  markConversationAsRead,
  startConversationWithMyAdvisor,
} from "../api/messageApi";
import { startChatConnection } from "../signalr/chatConnection";
import { getAccessToken } from "../../../auth/getAccessToken";

export default function StudentMessagesPage() {
  const [token, setToken] = useState(null);
  const [conversations, setConversations] = useState([]);
  const [selectedConversation, setSelectedConversation] = useState(null);
  const [selectedConversationRef, setSelectedConversationRef] = useState(null);
  const [messages, setMessages] = useState([]);
  const [liveBroadcast, setLiveBroadcast] = useState(null);
  const [loading, setLoading] = useState(true);
  const [broadcastOpen, setBroadcastOpen] = useState(false);
  const [unreadBroadcasts, setUnreadBroadcasts] = useState(0);

  useEffect(() => {
    getAccessToken()
      .then((accessToken) => {
        setToken(accessToken);
        initializeStudentMessages(accessToken);
        connectSignalR(accessToken);
      })
      .catch((err) => console.error("Could not get token", err));
  }, []);

  async function initializeStudentMessages(accessToken) {
    try {
      setLoading(true);
      let data = await getConversations(accessToken);
      if (data.length === 0) {
        const conversation = await startConversationWithMyAdvisor(accessToken);
        data = [conversation];
      }
      setConversations(data);
      await handleSelectConversation(data[0], accessToken);
    } catch (err) {
      console.error(err);
      alert("Could not open chat with advisor.");
    } finally {
      setLoading(false);
    }
  }

  async function connectSignalR(accessToken) {
    try {
      const connection = await startChatConnection(accessToken);

      connection.off("ReceiveMessage");
      connection.on("ReceiveMessage", (message) => {
        setMessages((prev) => {
          if (prev.some((m) => m.messageId === message.messageId)) return prev;
          if (
            selectedConversationRef &&
            message.conversationId === selectedConversationRef.conversationId
          ) {
            return [...prev, message];
          }
          return prev;
        });
        refreshConversations(accessToken);
      });

      connection.off("MessageSent");
      connection.on("MessageSent", (message) => {
        setMessages((prev) => {
          if (prev.some((m) => m.messageId === message.messageId)) return prev;
          return [...prev, message];
        });
        refreshConversations(accessToken);
      });

      connection.off("ReceiveBroadcast");
      connection.on("ReceiveBroadcast", (broadcast) => {
        setLiveBroadcast(broadcast);
      });
    } catch (err) {
      console.error("SignalR connection failed", err);
    }
  }

  async function refreshConversations(accessToken) {
    const data = await getConversations(accessToken);
    setConversations(data);
  }

  async function handleSelectConversation(conversation, accessToken = token) {
    setSelectedConversation(conversation);
    setSelectedConversationRef(conversation);
    const data = await getMessages(accessToken, conversation.conversationId);
    setMessages(data);
    await markConversationAsRead(accessToken, conversation.conversationId);
    setConversations((prev) =>
      prev.map((c) =>
        c.conversationId === conversation.conversationId ? { ...c, unreadCount: 0 } : c
      )
    );
  }

  if (loading) {
    return (
      <div className="msg-page" style={{ color: "var(--gray-400)", fontSize: 13 }}>
        Loading messages…
      </div>
    );
  }

  return (
    <div style={{ position: "relative", height: "calc(100vh - 100px)", overflow: "hidden", borderRadius: "var(--radius-lg)" }}>
      <button
        className="broadcast-toggle-btn"
        onClick={() => setBroadcastOpen(true)}
        aria-label="Open broadcasts"
      >
        Broadcasts
        {unreadBroadcasts > 0 && (
          <span className="badge" style={{ marginLeft: 6 }}>{unreadBroadcasts}</span>
        )}
      </button>

      <div className="chat-panel" style={{ height: "100%" }}>
        <ConversationList
          role="student"
          conversations={conversations}
          selectedConversationId={selectedConversation?.conversationId}
          onSelectConversation={handleSelectConversation}
        />
        <ChatWindow
          token={token}
          selectedConversation={selectedConversation}
          messages={messages}
          setMessages={setMessages}
        />
      </div>

      {broadcastOpen && (
        <div className="broadcast-overlay" onClick={() => setBroadcastOpen(false)} />
      )}

      <BroadcastInbox
        token={token}
        liveBroadcast={liveBroadcast}
        isOpen={broadcastOpen}
        onClose={() => setBroadcastOpen(false)}
        onUnreadChange={setUnreadBroadcasts}
      />
    </div>
  );
}
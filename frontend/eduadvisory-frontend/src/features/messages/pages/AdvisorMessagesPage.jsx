import { useEffect, useState } from "react";
import StudentList from "../components/StudentList";
import ChatWindow from "../components/ChatWindow";
import BroadcastForm from "../components/BroadcastForm";
import {
  getAdvisorStudents,
  getMessages,
  markConversationAsRead,
  startConversation,
} from "../api/messageApi";
import { startChatConnection } from "../signalr/chatConnection";
import { getAccessToken } from "../../../auth/getAccessToken";

export default function AdvisorMessagesPage() {
  const [token, setToken] = useState(null);
  const [students, setStudents] = useState([]);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [selectedConversation, setSelectedConversation] = useState(null);
  const [selectedConversationRef, setSelectedConversationRef] = useState(null);
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getAccessToken()
      .then((accessToken) => {
        setToken(accessToken);
        loadStudents(accessToken);
        connectSignalR(accessToken);
      })
      .catch((err) => console.error("Could not get token", err));
  }, []);

  async function loadStudents(accessToken) {
    try {
      setLoading(true);
      const data = await getAdvisorStudents(accessToken);
      setStudents(data);
    } catch (err) {
      console.error(err);
      alert("Could not load assigned students.");
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
            markConversationAsRead(accessToken, message.conversationId);
            return [...prev, message];
          }
        });
        loadStudents(accessToken);
      });

      connection.off("MessageSent");
      connection.on("MessageSent", (message) => {
        setMessages((prev) => {
          if (prev.some((m) => m.messageId === message.messageId)) return prev;
          return [...prev, message];
        });
      });
      connection.off("MessagesRead");
      connection.on("MessagesRead", (data) => {
        setMessages((prev) =>
          prev.map((message) =>
            message.conversationId === data.conversationId && message.isMine
              ? { ...message, isRead: true }
              : message,
          ),
        );
      });
      connection.off("MessageEdited");
      connection.on("MessageEdited", (updatedMessage) => {
        setMessages((prev) =>
          prev.map((m) =>
            m.messageId === updatedMessage.messageId ? updatedMessage : m,
          ),
        );
      });
      connection.off("MessageDeleted");
      connection.on("MessageDeleted", (data) => {
        setMessages((prev) =>
          prev.map((m) =>
            m.messageId === data.messageId
              ? { ...m, isDeleted: true, content: "", attachments: [] }
              : m,
          ),
        );
      });
    } catch (err) {
      console.error("SignalR connection failed", err);
    }
  }

  async function handleSelectStudent(student) {
    try {
      setSelectedStudent(student);
      const conversation = await startConversation(token, student.studentId);
      setSelectedConversation(conversation);
      setSelectedConversationRef(conversation);
      const data = await getMessages(token, conversation.conversationId);
      setMessages(data);
      await markConversationAsRead(token, conversation.conversationId);
      setStudents((prev) =>
        prev.map((s) =>
          s.studentId === student.studentId ? { ...s, unreadCount: 0 } : s,
        ),
      );
    } catch (err) {
      console.error(err);
      alert("Could not open conversation.");
    }
  }

  if (loading) {
    return (
      <div
        className="msg-page"
        style={{ color: "var(--gray-400)", fontSize: 13 }}
      >
        Loading students…
      </div>
    );
  }

  return (
    <div className="msg-page">
      <BroadcastForm token={token} />

      <div className="chat-panel" style={{ height: "calc(100vh - 230px)" }}>
        <StudentList
          students={students}
          selectedStudentId={selectedStudent?.studentId}
          onSelectStudent={handleSelectStudent}
        />
        <ChatWindow
          token={token}
          selectedConversation={selectedConversation}
          messages={messages}
          setMessages={setMessages}
        />
      </div>
    </div>
  );
}

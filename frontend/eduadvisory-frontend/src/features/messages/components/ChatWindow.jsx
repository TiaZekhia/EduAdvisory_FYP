import { useEffect, useRef, useState } from "react";
import MessageBubble from "./MessageBubble";
import { sendMessage } from "../api/messageApi";

export default function ChatWindow({ token, selectedConversation, messages, setMessages }) {
  const [content, setContent] = useState("");
  const bottomRef = useRef(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  async function handleSend(e) {
    e.preventDefault();
    if (!content.trim() || !selectedConversation) return;
    const text = content.trim();
    setContent("");
    try {
      await sendMessage(token, selectedConversation.conversationId, text);
    } catch (err) {
      console.error(err);
      setContent(text);
    }
  }

  if (!selectedConversation) {
    return (
      <div className="chat-window">
        <div className="chat-window__empty">Select a conversation</div>
      </div>
    );
  }

  return (
    <div className="chat-window">
      <div className="chat-window__head">
        {selectedConversation.studentName || selectedConversation.advisorName}
      </div>

      <div className="chat-window__messages">
        {messages.map((message) => (
          <MessageBubble key={message.messageId} message={message} />
        ))}
        <div ref={bottomRef} />
      </div>

      <form onSubmit={handleSend} className="chat-window__form">
        <input
          className="msg-input"
          style={{ flex: 1 }}
          placeholder="Type your message…"
          value={content}
          onChange={(e) => setContent(e.target.value)}
        />
        <button className="msg-btn" type="submit">Send</button>
      </form>
    </div>
  );
}
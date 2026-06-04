import { useEffect, useRef, useState } from "react";
import AiMessage from "./AiMessage";
import AiInput from "./AiInput";
import { sendStudentAiMessage } from "../../../../services/students/studentAiApi";

const AiChatBox = ({ selectedQuestion }) => {
  const [messages, setMessages] = useState([
    {
      role: "assistant",
      content:
        "Hi! I’m your AI Academic Assistant. Ask me about your study guides, syllabuses, courses, academic progress, or meetings.",
      sources: [],
      responseSource: "system",
    },
  ]);

  const [sessionId, setSessionId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const bottomRef = useRef(null);

  useEffect(() => {
    if (selectedQuestion) {
      handleSend(selectedQuestion);
    }
  }, [selectedQuestion]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  const handleSend = async (text) => {
    if (!text.trim() || isLoading) return;

    const userMessage = {
      role: "user",
      content: text,
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsLoading(true);

    try {
      const response = await sendStudentAiMessage({
        message: text,
        sessionId,
      });

      setSessionId(response.sessionId);

      const assistantMessage = {
        role: "assistant",
        content: response.answer,
        sources: response.sources || [],
        responseSource: response.responseSource,
        topSimilarityScore: response.topSimilarityScore,
      };

      setMessages((prev) => [...prev, assistantMessage]);
    } catch (error) {
      setMessages((prev) => [
        ...prev,
        {
          role: "assistant",
          content:
            error?.response?.data?.message ||
            "Sorry, I could not process your question right now.",
          sources: [],
          responseSource: "error",
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="ai-chat-box">
      <div className="ai-chat-messages">
        {messages.map((message, index) => (
          <AiMessage key={index} message={message} />
        ))}

        {isLoading && (
          <div className="ai-message assistant">
            <div className="ai-message-bubble">Thinking...</div>
          </div>
        )}

        <div ref={bottomRef} />
      </div>

      <AiInput onSend={handleSend} disabled={isLoading} />
    </div>
  );
};

export default AiChatBox;
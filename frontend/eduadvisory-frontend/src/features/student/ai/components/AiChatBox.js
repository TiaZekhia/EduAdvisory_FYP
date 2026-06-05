import { useEffect, useRef, useState } from "react";
import AiMessage from "./AiMessage";
import AiInput from "./AiInput";
import { sendStudentAiMessageStream } from "../../../../services/students/studentAiApi";

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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedQuestion]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  const handleSend = async (text) => {
    if (!text.trim() || isLoading) return;

    setMessages((prev) => [...prev, { role: "user", content: text }]);
    setIsLoading(true);

    // Track whether the assistant placeholder has been added
    let assistantAdded = false;

    const addAssistantPlaceholder = (firstContent) => {
      assistantAdded = true;
      setMessages((prev) => [
        ...prev,
        {
          role: "assistant",
          content: firstContent,
          sources: [],
          responseSource: null,
          topSimilarityScore: null,
        },
      ]);
    };

    const appendToken = (content) => {
      setMessages((prev) => {
        const updated = [...prev];
        const last = { ...updated[updated.length - 1] };
        last.content += content;
        updated[updated.length - 1] = last;
        return updated;
      });
    };

    try {
      await sendStudentAiMessageStream({
        message: text,
        sessionId,
        onToken: (content) => {
          if (!assistantAdded) {
            addAssistantPlaceholder(content);
          } else {
            appendToken(content);
          }
        },
        onMetadata: (metadata) => {
          setSessionId(metadata.sessionId);
          setMessages((prev) => {
            const updated = [...prev];
            const last = { ...updated[updated.length - 1] };
            last.responseSource = metadata.responseSource;
            last.topSimilarityScore = metadata.topSimilarityScore;
            last.sources = metadata.sources || [];
            updated[updated.length - 1] = last;
            return updated;
          });
        },
        onError: (errorMsg) => {
          const errorMessage = {
            role: "assistant",
            content:
              errorMsg || "Sorry, I could not stream the response right now.",
            sources: [],
            responseSource: "error",
            topSimilarityScore: null,
          };
          if (!assistantAdded) {
            assistantAdded = true;
            setMessages((prev) => [...prev, errorMessage]);
          } else {
            setMessages((prev) => {
              const updated = [...prev];
              updated[updated.length - 1] = errorMessage;
              return updated;
            });
          }
        },
        onDone: () => {
          // Loading is cleared in finally
        },
      });
    } catch {
      const fallback = {
        role: "assistant",
        content: "Sorry, I could not stream the response right now.",
        sources: [],
        responseSource: "error",
        topSimilarityScore: null,
      };
      if (!assistantAdded) {
        setMessages((prev) => [...prev, fallback]);
      } else {
        setMessages((prev) => {
          const updated = [...prev];
          updated[updated.length - 1] = fallback;
          return updated;
        });
      }
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

        {isLoading && messages[messages.length - 1]?.role !== "assistant" && (
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
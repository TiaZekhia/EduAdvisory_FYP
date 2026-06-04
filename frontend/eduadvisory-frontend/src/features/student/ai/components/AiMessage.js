import AiSourcesPanel from "./AiSourcesPanel";

const AiMessage = ({ message }) => {
  const isUser = message.role === "user";

  return (
    <div className={`ai-message ${isUser ? "user" : "assistant"}`}>
      <div className="ai-message-bubble">
        <div className="ai-message-content">{message.content}</div>

        {!isUser && message.responseSource && (
          <div className="ai-response-meta">
            Source: {message.responseSource}
            {message.topSimilarityScore != null &&
              ` • Similarity: ${message.topSimilarityScore.toFixed(2)}`}
          </div>
        )}

        {!isUser && message.sources?.length > 0 && (
          <AiSourcesPanel sources={message.sources} />
        )}
      </div>
    </div>
  );
};

export default AiMessage;
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";
import AiSourcesPanel from "./AiSourcesPanel";

const AiMessage = ({ message }) => {
  const isUser = message.role === "user";

  return (
    <div className={`ai-message ${isUser ? "user" : "assistant"}`}>
      <div className="ai-message-bubble">
        <div className="ai-message-content">
          {isUser ? (
            message.content
          ) : (
            <ReactMarkdown
              remarkPlugins={[remarkMath]}
              rehypePlugins={[rehypeKatex]}
            >
              {message.content}
            </ReactMarkdown>
          )}
        </div>

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
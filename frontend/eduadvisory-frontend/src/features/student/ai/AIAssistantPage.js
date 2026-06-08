import AiChatBox from "./components/AiChatBox";
import "./AIAssistantPage.css";

const AIAssistantPage = () => {
  return (
    <div className="ai-page">
      <div className="ai-page-header">
        <div className="ai-page-header-icon">
          <i className="pi pi-sparkles" />
        </div>
        <div>
          <div className="ai-page-title">AI Academic Assistant</div>
          <div className="ai-page-subtitle">
            Ask about your study guides, syllabuses, courses, academic progress, or meetings.
          </div>
        </div>
      </div>

      <div className="ai-page-body">
        <AiChatBox />
      </div>
    </div>
  );
};

export default AIAssistantPage;

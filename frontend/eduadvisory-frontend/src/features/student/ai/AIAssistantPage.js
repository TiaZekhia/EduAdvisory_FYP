import { useState } from "react";
import AiChatBox from "./components/AiChatBox";
import SuggestedQuestions from "./components/SuggestedQuestions";
import "./AIAssistantPage.css";

const AIAssistantPage = () => {
  const [selectedQuestion, setSelectedQuestion] = useState("");

  return (
    <div className="student-ai-page">
      <div className="student-ai-header">
        <h1>AI Academic Assistant</h1>
        <p>
          Ask questions about your study guides, course syllabuses, academic
          progress, courses, or upcoming meetings.
        </p>
      </div>

      <div className="student-ai-layout">
        <div className="student-ai-main">
          <AiChatBox selectedQuestion={selectedQuestion} />
        </div>

        <aside className="student-ai-sidebar">
          <SuggestedQuestions onSelectQuestion={setSelectedQuestion} />
        </aside>
      </div>
    </div>
  );
};

export default AIAssistantPage;
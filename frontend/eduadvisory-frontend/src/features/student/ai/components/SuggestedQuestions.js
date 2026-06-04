const questions = [
  "What topics should I focus on for this course?",
  "Summarize my study guide.",
  "What does the syllabus say about grading?",
  "Explain the most important concepts simply.",
  "What courses am I currently enrolled in?",
  "What is my GPA and academic status?",
  "Do I have any upcoming advisor meetings?",
];

const SuggestedQuestions = ({ onSelectQuestion }) => {
  return (
    <div className="suggested-questions">
      <h3>Suggested Questions</h3>

      {questions.map((question) => (
        <button
          key={question}
          type="button"
          onClick={() => onSelectQuestion(question)}
        >
          {question}
        </button>
      ))}
    </div>
  );
};

export default SuggestedQuestions;
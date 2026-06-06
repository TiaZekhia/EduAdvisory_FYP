import { useState } from "react";

const AiInput = ({ onSend, disabled }) => {
  const [value, setValue] = useState("");

  const submit = () => {
    if (!value.trim()) return;
    onSend(value);
    setValue("");
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      submit();
    }
  };

  return (
    <div className="ai-input-wrapper">
      <textarea
        value={value}
        disabled={disabled}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Ask about your study guide, syllabus, progress, or courses..."
        rows={2}
      />

      <button onClick={submit} disabled={disabled || !value.trim()}>
        Send
      </button>
    </div>
  );
};

export default AiInput;
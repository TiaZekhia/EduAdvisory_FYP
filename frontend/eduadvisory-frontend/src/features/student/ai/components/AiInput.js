import { useRef, useState } from "react";

const AiInput = ({ onSend, disabled }) => {
  const [value, setValue] = useState("");
  const ref = useRef(null);

  const autoGrow = () => {
    const el = ref.current;
    if (!el) return;
    el.style.height = "auto";
    el.style.height = Math.min(el.scrollHeight, 120) + "px";
  };

  const submit = () => {
    if (!value.trim() || disabled) return;
    onSend(value);
    setValue("");
    if (ref.current) ref.current.style.height = "auto";
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      submit();
    }
  };

  return (
    <div className="ai-input-wrapper">
      <div className="ai-input-inner">
        <textarea
          ref={ref}
          rows={1}
          value={value}
          disabled={disabled}
          onChange={(e) => { setValue(e.target.value); autoGrow(); }}
          onKeyDown={handleKeyDown}
          placeholder="Ask something…"
        />
      </div>

      <button className="ai-send-btn" onClick={submit} disabled={disabled || !value.trim()}>
        <i className="pi pi-send" style={{ fontSize: 15 }} />
      </button>
    </div>
  );
};

export default AiInput;

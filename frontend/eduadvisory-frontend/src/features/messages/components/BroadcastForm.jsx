import { useEffect, useRef, useState } from "react";
import { Toast } from "primereact/toast";
import { createBroadcast } from "../api/broadcastApi";
import { getAdvisorStudents } from "../api/messageApi";

export default function BroadcastForm({ token }) {
  const toast = useRef(null);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [students, setStudents] = useState([]);
  const [selectedStudentIds, setSelectedStudentIds] = useState([]);
  const [sending, setSending] = useState(false);

  useEffect(() => {
    if (!token) return;
    getAdvisorStudents(token)
      .then(setStudents)
      .catch((err) => console.error("Failed to load students", err));
  }, [token]);

  function toggleStudent(studentId) {
    setSelectedStudentIds((prev) =>
      prev.includes(studentId)
        ? prev.filter((id) => id !== studentId)
        : [...prev, studentId]
    );
  }

  function selectAllStudents() {
    setSelectedStudentIds(students.map((s) => s.studentId));
  }

  function clearStudents() {
    setSelectedStudentIds([]);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    if (!title.trim() || !content.trim()) return;
    try {
      setSending(true);
      await createBroadcast(token, title.trim(), content.trim(), selectedStudentIds);
      setTitle("");
      setContent("");
      setSelectedStudentIds([]);
      toast.current.show({
        severity: "success",
        summary: "Broadcast sent",
        detail: "Your message was delivered to the selected students.",
        life: 3000,
      });
    } catch (err) {
      console.error(err);
      toast.current.show({
        severity: "error",
        summary: "Failed to send",
        detail: "Something went wrong. Please try again.",
        life: 4000,
      });
    } finally {
      setSending(false);
    }
  }

  return (
    <>
      <Toast ref={toast} position="top-right" />

      <form onSubmit={handleSubmit} className="broadcast-form-card">
        <div className="broadcast-form-card__body">
          <div className="broadcast-form-card__title">Broadcast message</div>

          <div className="broadcast-form-card__row">
            <input
              className="msg-input"
              placeholder="Broadcast title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
            <input
              className="msg-input"
              style={{ flex: 2 }}
              placeholder="Message content"
              value={content}
              onChange={(e) => setContent(e.target.value)}
            />
            <button className="msg-btn" type="submit" disabled={sending}>
              {sending ? "Sending…" : "Broadcast"}
            </button>
          </div>

          <div className="student-selector">
            <div className="student-selector__header">
              <span className="student-selector__label">Select students</span>
              <div className="student-selector__actions">
                <button
                  type="button"
                  className="msg-btn msg-btn--outline msg-btn--sm"
                  onClick={selectAllStudents}
                >
                  Select all
                </button>
                <button
                  type="button"
                  className="msg-btn msg-btn--outline msg-btn--sm"
                  onClick={clearStudents}
                >
                  Clear
                </button>
              </div>
            </div>

            <div className="student-selector__hint">
              If none are selected, the broadcast will be sent to all assigned students.
            </div>

            <div className="student-selector__chips">
              {students.map((student) => {
                const selected = selectedStudentIds.includes(student.studentId);
                return (
                  <label
                    key={student.studentId}
                    className={`chip ${selected ? "chip--selected" : ""}`}
                  >
                    <input
                      type="checkbox"
                      checked={selected}
                      onChange={() => toggleStudent(student.studentId)}
                    />
                    <span>{student.fullName}</span>
                    {student.programCode && (
                      <span style={{ opacity: 0.6 }}>{student.programCode}</span>
                    )}
                  </label>
                );
              })}

              {students.length === 0 && (
                <span className="student-selector__hint">No assigned students found.</span>
              )}
            </div>
          </div>
        </div>
      </form>
    </>
  );
}
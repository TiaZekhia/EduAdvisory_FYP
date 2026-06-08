import { useEffect, useRef, useState } from "react";
import { Toast } from "primereact/toast";
import { createBroadcast } from "../api/broadcastApi";
import { getAdvisorStudents } from "../api/messageApi";

const MAX_FILE_SIZE = 10 * 1024 * 1024;

export default function BroadcastForm({ token }) {
  const toast = useRef(null);
  const fileInputRef = useRef(null);

  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [files, setFiles] = useState([]);
  const [students, setStudents] = useState([]);
  const [selectedStudentIds, setSelectedStudentIds] = useState([]);
  const [studentSearch, setStudentSearch] = useState("");
  const [programFilter, setProgramFilter] = useState("ALL");
  const [sending, setSending] = useState(false);

  useEffect(() => {
    if (!token) return;

    getAdvisorStudents(token)
      .then(setStudents)
      .catch((err) => console.error("Failed to load students", err));
  }, [token]);

  const programs = [...new Set(students.map((s) => s.programCode).filter(Boolean))].sort();

  const filteredStudents = students.filter((student) => {
    const q = studentSearch.trim().toLowerCase();
    if (
      q &&
      !`${student.fullName || ""} ${student.studentId || ""}`
        .toLowerCase()
        .includes(q)
    )
      return false;

    if (programFilter !== "ALL" && student.programCode !== programFilter)
      return false;

    return true;
  });

  const hasActiveFilters = studentSearch.trim() || programFilter !== "ALL";

  function clearFilters() {
    setStudentSearch("");
    setProgramFilter("ALL");
  }

  function showFileError(message) {
    toast.current?.show({
      severity: "error",
      summary: "File error",
      detail: message,
      life: 3500,
    });
  }

  function handleFileChange(e) {
    const selectedFiles = Array.from(e.target.files || []);
    const validFiles = [];

    selectedFiles.forEach((file) => {
      if (file.size === 0) {
        showFileError(`${file.name} is empty and cannot be uploaded.`);
        return;
      }

      if (file.size > MAX_FILE_SIZE) {
        showFileError(`${file.name} is too large. Maximum size is 10MB.`);
        return;
      }

      validFiles.push(file);
    });

    setFiles((prev) => [...prev, ...validFiles]);
    e.target.value = "";
  }

  function removeFile(index) {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  }

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

      await createBroadcast(
        token,
        title.trim(),
        content.trim(),
        selectedStudentIds,
        files
      );

      setTitle("");
      setContent("");
      setFiles([]);
      setSelectedStudentIds([]);
      setStudentSearch("");
      setProgramFilter("ALL");

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

            <input
              ref={fileInputRef}
              type="file"
              multiple
              hidden
              onChange={handleFileChange}
              accept="image/*,.pdf,.doc,.docx,.xls,.xlsx,.txt"
            />

            <button
              type="button"
              className="msg-btn msg-btn--outline"
              onClick={() => fileInputRef.current?.click()}
            >
              📎
            </button>

            <button className="msg-btn" type="submit" disabled={sending}>
              {sending ? "Sending…" : "Broadcast"}
            </button>
          </div>

          {files.length > 0 && (
            <div className="broadcast-file-preview-bar">
              {files.map((file, index) => {
                const isImage = file.type.startsWith("image/");

                return (
                  <div
                    className="broadcast-file-card"
                    key={`${file.name}-${index}`}
                  >
                    <button
                      type="button"
                      className="broadcast-file-card__remove"
                      onClick={() => removeFile(index)}
                    >
                      ×
                    </button>

                    {isImage ? (
                      <img
                        src={URL.createObjectURL(file)}
                        alt={file.name}
                        className="broadcast-file-card__image"
                      />
                    ) : (
                      <div className="broadcast-file-card__doc">📄</div>
                    )}

                    <div className="broadcast-file-card__name" title={file.name}>
                      {file.name}
                    </div>

                    <div className="broadcast-file-card__size">
                      {(file.size / 1024).toFixed(1)} KB
                    </div>
                  </div>
                );
              })}
            </div>
          )}

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
              If none are selected, the broadcast will be sent to all assigned
              students.
            </div>

            {/* Filter panel */}
            <div className="bc-filter-panel">
              <div className="bc-filter-label">
                <i className="pi pi-filter" />
                Filters
              </div>

              <div className="bc-filter-controls">
                <div className="bc-filter-search-wrap">
                  <i className="pi pi-search bc-filter-search-icon" />
                  <input
                    type="text"
                    className="bc-filter-search"
                    placeholder="Name or Student ID…"
                    value={studentSearch}
                    onChange={(e) => setStudentSearch(e.target.value)}
                  />
                  {studentSearch && (
                    <button
                      type="button"
                      className="bc-filter-search-clear"
                      onClick={() => setStudentSearch("")}
                    >
                      <i className="pi pi-times" />
                    </button>
                  )}
                </div>

                {programs.length > 0 && (
                  <div className="bc-filter-select-wrap">
                    <i className="pi pi-book bc-filter-select-icon" />
                    <select
                      className="bc-filter-select"
                      value={programFilter}
                      onChange={(e) => setProgramFilter(e.target.value)}
                    >
                      <option value="ALL">All Programs</option>
                      {programs.map((p) => (
                        <option key={p} value={p}>
                          {p}
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                {hasActiveFilters && (
                  <button
                    type="button"
                    className="bc-filter-clear"
                    onClick={clearFilters}
                  >
                    <i className="pi pi-times" />
                    Clear
                  </button>
                )}
              </div>

              <div className="bc-filter-count">
                {hasActiveFilters ? (
                  <>
                    <span className="bc-filter-count-highlight">
                      {filteredStudents.length}
                    </span>
                    {" of "}
                    {students.length} students match
                  </>
                ) : (
                  <>{students.length} students total</>
                )}
                {selectedStudentIds.length > 0 && (
                  <span className="bc-filter-count-selected">
                    {" · "}
                    {selectedStudentIds.length} selected
                  </span>
                )}
              </div>
            </div>

            <div
              className="student-selector__chips"
              style={{
                maxHeight: "250px",
                overflowY: "auto",
                border: "1px solid #ddd",
                borderRadius: "8px",
                padding: "0.5rem",
              }}
            >
              {filteredStudents.map((student) => {
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
                      <span style={{ opacity: 0.6 }}>
                        {student.programCode}
                      </span>
                    )}
                  </label>
                );
              })}

              {students.length === 0 && (
                <span className="student-selector__hint">
                  No assigned students found.
                </span>
              )}

              {students.length > 0 && filteredStudents.length === 0 && (
                <span className="student-selector__hint">
                  No students match your search.
                </span>
              )}
            </div>
          </div>
        </div>
      </form>
    </>
  );
}
import { useState } from "react";

export default function StudentList({ students, selectedStudentId, onSelectStudent }) {
  const [search, setSearch] = useState("");

  const filteredStudents = students.filter((student) =>
    student.fullName?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="msg-sidebar">
      <div className="msg-sidebar__head">
        <div className="msg-sidebar__title">Students</div>
        <input
          className="msg-input"
          style={{ width: "100%" }}
          placeholder="Search students…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="msg-sidebar__list">
        {filteredStudents.length === 0 && (
          <div style={{ padding: "12px 16px", fontSize: 12, color: "var(--gray-400)" }}>
            No assigned students found.
          </div>
        )}

        {filteredStudents.map((student) => (
          <button
            key={student.studentId}
            className={`sidebar-item ${
              selectedStudentId === student.studentId ? "sidebar-item--active" : ""
            }`}
            onClick={() => onSelectStudent(student)}
          >
            <span className="sidebar-item__name">{student.fullName}</span>
            <span className="sidebar-item__sub">
              {student.programCode || "No program"}
              {student.currentSemester ? ` · Semester ${student.currentSemester}` : ""}
            </span>
            {student.email && (
              <span className="sidebar-item__preview">{student.email}</span>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
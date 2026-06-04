import { Alert } from "bootstrap";
import { useState, useRef } from "react";

const initialState = {
  scope: "course",
  title: "",
  documentType: "study_guide",
  courseCode: "",
  programCode: "",
  academicYear: "",
  semester: "",
  file: null,
};

export default function DocumentUploadForm({ onSubmit, isUploading }) {
  const [values, setValues] = useState(initialState);
  const fileInputRef = useRef(null);

  const updateField = (field, value) => {
    setValues((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Basic validation depending on scope
    if (!values.title || !values.documentType || !values.file) {
      alert("Please fill in all required fields and select a PDF file.");
      return;
    }

    if (values.scope === "course" && !values.courseCode) {
      alert("Please provide a Course Code for course-scoped documents.");
      return;
    }

    if (values.scope === "program" && !values.programCode) {
      alert("Please provide a Program Code for program-scoped documents.");
      return;
    }

    await onSubmit(values);
    setValues(initialState);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <form className="document-upload-form" onSubmit={handleSubmit}>
      <div className="form-group mb-3">
        <label>Document Title</label>
        <input
          className="form-control"
          value={values.title}
          onChange={(e) => updateField("title", e.target.value)}
          placeholder="Example: Data Structures Study Guide"
          disabled={isUploading}
        />
      </div>

      <div className="form-group mb-3">
        <label>Scope</label>
        <select
          className="form-select mb-2"
          value={values.scope}
          onChange={(e) => updateField("scope", e.target.value)}
          disabled={isUploading}
        >
          <option value="course">Course</option>
          <option value="program">Program</option>
          <option value="general">General</option>
        </select>

        <label>Document Type</label>
        <select
          className="form-select"
          value={values.documentType}
          onChange={(e) => updateField("documentType", e.target.value)}
          disabled={isUploading}
        >
          <option value="study_guide">Study Guide</option>
          <option value="course_syllabus">Course Syllabus</option>
        </select>
      </div>

      {values.scope === "course" && (
        <div className="form-group mb-3">
          <label>Course Code</label>
          <input
            className="form-control"
            value={values.courseCode}
            onChange={(e) => updateField("courseCode", e.target.value)}
            placeholder="Example: CS201"
            disabled={isUploading}
          />
        </div>
      )}

      {values.scope === "program" && (
        <div className="form-group mb-3">
          <label>Program Code</label>
          <input
            className="form-control"
            value={values.programCode}
            onChange={(e) => updateField("programCode", e.target.value)}
            placeholder="Example: CCE"
            disabled={isUploading}
          />
        </div>
      )}

      <div className="form-group mb-3">
        <label>Academic Year</label>
        <input
          className="form-control"
          value={values.academicYear}
          onChange={(e) => updateField("academicYear", e.target.value)}
          placeholder="Example: 2023-2024"
          disabled={isUploading}
        />
      </div>

      <div className="form-group mb-3">
        <label>Semester</label>
        <input
          className="form-control"
          value={values.semester}
          onChange={(e) => updateField("semester", e.target.value)}
          placeholder="Example: Spring 2026"
          disabled={isUploading}
        />
      </div>

      <div className="form-group mb-3">
        <label>PDF File</label>
        <input
          ref={fileInputRef}
          type="file"
          className="form-control"
          accept="application/pdf"
          disabled={isUploading}
          onChange={(e) => updateField("file", e.target.files?.[0] || null)}
        />
      </div>

      <button className="btn btn-dark w-100" disabled={isUploading}>
        {isUploading ? "Uploading and Processing..." : "Upload Document"}
      </button>
    </form>
  );
}
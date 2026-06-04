import DocumentStatusBadge from "./DocumentStatusBadge";

const formatType = (type) => {
  if (type === "study_guide") return "Study Guide";
  if (type === "course_syllabus") return "Course Syllabus";
  return type;
};

export default function DocumentListTable({
  documents,
  isLoading,
  onDelete,
  onReprocess,
}) {
  if (isLoading) {
    return <div className="text-muted">Loading documents...</div>;
  }

  if (!documents.length) {
    return <div className="text-muted">No AI documents uploaded yet.</div>;
  }

  return (
    <div className="ai-document-list">
      {documents.map((doc) => (
        <div className="ai-document-card" key={doc.documentId}>
          <div className="ai-document-card-header">
            <div>
              <div className="ai-document-card-title">{doc.title}</div>
              <div className="ai-document-card-filename text-muted small">
                {doc.fileName}
              </div>
            </div>
            <span className="badge bg-primary text-uppercase small px-3">
              {formatType(doc.documentType)}
            </span>
          </div>

          <div className="ai-document-card-body">
            <div className="ai-document-card-row">
              <div>
                <span className="field-label">Scope</span>
                <div>{doc.scope || "course"}</div>
              </div>
              <div>
                <span className="field-label">Course</span>
                <div>{doc.courseCode || "—"}</div>
              </div>
            </div>
            <div className="ai-document-card-row">
              <div>
                <span className="field-label">Program</span>
                <div>{doc.programCode || "—"}</div>
              </div>
              <div>
                <span className="field-label">Academic Year</span>
                <div>{doc.academicYear || "—"}</div>
              </div>
            </div>
          </div>

          <div className="ai-document-card-footer">
            <div>
              <DocumentStatusBadge status={doc.status} />
              {doc.errorMessage && (
                <div className="text-danger small mt-1">
                  {doc.errorMessage}
                </div>
              )}
            </div>

            <div className="ai-document-card-meta">
              <span>{doc.chunkCount} chunks</span>
              <span>
                {doc.createdAt
                  ? new Date(doc.createdAt).toLocaleDateString()
                  : "-"}
              </span>
            </div>
          </div>

          <div className="ai-document-card-actions">
            <button
              className="btn btn-sm btn-outline-secondary"
              onClick={() => onReprocess(doc.documentId)}
            >
              Reprocess
            </button>
            <button
              className="btn btn-sm btn-outline-danger"
              onClick={() => onDelete(doc.documentId)}
            >
              Delete
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
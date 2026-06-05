import { useState, useEffect } from "react";
import DocumentStatusBadge from "./DocumentStatusBadge";

const ITEMS_PER_PAGE = 6;

const formatType = (type) => {
  if (type === "study_guide") return "Study Guide";
  if (type === "course_syllabus") return "Course Syllabus";
  return type;
};

const TYPE_META = {
  study_guide: { label: "Study Guide", cls: "doc-type-study-guide", icon: "📚" },
  course_syllabus: { label: "Course Syllabus", cls: "doc-type-syllabus", icon: "📋" },
};

const IconChunks = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="2" y="3" width="20" height="14" rx="2" />
    <path d="M8 21h8M12 17v4" />
  </svg>
);

const IconCalendar = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" />
    <path d="M16 2v4M8 2v4M3 10h18" />
  </svg>
);

const IconSearch = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <path d="M21 21l-4.35-4.35" />
  </svg>
);

const IconReprocess = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 2v6h-6" />
    <path d="M3 12a9 9 0 0 1 15-6.7L21 8" />
    <path d="M3 22v-6h6" />
    <path d="M21 12a9 9 0 0 1-15 6.7L3 16" />
  </svg>
);

const IconTrash = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6" />
  </svg>
);

const IconChevronLeft = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M15 18l-6-6 6-6" />
  </svg>
);

const IconChevronRight = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9 18l6-6-6-6" />
  </svg>
);

export default function DocumentListTable({ documents, isLoading, onDelete, onReprocess }) {
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Reset to page 1 whenever search changes
  useEffect(() => {
    setCurrentPage(1);
  }, [search]);

  if (isLoading) {
    return (
      <div className="doc-loading-state">
        <div className="doc-loading-spinner" />
        <span>Loading documents...</span>
      </div>
    );
  }

  if (!documents.length) {
    return (
      <div className="doc-empty-state">
        <div className="doc-empty-icon">📂</div>
        <div className="doc-empty-title">No documents yet</div>
        <div className="doc-empty-sub">Upload a study guide or syllabus to get started.</div>
      </div>
    );
  }

  const stats = {
    total: documents.length,
    processed: documents.filter((d) => d.status === "Processed").length,
    processing: documents.filter((d) => d.status === "Processing").length,
    failed: documents.filter((d) => d.status === "Failed").length,
  };

  const filtered = search
    ? documents.filter(
        (d) =>
          d.title.toLowerCase().includes(search.toLowerCase()) ||
          d.courseCode?.toLowerCase().includes(search.toLowerCase()) ||
          d.programCode?.toLowerCase().includes(search.toLowerCase()) ||
          formatType(d.documentType).toLowerCase().includes(search.toLowerCase())
      )
    : documents;

  const totalPages = Math.ceil(filtered.length / ITEMS_PER_PAGE);
  const paginated = filtered.slice(
    (currentPage - 1) * ITEMS_PER_PAGE,
    currentPage * ITEMS_PER_PAGE
  );

  const getPageNumbers = () => {
    if (totalPages <= 5) return Array.from({ length: totalPages }, (_, i) => i + 1);
    if (currentPage <= 3) return [1, 2, 3, 4, "...", totalPages];
    if (currentPage >= totalPages - 2) return [1, "...", totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    return [1, "...", currentPage - 1, currentPage, currentPage + 1, "...", totalPages];
  };

  return (
    <div className="doc-list-container">
      <div className="doc-stats-bar">
        <div className="doc-stat">
          <span className="doc-stat-value">{stats.total}</span>
          <span className="doc-stat-label">Total</span>
        </div>
        <div className="doc-stat doc-stat-success">
          <span className="doc-stat-value">{stats.processed}</span>
          <span className="doc-stat-label">Processed</span>
        </div>
        <div className="doc-stat doc-stat-warning">
          <span className="doc-stat-value">{stats.processing}</span>
          <span className="doc-stat-label">Processing</span>
        </div>
        <div className="doc-stat doc-stat-danger">
          <span className="doc-stat-value">{stats.failed}</span>
          <span className="doc-stat-label">Failed</span>
        </div>
      </div>

      <div className="doc-search-wrap">
        <span className="doc-search-icon">
          <IconSearch />
        </span>
        <input
          type="text"
          className="doc-search-input"
          placeholder="Search by title, course, or program..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {search && (
          <button className="doc-search-clear" onClick={() => setSearch("")}>
            ✕
          </button>
        )}
      </div>

      {!filtered.length ? (
        <div className="doc-no-results">No documents match "{search}".</div>
      ) : (
        <>
          <div className="ai-document-list">
            {paginated.map((doc) => {
              const typeMeta = TYPE_META[doc.documentType] || {
                label: formatType(doc.documentType),
                cls: "doc-type-default",
                icon: "📄",
              };

              return (
                <div className="ai-document-card" key={doc.documentId}>
                  <div className="ai-document-card-header">
                    <div className="doc-type-icon">{typeMeta.icon}</div>
                    <div className="doc-title-block">
                      <div className="ai-document-card-title">{doc.title}</div>
                      <div className="ai-document-card-filename">{doc.fileName}</div>
                    </div>
                    <span className={`doc-type-badge ${typeMeta.cls}`}>{typeMeta.label}</span>
                  </div>

                  <div className="ai-document-card-body">
                    <div className="ai-document-card-row">
                      <div className="doc-field">
                        <span className="field-label">Scope</span>
                        <div className="field-value">{doc.scope || "course"}</div>
                      </div>
                      <div className="doc-field">
                        <span className="field-label">Course</span>
                        <div className="field-value">{doc.courseCode || "—"}</div>
                      </div>
                    </div>
                    <div className="ai-document-card-row">
                      <div className="doc-field">
                        <span className="field-label">Program</span>
                        <div className="field-value">{doc.programCode || "—"}</div>
                      </div>
                      <div className="doc-field">
                        <span className="field-label">Academic Year</span>
                        <div className="field-value">{doc.academicYear || "—"}</div>
                      </div>
                    </div>
                  </div>

                  <div className="ai-document-card-footer">
                    <div className="doc-status-block">
                      <DocumentStatusBadge status={doc.status} />
                      {doc.errorMessage && (
                        <div className="doc-error-msg">{doc.errorMessage}</div>
                      )}
                    </div>
                    <div className="doc-meta-chips">
                      <span className="doc-meta-chip">
                        <IconChunks />
                        {doc.chunkCount ?? 0} chunks
                      </span>
                      <span className="doc-meta-chip">
                        <IconCalendar />
                        {doc.createdAt ? new Date(doc.createdAt).toLocaleDateString() : "—"}
                      </span>
                    </div>
                  </div>

                  <div className="ai-document-card-actions">
                    <button
                      className="doc-action-btn doc-action-reprocess"
                      onClick={() => onReprocess(doc.documentId)}
                    >
                      <IconReprocess />
                      Reprocess
                    </button>
                    <button
                      className="doc-action-btn doc-action-delete"
                      onClick={() => onDelete(doc.documentId)}
                    >
                      <IconTrash />
                      Delete
                    </button>
                  </div>
                </div>
              );
            })}
          </div>

          {totalPages > 1 && (
            <div className="doc-pagination">
              <span className="doc-pagination-info">
                {(currentPage - 1) * ITEMS_PER_PAGE + 1}–
                {Math.min(currentPage * ITEMS_PER_PAGE, filtered.length)} of {filtered.length}
              </span>

              <div className="doc-pagination-controls">
                <button
                  className="doc-page-btn"
                  onClick={() => setCurrentPage((p) => p - 1)}
                  disabled={currentPage === 1}
                >
                  <IconChevronLeft />
                </button>

                {getPageNumbers().map((page, idx) =>
                  page === "..." ? (
                    <span key={`ellipsis-${idx}`} className="doc-page-ellipsis">…</span>
                  ) : (
                    <button
                      key={page}
                      className={`doc-page-btn ${currentPage === page ? "active" : ""}`}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  )
                )}

                <button
                  className="doc-page-btn"
                  onClick={() => setCurrentPage((p) => p + 1)}
                  disabled={currentPage === totalPages}
                >
                  <IconChevronRight />
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

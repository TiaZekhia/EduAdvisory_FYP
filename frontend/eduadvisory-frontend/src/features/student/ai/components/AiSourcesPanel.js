const formatDocumentType = (type) => {
  if (type === "study_guide") return "Study Guide";
  if (type === "course_syllabus") return "Course Syllabus";
  return type;
};

const AiSourcesPanel = ({ sources }) => {
  if (!sources?.length) return null;

  return (
    <div className="ai-sources-panel">
      <strong>Sources</strong>

      {sources.map((source) => (
        <div key={source.chunkId} className="ai-source-card">
          <div className="ai-source-title">{source.title}</div>
          <div className="ai-source-meta">
            {formatDocumentType(source.documentType)} • {source.courseCode}
          </div>

          {source.sectionTitle && (
            <div className="ai-source-section">
              Section: {source.sectionTitle}
            </div>
          )}

          {source.pageNumber && (
            <div className="ai-source-page">Page: {source.pageNumber}</div>
          )}

          <div className="ai-source-score">
            Similarity: {source.similarityScore.toFixed(2)}
          </div>
        </div>
      ))}
    </div>
  );
};

export default AiSourcesPanel;
import { useState } from "react";

const formatDocumentType = (type) => {
  if (type === "study_guide") return "Study Guide";
  if (type === "course_syllabus") return "Syllabus";
  return type ?? "";
};

const AiSourcesPanel = ({ sources }) => {
  const [open, setOpen] = useState(false);

  if (!sources?.length) return null;

  return (
    <div>
      <button className="ai-sources-toggle" onClick={() => setOpen((o) => !o)}>
        <i className={`pi ${open ? "pi-chevron-up" : "pi-chevron-down"}`} />
        {sources.length} source{sources.length !== 1 ? "s" : ""}
      </button>

      {open && (
        <div className="ai-sources-chips">
          {sources.map((src) => (
            <span key={src.chunkId} className="ai-source-chip" title={src.title}>
              <i className="pi pi-file ai-source-chip-icon" />
              <span>
                {src.courseCode && <strong>{src.courseCode} · </strong>}
                {formatDocumentType(src.documentType)}
                {src.pageNumber && ` · p.${src.pageNumber}`}
              </span>
            </span>
          ))}
        </div>
      )}
    </div>
  );
};

export default AiSourcesPanel;

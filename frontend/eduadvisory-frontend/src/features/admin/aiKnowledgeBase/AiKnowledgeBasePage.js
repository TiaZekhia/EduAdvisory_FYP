import { useEffect, useState } from "react";
import {
  deleteAiDocument,
  getAiDocuments,
  reprocessAiDocument,
  uploadAiDocument,
} from "../../../services/admin/aiKnowledgeBaseApi";
import DocumentUploadForm from "./components/DocumentUploadForm";
import DocumentListTable from "./components/DocumentListTable";
import "./AiKnowledgeBasePage.css";

export default function AiKnowledgeBasePage() {
  const [documents, setDocuments] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const loadDocuments = async () => {
    setIsLoading(true);
    setError("");

    try {
      const data = await getAiDocuments();
      setDocuments(data);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          "Failed to load AI knowledge base documents."
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadDocuments();
  }, []);

  const handleUpload = async (values) => {
    setIsUploading(true);
    setError("");
    setSuccessMessage("");

    try {
      const formData = new FormData();

      formData.append("title", values.title);
      formData.append("documentType", values.documentType);
      formData.append("scope", values.scope || "course");
      formData.append("courseCode", values.courseCode || "");
      formData.append("programCode", values.programCode || "");
      formData.append("academicYear", values.academicYear || "");
      formData.append("semester", values.semester || "");
      formData.append("file", values.file);

      await uploadAiDocument(formData);

      setSuccessMessage("Document uploaded and processed successfully.");
      await loadDocuments();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          "Failed to upload document. Please check the file and course code."
      );
    } finally {
      setIsUploading(false);
    }
  };

  const handleDelete = async (documentId) => {
    const confirmed = window.confirm(
      "Are you sure you want to delete this AI document?"
    );

    if (!confirmed) return;

    setError("");
    setSuccessMessage("");

    try {
      await deleteAiDocument(documentId);
      setSuccessMessage("Document deleted successfully.");
      await loadDocuments();
    } catch (err) {
      setError(
        err?.response?.data?.message || "Failed to delete AI document."
      );
    }
  };

  const handleReprocess = async (documentId) => {
    setError("");
    setSuccessMessage("");

    try {
      await reprocessAiDocument(documentId);
      setSuccessMessage("Document reprocessed successfully.");
      await loadDocuments();
    } catch (err) {
      setError(
        err?.response?.data?.message || "Failed to reprocess AI document."
      );
    }
  };

  return (
    <div className="ai-kb-page">
      <div className="ai-kb-header">
        <div>
          <h1>AI Knowledge Base</h1>
          <p>
            Upload study guides and course syllabuses used by the Student AI
            Assistant.
          </p>
        </div>

        <button className="btn btn-outline-secondary" onClick={loadDocuments}>
          Refresh
        </button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {successMessage && (
        <div className="alert alert-success">{successMessage}</div>
      )}

      <div className="ai-kb-grid">
        <section className="ai-kb-card">
          <h2>Upload Document</h2>
          <DocumentUploadForm
            onSubmit={handleUpload}
            isUploading={isUploading}
          />
        </section>

        <section className="ai-kb-card">
          <h2>Uploaded Documents</h2>
          <DocumentListTable
            documents={documents}
            isLoading={isLoading}
            onDelete={handleDelete}
            onReprocess={handleReprocess}
          />
        </section>
      </div>
    </div>
  );
}
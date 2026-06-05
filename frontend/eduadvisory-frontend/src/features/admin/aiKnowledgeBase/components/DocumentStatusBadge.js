const STATUS_MAP = {
  Uploaded: { cls: "status-badge-uploaded", icon: "⏳", label: "Uploaded" },
  Processing: { cls: "status-badge-processing", icon: "⚙️", label: "Processing" },
  Processed: { cls: "status-badge-processed", icon: "✓", label: "Processed" },
  Failed: { cls: "status-badge-failed", icon: "✕", label: "Failed" },
  Deleted: { cls: "status-badge-deleted", icon: "🗑", label: "Deleted" },
};

export default function DocumentStatusBadge({ status }) {
  const meta = STATUS_MAP[status] || { cls: "status-badge-uploaded", icon: "•", label: status };
  return (
    <span className={`status-badge ${meta.cls}`}>
      <span className="status-badge-icon">{meta.icon}</span>
      {meta.label}
    </span>
  );
}

const statusClassMap = {
  Uploaded: "bg-secondary",
  Processing: "bg-warning text-dark",
  Processed: "bg-success",
  Failed: "bg-danger",
  Deleted: "bg-dark",
};

export default function DocumentStatusBadge({ status }) {
  const className = statusClassMap[status] || "bg-secondary";

  return <span className={`badge ${className}`}>{status}</span>;
}
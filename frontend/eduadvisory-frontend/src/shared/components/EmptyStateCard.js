import "./EmptyStateCard.css"

export default function EmptyStateCard({
  icon = "pi pi-inbox",
  title,
  text,
  className = "",
}) {
  return (
    <div className={`shared-empty-state ${className}`}>
      <div className="shared-empty-icon">
        <i className={icon} />
      </div>

      <div>
        <div className="shared-empty-title">{title}</div>
        {text ? <div className="shared-empty-text">{text}</div> : null}
      </div>
    </div>
  );
}
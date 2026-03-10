import { Card } from "primereact/card";
import "./PageSectionCard.css";

export default function PageSectionCard({
  title,
  subtitle,
  children,
  className = "",
}) {
  return (
    <Card className={`page-section-card shadow-sm border-0 ${className}`}>
      <div className="page-section-card-header">
        <div>
          <div className="fw-semibold fs-4">{title}</div>
          {subtitle ? <div className="text-muted mt-1">{subtitle}</div> : null}
        </div>
      </div>

      {children}
    </Card>
  );
}
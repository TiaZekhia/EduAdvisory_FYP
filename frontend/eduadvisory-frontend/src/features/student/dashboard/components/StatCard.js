import { Card } from "primereact/card";

export default function StatCard({
  title,
  value,
  subtitle,
  icon,
  valueClassName
}) {
  return (
    <Card className="stat-card">
      <div className="stat-card-top">
        <div className="stat-card-title">
          {icon && <i className={`${icon} stat-card-icon`} />}
          <span>{title}</span>
        </div>
      </div>

      <div className={`stat-card-value ${valueClassName ?? ""}`}>{value}</div>
      <div className="stat-card-subtitle">{subtitle}</div>
    </Card>
  );
}
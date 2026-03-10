import { Card } from "primereact/card";

export default function QuickActionCard({
  title,
  subtitle,
  icon,
  iconClassName = "",
  onClick,
}) {
  return (
    <Card className="quick-card" onClick={onClick}>
      <i className={`${icon} quick-card-icon ${iconClassName}`} />
      <div className="quick-card-title">{title}</div>
      <div className="quick-card-subtitle">{subtitle}</div>
    </Card>
  );
}
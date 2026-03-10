import "./DashboardStatCard.css";

export default function DashboardStatCard({ title, value, icon, className = "" }) {
  return (
    <div className={`col-12 col-md-4 ${className}`}>
      <div className="dashboard-stat-card h-100">
        <div className="dashboard-stat-card-top">
          <div className="dashboard-stat-icon">
            {icon ? <i className={icon} /> : null}
          </div>
          <div className="dashboard-stat-title">{title}</div>
        </div>
        <div className="dashboard-stat-value">{value}</div>
      </div>
    </div>
  );
}
import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { useStudentAlerts } from "../context/StudentAlertsProvider";
import { PageHero } from "../../../shared/components/PageHero";
import DashboardStatCard from "../../../shared/components/DashboardStatCard";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import EmptyStateCard from "../../../shared/components/EmptyStateCard";
import "./alertsPage.css";

function severityToTag(sev) {
  if (sev === "HIGH") return "danger";
  if (sev === "MEDIUM") return "warning";
  return "info";
}

function severityLabel(sev) {
  if (sev === "HIGH") return "Critical";
  if (sev === "MEDIUM") return "Warning";
  return "Info";
}

export default function AlertsPage() {
  const {
    alerts,
    alertsCount,
    highCount,
    mediumCount,
    lowCount,
    loading,
    refreshAlerts,
  } = useStudentAlerts();

  return (
    <div className="alerts-page container-fluid p-3 p-md-4">
      <PageHero
        title="Alerts & Notifications"
        subtitle="Important alerts about your academic progress and requirements"
      />

      <div className="row g-4 mb-4">
        <DashboardStatCard
          title="Total Alerts"
          value={alertsCount}
          icon="pi pi-bell"
        />
        <DashboardStatCard
          title="Critical"
          value={highCount}
          icon="pi pi-exclamation-triangle"
        />
        <DashboardStatCard
          title="Warnings"
          value={mediumCount}
          icon="pi pi-info-circle"
        />
        <DashboardStatCard
          title="Info"
          value={lowCount}
          icon="pi pi-bookmark"
        />
      </div>

      <PageSectionCard
        title="All Alerts"
        subtitle="Review all active alerts related to your courses and academic status"
        className="mb-4"
        headerRight={
          <Button
            label="Refresh"
            icon="pi pi-refresh"
            outlined
            onClick={refreshAlerts}
            loading={loading}
          />
        }
      >
        {loading ? (
          <div className="alerts-empty">Loading alerts...</div>
        ) : alerts.length === 0 ? (
          <EmptyStateCard title="No alerts right now" icon="pi pi-check-circle" />
        ) : (
          <div className="d-flex flex-column gap-3">
            {alerts.map((alert, index) => (
              <div key={index} className="alert-item-card">
                <div className="alert-item-main">
                  <div className="alert-item-top">
                    {alert.courseCode ? (
                      <span className="alert-course-code">{alert.courseCode}</span>
                    ) : null}
                    <span className="alert-item-title">{alert.title}</span>
                  </div>

                  <div className="alert-item-message">{alert.message}</div>
                </div>

                <div className="alert-item-side">
                  <Tag
                    value={severityLabel(alert.severity)}
                    severity={severityToTag(alert.severity)}
                  />
                </div>
              </div>
            ))}
          </div>
        )}
      </PageSectionCard>
    </div>
  );
}
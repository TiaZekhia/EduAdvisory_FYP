import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { useNavigate } from "react-router-dom";
import "./components.css";

function rankSeverity(sev) {
  if (sev === "HIGH") return 3;
  if (sev === "MEDIUM") return 2;
  return 1;
}

function severityMeta(sev) {
  if (sev === "HIGH") return { cls: "danger", label: "At-Risk Alert", icon: "pi pi-exclamation-triangle" };
  if (sev === "MEDIUM") return { cls: "warning", label: "Warning", icon: "pi pi-exclamation-triangle" };
  return { cls: "info", label: "Notice", icon: "pi pi-info-circle" };
}

function severityToTag(sev) {
  if (sev === "HIGH") return "danger";
  if (sev === "MEDIUM") return "warning";
  return "info";
}

export default function AlertsBanner({ alerts }) {
  const navigate = useNavigate();
  if (!alerts?.length) return null;

  const top = [...alerts].sort((a, b) => rankSeverity(b.severity) - rankSeverity(a.severity))[0];
  const meta = severityMeta(top.severity);

  return (
    <section className={`alerts-box alerts-box-${meta.cls}`} role="alert">
      {/* Header banner row (like screenshot) */}
      <div className="alerts-box-header">
        <div className="alerts-box-titlewrap">
          <i className={`${meta.icon} alerts-box-icon`} />
          <div className="alerts-box-titletext">
            <span className="alerts-box-title">{meta.label}:</span>{" "}
            <span className="alerts-box-subtitle">
              You have <b>{alerts.length}</b> alert(s). Please review your performance and consider scheduling a meeting
              with your advisor.
            </span>
          </div>
        </div>

        <Button
          label="View all"
          icon="pi pi-arrow-right"
          iconPos="right"
          text
          className="alerts-box-viewbtn"
          onClick={() => navigate("/student/alerts")}
        />
      </div>

      {/* List of alerts */}
      <div className="alerts-box-list">
        {alerts.map((a, idx) => (
          <div key={idx} className="alerts-item">
            <div className="alerts-item-main">
              <div className="alerts-item-title">
                {a.courseCode ? <span className="alerts-item-code">{a.courseCode}</span> : null}
                <span>{a.title}</span>
              </div>
              <div className="alerts-item-message">{a.message}</div>
            </div>

            <Tag value={a.severity} severity={severityToTag(a.severity)} className="alerts-item-tag" />
          </div>
        ))}
      </div>
    </section>
  );
}
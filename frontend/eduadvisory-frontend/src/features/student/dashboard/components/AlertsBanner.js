import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

function severityToPrime(sev) {
  if (sev === "HIGH") return "danger";
  if (sev === "MEDIUM") return "warning";
  return "info";
}

export default function AlertsBanner({ alerts }) {
  const navigate = useNavigate();

  if (!alerts || alerts.length === 0) return null;

  return (
    <Card className="shadow-sm border-0 mb-4">
      <div className="d-flex justify-content-between align-items-start">
        <div>
          <h5 className="mb-1">Alerts</h5>
          <div className="text-muted small">
            You have {alerts.length} alert(s) that need attention.
          </div>
        </div>

        <Button
          label="View all"
          icon="pi pi-arrow-right"
          text
          onClick={() => navigate("/student/alerts")}
        />
      </div>

      <div className="mt-3 d-flex flex-column gap-2">
        {alerts.map((a, idx) => (
          <div
            key={idx}
            className="d-flex justify-content-between align-items-center p-2 rounded border bg-white"
          >
            <div>
              <div className="fw-semibold">{a.title}</div>
              <div className="text-muted small">
                {a.courseCode ? `${a.courseCode} — ` : ""}
                {a.message}
              </div>
            </div>
            <Tag value={a.severity} severity={severityToPrime(a.severity)} />
          </div>
        ))}
      </div>
    </Card>
  );
}
import { NavLink } from "react-router-dom";
import { Button } from "primereact/button";
import { useStudentSummary } from "../../features/student/context/StudentSummaryProvider";
import { useAuth } from "../../app/providers/AuthProvider";

const navItems = [
  { to: "/student/dashboard", label: "Dashboard", icon: "pi pi-home" },
  { to: "/student/progress", label: "My Progress", icon: "pi pi-chart-line" },
  { to: "/student/course-plan", label: "Course Plan", icon: "pi pi-calendar" },
  { to: "/student/alerts", label: "Alerts", icon: "pi pi-exclamation-triangle" },
  { to: "/student/meetings", label: "Meetings", icon: "pi pi-clock" },
  { to: "/student/messages", label: "Messages", icon: "pi pi-envelope" },
  { to: "/student/ai-assistant", label: "AI Assistant", icon: "pi pi-comments" },
];

export default function StudentSidebar() {
    const { summary, loading } = useStudentSummary();
    const { keycloak } = useAuth();

  return (
    <div className="d-flex flex-column h-100 p-3">
      <div className="mb-4">
        <div className="fw-bold">Student Portal</div>
        <div className="text-muted small">University System</div>
      </div>

      <div className="nav nav-pills flex-column gap-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `nav-link d-flex align-items-center gap-2 ${
                isActive ? "active" : "text-dark"
              }`
            }
          >
            <i className={item.icon} />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>

      <div className="mt-auto pt-3 border-top">
  {loading || !summary ? (
    <div className="text-muted small">Loading student...</div>
  ) : (
    <>
      <div className="fw-semibold">{summary.fullName}</div>
      <div className="text-muted small">{summary.studentId}</div>
      <div className="text-muted small">GPA: {summary.currentGpa ?? "-"}</div>
    </>
  )}

  <Button
    className="mt-3 w-100"
    label="Logout"
    icon="pi pi-sign-out"
    severity="secondary"
    onClick={() => keycloak.logout()}
  />
</div>
    </div>
  );
}
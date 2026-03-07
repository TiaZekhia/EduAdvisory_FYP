import { NavLink } from "react-router-dom";
import { Badge } from "primereact/badge";
// import { useAlertsCount } from "../../features/student/alerts/hooks/useAlertsCount";

const navItems = [
  { to: "/student/dashboard", label: "Dashboard", icon: "pi pi-home" },
  { to: "/student/progress", label: "My Progress", icon: "pi pi-chart-line" },
  { to: "/student/course-plan", label: "Course Plan", icon: "pi pi-calendar" },
  { to: "/student/alerts", label: "Alerts", icon: "pi pi-exclamation-triangle" },
  { to: "/student/meetings", label: "Meetings", icon: "pi pi-clock" },
  { to: "/student/messages", label: "Messages", icon: "pi pi-envelope" },
  { to: "/student/ai-assistant", label: "AI Assistant", icon: "pi pi-comments" },
];

export default function StudentSidebarContent({ onNavigate }) {

  // Replace with real hook:
  // const { alertsCount } = useAlertsCount();
  const alertsCount = { count: 3 };

  return (
    <div className="d-flex flex-column h-100 p-3 w-100">
      <div className="mb-4">
        <div className="fw-bold">Student Portal</div>
        <div className="text-muted small">University System</div>
      </div>

      <div className="nav nav-pills flex-column gap-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            onClick={() => onNavigate?.()}
            className={({ isActive }) =>
              `nav-link d-flex align-items-center gap-2 ${
                isActive ? "active" : "text-dark"
              }`
            }
            style={({ isActive }) => ({
              borderRadius: 12,
              padding: "10px 12px",
              background: isActive ? "#111827" : "transparent",
              color: isActive ? "#fff" : "inherit",
            })}
          >
            <i className={item.icon} />
            <span>{item.label}</span>

            {item.label === "Alerts" && (
              <Badge value={alertsCount.count} severity="danger" className="ms-auto" />
            )}
          </NavLink>
        ))}
      </div>
    </div>
  );
}
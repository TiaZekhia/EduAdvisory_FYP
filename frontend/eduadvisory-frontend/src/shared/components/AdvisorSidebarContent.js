import { NavLink } from "react-router-dom";
import { useAdvisorSummary } from "../../features/advisor/context/AdvisorSummaryProvider";
import { Badge } from "primereact/badge";

const navItems = [
  { to: "/advisor/dashboard", label: "Dashboard", icon: "pi pi-th-large" },
  { to: "/advisor/student-analysis", label: "Student Analysis", icon: "pi pi-copy" },
  { to: "/advisor/plan-generator", label: "Plan Generator", icon: "pi pi-calendar" },
  { to: "/advisor/risk-assessment", label: "Risk Assessment", icon: "pi pi-chart-line" },
  { to: "/advisor/meetings", label: "Meetings", icon: "pi pi-calendar-plus" },
  { to: "/advisor/messages", label: "Messages", icon: "pi pi-envelope" },
];

export default function AdvisorSidebarContent({ onNavigate,unreadMessagesCount }) {
  const { summary, loading } = useAdvisorSummary();

  return (
    <div className="d-flex flex-column h-100 p-3 w-100">
      <div className="mb-4">
        <div className="fw-bold">Advisor Portal</div>
        <div className="text-muted small">EduAdvisory System</div>
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
            {item.label === "Messages" && unreadMessagesCount > 0 && (
  <Badge value={unreadMessagesCount} severity="danger" className="ms-auto" />
)}
          </NavLink>
        ))}
      </div>
    </div>
  );
}
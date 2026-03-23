import { NavLink } from "react-router-dom";
import { Button } from "primereact/button";
import { useAuth } from "../../app/providers/AuthProvider";
import { useAdvisorSummary } from "../../features/advisor/context/AdvisorSummaryProvider";

const navItems = [
  { to: "/advisor/dashboard", label: "Dashboard", icon: "pi pi-home" },
  { to: "/advisor/meetings", label: "Meetings", icon: "pi pi-calendar" },
  { to: "/advisor/messages", label: "Messages", icon: "pi pi-envelope" },
];

export default function AdvisorSidebarContent({ onNavigate }) {
  const { keycloak } = useAuth();
  const { summary, loading } = useAdvisorSummary();

  return (
    <div className="d-flex flex-column h-100 p-3">
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
              `nav-link d-flex align-items-center gap-2 advisor-nav-link ${
                isActive ? "active" : ""
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
          <div className="text-muted small">Loading advisor...</div>
        ) : (
          <>
            <div className="fw-semibold">{summary.name}</div>
            <div className="text-muted small">{summary.email}</div>
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
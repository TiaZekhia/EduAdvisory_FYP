import { NavLink } from "react-router-dom";

const navItems = [
  { to: "/admin/users", label: "User Management", icon: "pi pi-users" },
];

export default function AdminSidebarContent({ onNavigate }) {
  return (
    <div className="d-flex flex-column h-100 p-3 w-100">
      <div className="mb-4">
        <div className="fw-bold">Admin Portal</div>
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
          </NavLink>
        ))}
      </div>
    </div>
  );
}
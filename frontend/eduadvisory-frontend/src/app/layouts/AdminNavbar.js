import { Avatar } from "primereact/avatar";
import { Button } from "primereact/button";
import { Menu } from "primereact/menu";
import { useRef } from "react";
import uaLogo from "../../assets/UA_LOGO.png";

export default function AdminNavbar({
  onToggleSidebar,
  sidebarOpen,
  userName = "Admin",
  userEmail = "",
  onLogout,
}) {
  const profileMenuRef = useRef(null);

  const profileItems = [
    { label: "Logout", icon: "pi pi-sign-out", command: onLogout },
  ];

  return (
    <header className="student-navbar">
      <div className="student-navbar-left">
        <Button
          icon={sidebarOpen ? "pi pi-times" : "pi pi-bars"}
          rounded
          text
          type="button"
          onClick={onToggleSidebar}
          aria-label="Open sidebar"
          className="burger-btn"
        />
        <div className="student-navbar-brand">
          <img src={uaLogo} alt="University Logo" className="student-navbar-logo" />
        </div>
      </div>

      <div className="student-navbar-right">
        <div className="student-profile">
          <button
            type="button"
            className="student-profile-btn"
            onClick={(event) => profileMenuRef.current?.toggle(event)}
          >
            <Avatar label={userName?.[0]?.toUpperCase() ?? "A"} shape="circle" />
            <div className="student-profile-meta d-none d-md-flex">
              <div className="fw-semibold">{userName}</div>
              <div className="text-muted small">{userEmail || "Administrator"}</div>
            </div>
            <i className="pi pi-chevron-down ms-2" />
          </button>

          <Menu model={profileItems} popup ref={profileMenuRef} />
        </div>
      </div>
    </header>
  );
}
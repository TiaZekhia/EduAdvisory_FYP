import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import { Avatar } from "primereact/avatar";
import { Menu } from "primereact/menu";
import { useRef } from "react";
import { useNavigate } from "react-router-dom";
import uaLogo from "../../assets/UA_LOGO.png";

export default function AdvisorNavbar({
  onToggleSidebar,
  sidebarOpen,
  messagesCount = 0,
  userName = "Advisor",
  userEmail = "",
  onLogout,
}) {
  const profileMenuRef = useRef(null);
  const navigate = useNavigate();

  const profileItems = [
    { label: "My Profile", icon: "pi pi-user" },
    { label: "Settings", icon: "pi pi-cog" },
    { separator: true },
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
        <Button
          type="button"
          text
          rounded
          icon="pi pi-envelope"
          className="student-icon-btn"
          aria-label="Messages"
          onClick={() => navigate("/advisor/messages")}
        >
          {messagesCount > 0 && (
            <Badge value={messagesCount} severity="info" className="student-badge" />
          )}
        </Button>

        <div className="student-profile">
          <button
            type="button"
            className="student-profile-btn"
            onClick={(e) => profileMenuRef.current?.toggle(e)}
          >
            <Avatar label={userName?.[0] ?? "A"} shape="circle" />
            <div className="student-profile-meta d-none d-md-flex">
              <div className="fw-semibold">{userName}</div>
              <div className="text-muted small">{userEmail}</div>
            </div>
            <i className="pi pi-chevron-down ms-2" />
          </button>

          <Menu model={profileItems} popup ref={profileMenuRef} />
        </div>
      </div>
    </header>
  );
}
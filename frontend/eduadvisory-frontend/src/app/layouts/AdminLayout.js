import { useState } from "react";
import { Outlet } from "react-router-dom";
import { useAuth } from "../providers/AuthProvider";
import AdminNavbar from "./AdminNavbar";
import AdminSidebarContent from "../../shared/components/AdminSidebarContent";
import "./studentLayout.css";

export default function AdminLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const { keycloak } = useAuth();

  return (
    <div className={`student-shell ${sidebarOpen ? "sidebar-open" : "sidebar-closed"}`}>
      <aside className="student-sidebar">
        <AdminSidebarContent />
      </aside>

      <div className="student-maincol">
        <AdminNavbar
          onToggleSidebar={() => setSidebarOpen((value) => !value)}
          sidebarOpen={sidebarOpen}
          userName={keycloak.tokenParsed?.preferred_username ?? "Admin"}
          userEmail={keycloak.tokenParsed?.email ?? ""}
          onLogout={() =>
            keycloak.logout({
              redirectUri: window.location.origin,
            })
          }
        />

        <main className="student-main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
import { useState } from "react";
import { Outlet } from "react-router-dom";
import { Sidebar } from "primereact/sidebar";
import { Button } from "primereact/button";
import StudentSidebarContent from "../../shared/components/StudentSidebarContent";
import "./studentLayout.css";

export default function StudentLayout() {
  const [sidebarVisible, setSidebarVisible] = useState(false);

  return (
    <div className="student-shell">
      {/* ===== Desktop sidebar ===== */}
      <aside className="student-sidebar d-none d-md-flex">
        <StudentSidebarContent />
      </aside>

      {/* ===== Main column (topbar + page content) ===== */}
      <div className="student-maincol">
        {/* Mobile topbar */}
        <div className="student-topbar d-md-none">
          <Button
            icon="pi pi-bars"
            rounded
            text
            type="button"
            onClick={() => setSidebarVisible(true)}
          />
          <div className="ms-2">
            <div className="fw-bold">Student Portal</div>
            <div className="text-muted small">University System</div>
          </div>

          <div className="ms-auto">
            <Button label="Help" outlined size="small" type="button" />
          </div>
        </div>

        {/* Page content scrolls here (sidebar stays fixed) */}
        <main className="student-main">
          <Outlet />
        </main>
      </div>

      {/* ===== Mobile PrimeReact Sidebar ===== */}
      <Sidebar
        visible={sidebarVisible}
        position="left"
        onHide={() => setSidebarVisible(false)}
        showCloseIcon
        dismissable
        blockScroll
        className="student-mobile-sidebar"
      >
        <StudentSidebarContent onNavigate={() => setSidebarVisible(false)} />
      </Sidebar>
    </div>
  );
}
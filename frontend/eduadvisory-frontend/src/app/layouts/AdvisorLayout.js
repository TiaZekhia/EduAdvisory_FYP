import { useState } from "react";
import { Outlet } from "react-router-dom";
import { Sidebar } from "primereact/sidebar";
import { Button } from "primereact/button";
import AdvisorSidebarContent from "../../shared/components/AdvisorSidebarContent";
import { AdvisorSummaryProvider } from "../../features/advisor/context/AdvisorSummaryProvider";
import "./advisorLayout.css";

export default function AdvisorLayout() {
  const [sidebarVisible, setSidebarVisible] = useState(false);

  return (
    <AdvisorSummaryProvider>
      <div className="advisor-shell">
        {/* Mobile topbar */}
        <div className="advisor-topbar d-md-none">
          <Button
            icon="pi pi-bars"
            rounded
            text
            onClick={() => setSidebarVisible(true)}
          />
          <div className="ms-2">
            <div className="fw-bold">Advisor Portal</div>
            <div className="text-muted small">EduAdvisory System</div>
          </div>
        </div>

        {/* Mobile sidebar */}
        <Sidebar
          visible={sidebarVisible}
          position="left"
          onHide={() => setSidebarVisible(false)}
          showCloseIcon
          dismissable
          className="advisor-mobile-sidebar"
        >
          <AdvisorSidebarContent onNavigate={() => setSidebarVisible(false)} />
        </Sidebar>

        {/* Desktop sidebar */}
        <aside className="advisor-sidebar d-none d-md-block">
          <AdvisorSidebarContent />
        </aside>

        {/* Main */}
        <main className="advisor-main">
          <Outlet />
        </main>
      </div>
    </AdvisorSummaryProvider>
  );
}
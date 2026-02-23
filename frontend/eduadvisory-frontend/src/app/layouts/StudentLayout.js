import { useState } from "react";
import { Outlet } from "react-router-dom";
import StudentSidebarContent from "../../shared/components/StudentSidebarContent";
import StudentNavbar from "./StudentNavbar";
import { useAuth } from "../providers/AuthProvider";
import { useStudentSummary } from "../../features/student/context/StudentSummaryProvider";
import "./studentLayout.css";

export default function StudentLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true); // default open (change if you want)
  const { keycloak } = useAuth();
  const { summary } = useStudentSummary();

  const notificationsCount = 3;
  const alertsCount = 6;

  return (
    <div className={`student-shell ${sidebarOpen ? "sidebar-open" : "sidebar-closed"}`}>
      <aside className="student-sidebar">
        <StudentSidebarContent />
      </aside>

      <div className="student-maincol">
        <StudentNavbar
          onToggleSidebar={() => setSidebarOpen((v) => !v)}
          sidebarOpen={sidebarOpen}
          notificationsCount={notificationsCount}
          alertsCount={alertsCount}
          userName={summary?.fullName ?? "Student"}
          userId={summary?.studentId ?? ""}
          onLogout={() => keycloak.logout()}
        />

        <main className="student-main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import StudentSidebarContent from "../../shared/components/StudentSidebarContent";
import StudentNavbar from "./StudentNavbar";
import { useAuth } from "../providers/AuthProvider";
import { useStudentSummary } from "../../features/student/context/StudentSummaryProvider";
import { useStudentAlerts } from "../../features/student/context/StudentAlertsProvider";
import { useMessages } from "../../features/messages/context/MessagesProvider";
import "./studentLayout.css";

export default function StudentLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const { keycloak } = useAuth();
  const { summary } = useStudentSummary();
  const { alertsCount } = useStudentAlerts();
  const { unreadMessagesCount, fetchUnreadCount } = useMessages();

  useEffect(() => {
    if (keycloak?.token) {
      fetchUnreadCount();
    }
  }, [keycloak?.token, fetchUnreadCount]);

  return (
    <div className={`student-shell ${sidebarOpen ? "sidebar-open" : "sidebar-closed"}`}>
      <aside className="student-sidebar">
        <StudentSidebarContent
          alertsCount={alertsCount}
          unreadMessagesCount={unreadMessagesCount}
        />
      </aside>

      <div className="student-maincol">
        <StudentNavbar
          onToggleSidebar={() => setSidebarOpen((v) => !v)}
          sidebarOpen={sidebarOpen}
          notificationsCount={unreadMessagesCount}
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
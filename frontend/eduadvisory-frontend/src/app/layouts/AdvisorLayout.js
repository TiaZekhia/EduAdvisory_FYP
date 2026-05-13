import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import AdvisorSidebarContent from "../../shared/components/AdvisorSidebarContent";
import AdvisorNavbar from "./AdvisorNavbar";
import { useAuth } from "../providers/AuthProvider";
import { useAdvisorSummary } from "../../features/advisor/context/AdvisorSummaryProvider";
import { useMessages } from "../../features/messages/context/MessagesProvider";
import "./studentLayout.css";

export default function AdvisorLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const { keycloak } = useAuth();
  const { summary } = useAdvisorSummary();
  const { unreadMessagesCount, fetchUnreadCount } = useMessages();

  useEffect(() => {
    if (keycloak?.token) {
      fetchUnreadCount();
    }
  }, [keycloak?.token, fetchUnreadCount]);

  return (
    <div className={`student-shell ${sidebarOpen ? "sidebar-open" : "sidebar-closed"}`}>
      <aside className="student-sidebar">
        <AdvisorSidebarContent unreadMessagesCount={unreadMessagesCount} />
      </aside>

      <div className="student-maincol">
        <AdvisorNavbar
          onToggleSidebar={() => setSidebarOpen((v) => !v)}
          sidebarOpen={sidebarOpen}
          messagesCount={unreadMessagesCount}
          userName={summary?.name ?? "Advisor"}
          userEmail={summary?.email ?? ""}
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
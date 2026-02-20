import { Outlet } from "react-router-dom";
import StudentSidebar from "../../shared/components/StudentSidebar";

export default function StudentLayout() {
  return (
    <div className="d-flex min-vh-100">
      {/* Sidebar */}
      <aside
        style={{ width: 260 }}
        className="border-end bg-white"
      >
        <StudentSidebar />
      </aside>

      {/* Main content */}
      <main className="flex-grow-1 bg-light">
        <Outlet />
      </main>
    </div>
  );
}
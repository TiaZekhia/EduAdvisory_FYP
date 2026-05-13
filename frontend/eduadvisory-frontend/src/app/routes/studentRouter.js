import { createBrowserRouter, Navigate } from "react-router-dom";
import ProtectedRoute from "../../shared/components/ProtectedRoute";
import { StudentSummaryProvider } from "../../features/student/context/StudentSummaryProvider";
import { StudentAlertsProvider } from "../../features/student/context/StudentAlertsProvider";
import StudentLayout from "../layouts/StudentLayout";
import DashboardPage from "../../features/student/dashboard/DashboardPage";
import MyProgressPage from "../../features/student/progress/MyProgressPage";
import CoursePlanPage from "../../features/student/coursePlan/CoursePlanPage";
import AlertsPage from "../../features/student/alerts/AlertsPage";
import MeetingsPage from "../../features/student/meetings/MeetingsPage";
import StudentMessagesPage from "../../features/messages/pages/StudentMessagesPage";
import AIAssistantPage from "../../features/student/ai/AIAssistantPage";
import StudentProfilePage from "../../features/student/profile/StudentProfilePage";
import NotFoundPage from "../../shared/pages/NotFoundPage";
export const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/student/dashboard" replace />,
  },
  {
    path: "/student",
  element: (
  <ProtectedRoute allowedRoles={["STUDENT"]}>
    <StudentSummaryProvider>
      <StudentAlertsProvider>
        <StudentLayout />
      </StudentAlertsProvider>  
    </StudentSummaryProvider>
  </ProtectedRoute>
),
    children: [
      { index: true, element: <Navigate to="dashboard" replace /> },
      { path: "dashboard", element: <DashboardPage /> },
      { path: "profile", element: <StudentProfilePage /> },
      { path: "progress", element: <MyProgressPage /> },
      { path: "course-plan", element: <CoursePlanPage /> },
      { path: "alerts", element: <AlertsPage /> },
      { path: "meetings", element: <MeetingsPage /> },
      { path: "messages", element: <StudentMessagesPage /> },
      { path: "ai-assistant", element: <AIAssistantPage /> },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
import { createBrowserRouter, Navigate } from "react-router-dom";
import ProtectedRoute from "../../shared/components/ProtectedRoute";
import AdvisorLayout from "../layouts/AdvisorLayout";
import AdvisorDashboardPage from "../../features/advisor/dashboard/AdvisorDashboardPage";
import AdvisorMeetingsPage from "../../features/advisor/meetings/AdvisorMeetingsPage";
import AdvisorMessagesPage from "../../features/advisor/messages/AdvisorMessagesPage";
import { AdvisorSummaryProvider } from "../../features/advisor/context/AdvisorSummaryProvider";
import NotFoundPage from "../../shared/pages/NotFoundPage";

export const advisorRouter = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/advisor/dashboard" replace />,
  },
  {
    path: "/advisor",
    element: (
      <ProtectedRoute allowedRoles={["ADVISOR"]}>
        <AdvisorSummaryProvider>
          <AdvisorLayout />
        </AdvisorSummaryProvider>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="dashboard" replace /> },
      { path: "dashboard", element: <AdvisorDashboardPage /> },
      { path: "meetings", element: <AdvisorMeetingsPage /> },
      { path: "messages", element: <AdvisorMessagesPage /> },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
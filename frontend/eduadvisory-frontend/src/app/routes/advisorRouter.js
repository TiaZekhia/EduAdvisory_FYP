import { createBrowserRouter, Navigate } from "react-router-dom";
import ProtectedRoute from "../../shared/components/ProtectedRoute";
import AdvisorLayout from "../layouts/AdvisorLayout";
import AdvisorDashboardPage from "../../features/advisor/dashboard/AdvisorDashboardPage";
import AdvisorStudentAnalysisPage from "../../features/advisor/studentAnalysis/AdvisorStudentAnalysisPage";
import AdvisorMeetingsPage from "../../features/advisor/meetings/AdvisorMeetingsPage";
import AdvisorMessagesPage from "../../features/advisor/messages/AdvisorMessagesPage";
import { AdvisorSummaryProvider } from "../../features/advisor/context/AdvisorSummaryProvider";
import NotFoundPage from "../../shared/pages/NotFoundPage";
import AdvisorCoursePlanPage from "../../features/advisor/coursePlan/AdvisorCoursePlanPage";
import AdvisorRiskAssessmentPage from "../../features/advisor/riskAssessment/AdvisorRiskAssessmentPage";

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
      { path: "student-analysis", element: <AdvisorStudentAnalysisPage /> },
      { path: "plan-generator", element: <AdvisorCoursePlanPage /> },
      { path: "risk-assessment", element: <AdvisorRiskAssessmentPage /> },
      { path: "meetings", element: <AdvisorMeetingsPage /> },
      { path: "messages", element: <AdvisorMessagesPage /> },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
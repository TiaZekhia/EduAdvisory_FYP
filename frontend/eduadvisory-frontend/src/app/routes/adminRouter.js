import { createBrowserRouter, Navigate } from "react-router-dom";
import ProtectedRoute from "../../shared/components/ProtectedRoute";
import AdminLayout from "../layouts/AdminLayout";
import AdminUserManagementPage from "../../features/admin/userManagement/AdminUserManagementPage";
import NotFoundPage from "../../shared/pages/NotFoundPage";
import AiKnowledgeBasePage from "../../features/admin/aiKnowledgeBase/AiKnowledgeBasePage";

export const adminRouter = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/admin/users" replace />,
  },
  {
    path: "/admin",
    element: (
      <ProtectedRoute allowedRoles={["ADMIN"]}>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="users" replace /> },
      { path: "users", element: <AdminUserManagementPage /> },
      { path: "ai-knowledge-base", element: <AiKnowledgeBasePage /> },
    ]
  },
  { path: "*", element: <NotFoundPage /> },
]);
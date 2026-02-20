import { Navigate } from "react-router-dom";
import { useAuth } from "../../app/providers/AuthProvider";

export default function ProtectedRoute({ allowedRoles = [], children }) {
  const { isAuthenticated, hasRole } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (allowedRoles.length > 0 && !allowedRoles.some((r) => hasRole(r))) {
    return (
      <div className="p-4">
        <h3>Not Authorized</h3>
        <p>You don’t have permission to access this page.</p>
      </div>
    );
  }

  return children;
}
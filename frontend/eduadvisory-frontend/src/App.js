import { RouterProvider } from "react-router-dom";
import { router as studentRouter } from "./app/routes/studentRouter";
import { advisorRouter } from "./app/routes/advisorRouter";
import { AuthProvider } from "./app/providers/AuthProvider";
import { MessagesProvider } from "./features/messages/context/MessagesProvider";
import { PrimeReactProvider } from "primereact/api";
import { useAuth } from "./app/providers/AuthProvider";
import { useEffect } from "react";
import "../src/features/messages/pages/messaging.css";

function AppRouterSelector() {
  const { roles } = useAuth();

  useEffect(() => {
    const path = window.location.pathname;

    if (roles.includes("ADVISOR") && path.startsWith("/student")) {
      window.history.replaceState({}, "", "/advisor/dashboard");
      window.dispatchEvent(new PopStateEvent("popstate"));
    }

    if (roles.includes("STUDENT") && path.startsWith("/advisor")) {
      window.history.replaceState({}, "", "/student/dashboard");
      window.dispatchEvent(new PopStateEvent("popstate"));
    }
  }, [roles]);

  if (roles.includes("ADVISOR")) {
    return (
      <MessagesProvider>
        <RouterProvider router={advisorRouter} />
      </MessagesProvider>
    );
  }

  if (roles.includes("STUDENT")) {
    return (
      <MessagesProvider>
        <RouterProvider router={studentRouter} />
      </MessagesProvider>
    );
  }

  return <div className="p-4">Unauthorized role</div>;
}

export default function App() {
  return (
    <PrimeReactProvider>
      <AuthProvider>
        <AppRouterSelector />
      </AuthProvider>
    </PrimeReactProvider>
  );
}
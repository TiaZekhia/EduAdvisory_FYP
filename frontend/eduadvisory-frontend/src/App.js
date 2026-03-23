import { RouterProvider } from "react-router-dom";
import { router as studentRouter } from "./app/routes/studentRouter";
import { advisorRouter } from "./app/routes/advisorRouter";
import { AuthProvider } from "./app/providers/AuthProvider";
import { PrimeReactProvider } from "primereact/api";
import { useAuth } from "./app/providers/AuthProvider";

function AppRouterSelector() {
  const { roles } = useAuth();

  if (roles.includes("ADVISOR")) {
    return <RouterProvider router={advisorRouter} />;
  }

  if (roles.includes("STUDENT")) {
    return <RouterProvider router={studentRouter} />;
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
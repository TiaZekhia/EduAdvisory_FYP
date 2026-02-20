import React, { createContext, useContext, useMemo } from "react";
import keycloak from "../../keycloak";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const value = useMemo(() => {
    const roles =
      keycloak.tokenParsed?.realm_access?.roles ?? [];

    return {
      keycloak,
      isAuthenticated: !!keycloak.authenticated,
      username: keycloak.tokenParsed?.preferred_username ?? "",
      roles,
      hasRole: (role) => roles.includes(role),
    };
  }, []);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
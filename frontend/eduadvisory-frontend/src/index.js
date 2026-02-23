import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import keycloak from "./keycloak";
import "bootstrap/dist/css/bootstrap.min.css";

import "primereact/resources/themes/lara-light-indigo/theme.css"; // or your theme
import "primereact/resources/primereact.min.css";
import "primeicons/primeicons.css";

keycloak.init({
  onLoad: "login-required",
  checkLoginIframe: false,
  pkceMethod: 'S256'
}).then((authenticated) => {
  if (authenticated) {
    
    
    ReactDOM.createRoot(document.getElementById("root")).render(
      <React.StrictMode>
        <App keycloak={keycloak} />
      </React.StrictMode>
    );
  } else {
    console.log("❌ User not authenticated");
    keycloak.login();
  }
}).catch((error) => {
  console.error("❌ Keycloak initialization failed:", error);
});
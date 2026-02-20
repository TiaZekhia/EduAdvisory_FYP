import { useEffect, useState } from "react";
import api from "./services/api";
import keycloak from "./keycloak";

function App() {
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        console.log("TOKEN PARSED:", keycloak.tokenParsed);
        console.log("TOKEN:", keycloak.token ? "EXISTS" : "MISSING");
        console.log("IS TOKEN EXPIRED?", keycloak.isTokenExpired());
        
        // Ensure token is fresh
        await keycloak.updateToken(30);
        
        const response = await api.get("/StudentAnalysis/student/202110001");
        console.log("✅ Response:", response.data);
      } catch (err) {
        console.error("❌ Error:", err);
        if (err.response) {
          console.error("Response status:", err.response.status);
          console.error("Response data:", err.response.data);
        }
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return <h1>Loading...</h1>;
  }

  return <h1>EduAdvisory</h1>;
}

export default App;
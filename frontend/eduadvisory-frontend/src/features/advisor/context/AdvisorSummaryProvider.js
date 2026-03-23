import { createContext, useContext, useEffect, useState } from "react";
import { advisorApi } from "../../../services/advisors/advisorApi";

const AdvisorSummaryContext = createContext(null);

export function AdvisorSummaryProvider({ children }) {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    advisorApi.getMySummary()
      .then((res) => setSummary(res.data))
      .catch((err) => console.error("Advisor summary load error:", err))
      .finally(() => setLoading(false));
  }, []);

  return (
    <AdvisorSummaryContext.Provider value={{ summary, loading }}>
      {children}
    </AdvisorSummaryContext.Provider>
  );
}

export function useAdvisorSummary() {
  return useContext(AdvisorSummaryContext);
}
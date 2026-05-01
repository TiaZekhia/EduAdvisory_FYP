import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { studentApi } from "../../../services/students/studentApi";

const StudentAlertsContext = createContext(null);

export function StudentAlertsProvider({ children }) {
  const [alerts, setAlerts] = useState([]);
  const [alertsCount, setAlertsCount] = useState(0);
  const [highCount, setHighCount] = useState(0);
  const [mediumCount, setMediumCount] = useState(0);
  const [lowCount, setLowCount] = useState(0);
  const [loading, setLoading] = useState(true);

  const loadAlerts = async () => {
    try {
      setLoading(true);

      const res = await studentApi.getMyAlerts();
      const data = res.data ?? {};

      setAlerts(data.alerts ?? []);
      setAlertsCount(data.count ?? 0);
      setHighCount(data.high ?? 0);
      setMediumCount(data.medium ?? 0);
      setLowCount(data.low ?? 0);
    } catch (err) {
      console.error("Failed to load alerts:", err);
      setAlerts([]);
      setAlertsCount(0);
      setHighCount(0);
      setMediumCount(0);
      setLowCount(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAlerts();
  }, []);

  const value = useMemo(() => {
    const latestAlerts = alerts.slice(0, 3);

    return {
      alerts,
      latestAlerts,
      alertsCount,
      highCount,
      mediumCount,
      lowCount,
      loading,
      refreshAlerts: loadAlerts,
    };
  }, [alerts, alertsCount, highCount, mediumCount, lowCount, loading]);

  return (
    <StudentAlertsContext.Provider value={value}>
      {children}
    </StudentAlertsContext.Provider>
  );
}

export function useStudentAlerts() {
  const ctx = useContext(StudentAlertsContext);
  if (!ctx) {
    throw new Error("useStudentAlerts must be used within StudentAlertsProvider");
  }
  return ctx;
}
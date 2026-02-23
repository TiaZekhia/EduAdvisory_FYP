import { useEffect, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useAlerts(limit = 3) {
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentApi
      .getMyAlerts(limit)
      .then((res) => setAlerts(res.data))
      .catch((err) => console.error("Failed to load alerts:", err))
      .finally(() => setLoading(false));
  }, [limit]);

  return { alerts, loading };
}
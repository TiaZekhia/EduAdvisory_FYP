import { useEffect, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useProgressSummary() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentApi.getMyProgressSummary()
      .then(res => setData(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  return { data, loading };
}
import { useEffect, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useAlertsCount() {
  const [count, setCount] = useState({ count: 0, high: 0, medium: 0, low: 0 });

  useEffect(() => {
    studentApi.getMyAlertsCount()
      .then((res) => setCount(res.data))
      .catch(console.error);
  }, []);

  return count;
}
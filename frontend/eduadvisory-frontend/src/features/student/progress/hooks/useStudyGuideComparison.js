import { useEffect, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useStudyGuideComparison() {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentApi.getMyStudyGuideComparison()
      .then(res => setData(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  return { data, loading };
}
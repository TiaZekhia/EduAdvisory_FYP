import { useEffect, useMemo, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useStudentDashboardCourses() {
  const [enrollment, setEnrollment] = useState([]);
  const [performance, setPerformance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const [e, p] = await Promise.all([
          studentApi.getMyCurrentEnrollment(),
          studentApi.getMyCurrentCoursesPerformance(),
        ]);
        setEnrollment(e.data);
        setPerformance(p.data);
      } catch (err) {
        console.error(err);
        setError("Failed to load current courses.");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const creditsEnrolled = useMemo(
    () => enrollment.reduce((sum, c) => sum + (c.credits ?? 0), 0),
    [enrollment]
  );

  return { enrollment, performance, creditsEnrolled, loading, error };
}
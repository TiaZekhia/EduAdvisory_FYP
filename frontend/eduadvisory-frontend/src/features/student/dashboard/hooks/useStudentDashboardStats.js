import { useEffect, useState } from "react";
import { studentApi } from "../../../../services/students/studentApi";

export function useStudentDashboardStats() {
  const [stats, setStats] = useState(null); // completed/failed/creditsEarned
  const [degreeProgress, setDegreeProgress] = useState(null); // earned/required/percent
  const [upcomingMeetingsCount, setUpcomingMeetingsCount] = useState(0);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const [s, dp, m] = await Promise.all([
          studentApi.getMyStats(),
          studentApi.getMyDegreeProgress(),
          studentApi.getMyUpcomingMeetingsCount(),
        ]);

        setStats(s.data);
        setDegreeProgress(dp.data);
        setUpcomingMeetingsCount(m.data?.upcomingMeetingsCount ?? 0);
      } catch (err) {
        console.error(err);
        setError("Failed to load dashboard stats.");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return { stats, degreeProgress, upcomingMeetingsCount, loading, error };
}
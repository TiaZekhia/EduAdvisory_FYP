import React, { createContext, useContext, useEffect, useState } from "react";
import { studentApi } from "../../../services/students/studentApi";

const StudentSummaryContext = createContext(null);

export function StudentSummaryProvider({ children }) {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentApi
      .getMySummary()
      .then((res) => setSummary(res.data))
      .catch((err) => console.error("Failed to load student summary:", err))
      .finally(() => setLoading(false));
  }, []);

  return (
    <StudentSummaryContext.Provider value={{ summary, loading }}>
      {children}
    </StudentSummaryContext.Provider>
  );
}

export function useStudentSummary() {
  return useContext(StudentSummaryContext);
}
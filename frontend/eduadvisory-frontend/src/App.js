/*import { useEffect, useState } from "react";
import api from "./services/api";

function App() {
  const [summary, setSummary] = useState(null);

  useEffect(() => {
    api.get("/students/me/summary")
    //api.get("/StudentAnalysis/me")
      .then(res => setSummary(res.data))
      .catch(err => console.error(err));
  }, []);

  if (!summary) return <h1>Loading...</h1>;

  return (
    <div>
      <h1>Welcome back, {summary.fullName}!</h1>
      <p>{summary.programCode} • Semester {summary.currentSemester}</p>
      <p>GPA: {summary.currentGpa}</p>
      <p>Status: {summary.academicStatus}</p>
    </div>
  );
  /*return (
  <div>
    <h1>Student ID: {summary.studentId}</h1>
    <p>On Track: {summary.isOnTrack ? "Yes" : "No"}</p>

    <h3>Missing Courses</h3>
    <pre>{JSON.stringify(summary.missingCurrentSemesterCourses, null, 2)}</pre>

    <h3>Failed Not Retaken</h3>
    <pre>{JSON.stringify(summary.failedNotRetakenCourses, null, 2)}</pre>

    <h3>Blocking Courses</h3>
    <pre>{JSON.stringify(summary.blockingCourses, null, 2)}</pre>
  </div>
); */
//}*/

//export default App;

/*import api from "../services/apiClient";
import { useEffect, useState } from "react";
import { getMySummary, getMyCurrentEnrollment } from "../services/studentService";

function App() {
  const [summary, setSummary] = useState(null);
  const [enrollment, setEnrollment] = useState([]);

  useEffect(() => {
    Promise.all([getMySummary(), getMyCurrentEnrollment()])
      .then(([s, e]) => {
        setSummary(s.data);
        setEnrollment(e.data);
      })
      .catch(console.error);
      api.get("/students/me/current-courses/performance")
      .then(res => console.log(res.data))
      .catch(console.error);
  }, []);

  if (!summary) return <h1>Loading...</h1>;

  const creditsEnrolled = enrollment.reduce((sum, c) => sum + c.credits, 0);

  return (
    <div>
      <h1>Welcome back, {summary.fullName}!</h1>
      <p>{summary.programCode} • Semester {summary.currentSemester}</p>
      <p>Credits enrolled: {creditsEnrolled}</p>

      <h2>Current Courses</h2>
      <ul>
        {enrollment.map(c => (
          <li key={c.courseCode}>
            {c.courseCode} - {c.courseName} ({c.credits} credits)
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;*/


import { RouterProvider } from "react-router-dom";
import { router } from "./app/routes";
import { AuthProvider } from "./app/providers/AuthProvider";

export default function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}
export default function StudentSelector({ students, value, onChange, loading }) {
  return (
    <select
      className="form-select"
      value={value}
      onChange={(e) => onChange(e.target.value)}
      disabled={loading}
    >
      <option value="">Select a student</option>
      {students.map((student) => (
        <option key={student.studentId} value={student.studentId}>
          {student.name} ({student.studentId}) - GPA: {student.gpa}
        </option>
      ))}
    </select>
  );
}
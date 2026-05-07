import Select from "react-select";

export default function StudentSelector({
  students,
  value,
  onChange,
  loading,
}) {
  const options = students.map((student) => ({
    value: student.studentId,
    label: `${student.name} (${student.studentId}) - GPA: ${student.gpa}`,
  }));

  const selectedOption =
    options.find((option) => String(option.value) === String(value)) ?? null;

  return (
    <Select
      options={options}
      value={selectedOption}
      onChange={(selected) => onChange(selected?.value ?? "")}
      isDisabled={loading}
      isSearchable={true}
      placeholder="Select a student"
      menuPortalTarget={document.body}
      menuPosition="fixed"
      menuShouldScrollIntoView={false}
      filterOption={(option, inputValue) =>
        option.label.toLowerCase().includes(inputValue.toLowerCase())
      }
      styles={{
        menuPortal: (base) => ({
          ...base,
          zIndex: 9999,
        }),
      }}
    />
  );
}
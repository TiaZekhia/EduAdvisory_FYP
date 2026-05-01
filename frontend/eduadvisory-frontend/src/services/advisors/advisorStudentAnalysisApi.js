import apiClient from "../apiClient";

export const getAdvisorStudentsOverview = async () => {
  const response = await apiClient.get("/Advisors/me/students-overview");
  return response.data;
};

export const getAdvisorStudentAnalysis = async (studentId) => {
  const response = await apiClient.get(`/advisors/me/students/${studentId}/analysis`);
  return response.data;
};
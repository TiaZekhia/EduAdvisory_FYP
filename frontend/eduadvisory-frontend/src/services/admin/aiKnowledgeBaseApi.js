import apiClient from "../apiClient";

export const getAiDocuments = async () => {
  const response = await apiClient.get("/admin/ai-documents");
  return response.data;
};

export const uploadAiDocument = async (formData) => {
  const response = await apiClient.post("/admin/ai-documents/upload", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data;
};

export const deleteAiDocument = async (documentId) => {
  await apiClient.delete(`/admin/ai-documents/${documentId}`);
};

export const reprocessAiDocument = async (documentId) => {
  const response = await apiClient.post(`/admin/ai-documents/${documentId}/reprocess`);
  return response.data;
};
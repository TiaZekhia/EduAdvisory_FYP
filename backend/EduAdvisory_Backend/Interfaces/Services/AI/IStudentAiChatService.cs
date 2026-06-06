using EduAdvisory_Backend.DTOs.AI.StudentChat;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IStudentAiChatService
{
    Task<StudentAiChatResponse> ChatAsync(
        int studentId,
        StudentAiChatRequest request,
        CancellationToken cancellationToken = default);

    Task StreamChatAsync(
        int studentId,
        StudentAiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default);
}
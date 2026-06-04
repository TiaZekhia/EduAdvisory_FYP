using System.Text;
using EduAdvisory_Backend.DTOs.AI.StudentChat;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EduAdvisory_Backend.Services.AI;

public class StudentAiChatService : IStudentAiChatService
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IAiKernelService _kernelService;
    private readonly AiOptions _aiOptions;
    private readonly ICurrentAiStudentContext _studentContext;
    private readonly ILogger<StudentAiChatService> _logger;

    public StudentAiChatService(
        EduAdvisoryDbContext dbContext,
        IVectorSearchService vectorSearchService,
        IAiKernelService kernelService,
        IOptions<AiOptions> aiOptions,
        ILogger<StudentAiChatService> logger,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _vectorSearchService = vectorSearchService;
        _kernelService = kernelService;
        _aiOptions = aiOptions.Value;
        _logger = logger;
        _studentContext = studentContext;
    }

    public async Task<StudentAiChatResponse> ChatAsync(
        int studentId,
        StudentAiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var studentExists = await _dbContext.SisStudents
            .AsNoTracking()
            .AnyAsync(s => s.StudentId == studentId, cancellationToken);

        if (!studentExists)
        {
            throw new InvalidOperationException("Student profile was not found.");
        }

        _studentContext.StudentId = studentId;

        var session = await GetOrCreateSessionAsync(
            studentId,
            request.SessionId,
            cancellationToken);

        var retrievedChunks = await _vectorSearchService.SearchRelevantChunksAsync(
            request.Message,
            studentId,
            request.CourseCode,
            cancellationToken);

        var topScore = retrievedChunks.FirstOrDefault()?.SimilarityScore;

        await SaveMessageAsync(
            session.SessionId,
            "user",
            request.Message,
            cancellationToken);

        if (!retrievedChunks.Any() ||
            topScore == null ||
            topScore < _aiOptions.MinSimilarityThreshold)
        {
            var pluginAnswer = await GeneratePluginAnswerAsync(
                request.Message,
                cancellationToken);

            await SaveMessageAsync(
                session.SessionId,
                "assistant",
                pluginAnswer,
                cancellationToken);

            await LogRetrievalAsync(
                studentId,
                session.SessionId,
                request.Message,
                retrievedChunks,
                "plugin",
                cancellationToken);

            return new StudentAiChatResponse
            {
                SessionId = session.SessionId,
                Answer = pluginAnswer,
                ResponseSource = "plugin",
                TopSimilarityScore = topScore,
                Sources = MapSources(retrievedChunks)
            };
        }

        var answer = await GenerateRagAnswerAsync(
            request.Message,
            retrievedChunks,
            cancellationToken);

        await SaveMessageAsync(
            session.SessionId,
            "assistant",
            answer,
            cancellationToken);

        await LogRetrievalAsync(
            studentId,
            session.SessionId,
            request.Message,
            retrievedChunks,
            "rag",
            cancellationToken);

        return new StudentAiChatResponse
        {
            SessionId = session.SessionId,
            Answer = answer,
            ResponseSource = "rag",
            TopSimilarityScore = topScore,
            Sources = MapSources(retrievedChunks)
        };
    }

    private async Task<string> GenerateRagAnswerAsync(
        string question,
        List<Models.RetrievedChunk> chunks,
        CancellationToken cancellationToken)
    {
        var kernel = _kernelService.CreateKernel();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var contextBuilder = new StringBuilder();

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];

            contextBuilder.AppendLine($"[Source {i + 1}]");
            contextBuilder.AppendLine($"Document: {chunk.DocumentTitle}");
            contextBuilder.AppendLine($"Type: {chunk.DocumentType}");
            contextBuilder.AppendLine($"Course: {chunk.CourseCode}");
            contextBuilder.AppendLine($"Section: {chunk.SectionTitle ?? "Unknown"}");
            contextBuilder.AppendLine($"Page: {chunk.PageNumber?.ToString() ?? "Unknown"}");
            contextBuilder.AppendLine("Content:");
            contextBuilder.AppendLine(chunk.ChunkText);
            contextBuilder.AppendLine();
        }

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage("""
You are EduAdvisory's student academic support assistant.

Rules:
- Answer only using the provided context from study guides and course syllabuses.
- If the context does not contain the answer, say you do not have enough information.
- Do not invent course rules, deadlines, grades, instructor instructions, or policies.
- Be clear, supportive, and student-friendly.
- If useful, structure the answer as short bullets.
- Mention the source section or page when relevant.
""");

        chatHistory.AddUserMessage($$"""
Student question:
{{question}}

Retrieved academic context:
{{contextBuilder}}
""");

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            cancellationToken: cancellationToken);

        return response.Content?.Trim()
               ?? "I could not generate an answer from the available context.";
    }

    private async Task<string> GeneratePluginAnswerAsync(
        string question,
        CancellationToken cancellationToken)
    {
        var kernel = _kernelService.CreateKernel();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage("""
You are EduAdvisory's student academic support assistant.

The RAG documents did not contain enough information to answer the student's question.

You may use available plugin functions only when the question requires live student data such as:
- enrolled courses
- academic profile
- GPA
- grades or progress
- upcoming meetings
- general academic support advice

Rules:
- Only answer about the authenticated current student.
- Do not expose other students' information.
- Do not invent data.
- If no plugin can answer, say that the information is not available.
- Keep the answer concise and supportive.
""");

        chatHistory.AddUserMessage(question);

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel,
            cancellationToken);

        return response.Content?.Trim()
               ?? "I could not find enough information to answer that.";
    }

    private async Task<AiChatSession> GetOrCreateSessionAsync(
        int studentId,
        int? sessionId,
        CancellationToken cancellationToken)
    {
        if (sessionId.HasValue)
        {
            var existing = await _dbContext.AiChatSessions
                .FirstOrDefaultAsync(s =>
                    s.SessionId == sessionId.Value &&
                    s.StudentId == studentId,
                    cancellationToken);

            if (existing != null)
            {
                existing.LastActivityAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return existing;
            }
        }

        var session = new AiChatSession
        {
            StudentId = studentId,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _dbContext.AiChatSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    private async Task SaveMessageAsync(
        int sessionId,
        string role,
        string message,
        CancellationToken cancellationToken)
    {
        _dbContext.AiChatMessages.Add(new AiChatMessage
        {
            SessionId = sessionId,
            Role = role,
            Message = message,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task LogRetrievalAsync(
        int studentId,
        int sessionId,
        string question,
        List<Models.RetrievedChunk> chunks,
        string responseSource,
        CancellationToken cancellationToken)
    {
        _dbContext.AiRetrievalLogs.Add(new AiRetrievalLog
        {
            StudentId = studentId,
            SessionId = sessionId,
            Question = question,
            RetrievedChunkIds = string.Join(",", chunks.Select(c => c.ChunkId)),
            TopSimilarityScore = chunks.FirstOrDefault()?.SimilarityScore,
            ResponseSource = responseSource,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<AiSourceDto> MapSources(
        List<Models.RetrievedChunk> chunks)
    {
        return chunks.Select(c => new AiSourceDto
        {
            ChunkId = c.ChunkId,
            DocumentId = c.DocumentId,
            Title = c.DocumentTitle,
            DocumentType = c.DocumentType,
            CourseCode = c.CourseCode,
            SectionTitle = c.SectionTitle,
            PageNumber = c.PageNumber,
            SimilarityScore = c.SimilarityScore
        }).ToList();
    }
}
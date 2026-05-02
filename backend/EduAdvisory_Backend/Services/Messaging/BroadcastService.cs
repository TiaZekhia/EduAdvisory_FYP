using EduAdvisory_Backend.DTOs.Broadcasts;
using EduAdvisory_Backend.Hubs;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Repositories.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services.Messaging;

public class BroadcastService : IBroadcastService
{
    private readonly EduAdvisoryDbContext _context;
    private readonly IBroadcastRepository _broadcastRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public BroadcastService(
        EduAdvisoryDbContext context,
        IBroadcastRepository broadcastRepository,
        IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _broadcastRepository = broadcastRepository;
        _hubContext = hubContext;
    }

    public async Task<BroadcastDto> CreateBroadcastAsync(string keycloakId, CreateBroadcastDto dto)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() != "advisor")
            throw new Exception("Only advisors can create broadcasts.");

        if (user.LinkedAdvisorId == null)
            throw new Exception("Advisor account is not linked to an advisor profile.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new Exception("Broadcast title is required.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new Exception("Broadcast content is required.");

        var advisorId = user.LinkedAdvisorId.Value;

        var studentsQuery = _context.SisStudents
    .Where(s => s.AdvisorId == advisorId);

        if (dto.StudentIds.Any())
        {
            studentsQuery = studentsQuery.Where(s => dto.StudentIds.Contains(s.StudentId));
        }

        var students = await studentsQuery.ToListAsync();

        if (!students.Any())
            throw new Exception("No students selected for this broadcast.");

        var broadcast = new BroadcastMessage
        {
            AdvisorId = advisorId,
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            Recipients = students.Select(s => new BroadcastRecipient
            {
                StudentId = s.StudentId,
                IsRead = false
            }).ToList()
        };

        var savedBroadcast = await _broadcastRepository.CreateBroadcastAsync(broadcast);

        savedBroadcast = await _context.BroadcastMessages
            .Include(b => b.Advisor)
            .Include(b => b.Recipients)
            .FirstAsync(b => b.BroadcastMessageId == savedBroadcast.BroadcastMessageId);

        var broadcastDto = ToBroadcastDto(savedBroadcast, false, null);

        var studentUsers = await _context.Users
            .Where(u =>
                u.LinkedStudentId != null &&
                students.Select(s => s.StudentId).Contains(u.LinkedStudentId.Value) &&
                u.KeycloakId != null)
            .ToListAsync();

        foreach (var studentUser in studentUsers)
        {
            await _hubContext.Clients.User(studentUser.KeycloakId!)
                .SendAsync("ReceiveBroadcast", broadcastDto);
        }

        return broadcastDto;
    }

    public async Task<List<BroadcastDto>> GetMyBroadcastsAsync(string keycloakId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() == "advisor")
        {
            if (user.LinkedAdvisorId == null)
                throw new Exception("Advisor account is not linked to an advisor profile.");

            var broadcasts = await _broadcastRepository
                .GetBroadcastsForAdvisorAsync(user.LinkedAdvisorId.Value);

            return broadcasts
                .Select(b => ToBroadcastDto(b, false, null))
                .ToList();
        }

        if (user.Role?.ToLower() == "student")
        {
            if (user.LinkedStudentId == null)
                throw new Exception("Student account is not linked to a student profile.");

            var recipients = await _broadcastRepository
                .GetBroadcastsForStudentAsync(user.LinkedStudentId.Value);

            return recipients
                .Select(r => ToBroadcastDto(
                    r.BroadcastMessage,
                    r.IsRead,
                    r.ReadAt))
                .ToList();
        }

        throw new Exception("Only advisors and students can view broadcasts.");
    }

    public async Task MarkBroadcastAsReadAsync(string keycloakId, int broadcastMessageId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() != "student")
            throw new Exception("Only students can mark broadcasts as read.");

        if (user.LinkedStudentId == null)
            throw new Exception("Student account is not linked to a student profile.");

        var recipient = await _broadcastRepository.GetRecipientAsync(
            broadcastMessageId,
            user.LinkedStudentId.Value);

        if (recipient == null)
            throw new Exception("Broadcast not found for this student.");

        await _broadcastRepository.MarkAsReadAsync(recipient);
    }

    private async Task<User> GetCurrentUserAsync(string keycloakId)
    {
        var user = await _context.Users
            .Include(u => u.LinkedAdvisor)
            .Include(u => u.LinkedStudent)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user == null)
            throw new Exception("User not found in local database.");

        return user;
    }

    private BroadcastDto ToBroadcastDto(
        BroadcastMessage broadcast,
        bool isRead,
        DateTime? readAt)
    {
        return new BroadcastDto
        {
            BroadcastMessageId = broadcast.BroadcastMessageId,
            AdvisorId = broadcast.AdvisorId,
            AdvisorName = broadcast.Advisor?.Name ?? "Advisor",
            Title = broadcast.Title,
            Content = broadcast.Content,
            CreatedAt = broadcast.CreatedAt,
            IsRead = isRead,
            ReadAt = readAt
        };
    }
}
using EduAdvisory_Backend.DTOs.Automation;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services;

public class RiskAutomationService : IRiskAutomationService
{
    private readonly EduAdvisoryDbContext _context;
    private readonly IStudentRiskAssessmentService _riskService;
    private const int COOLDOWN_DAYS = 14;

    public RiskAutomationService(
        EduAdvisoryDbContext context,
        IStudentRiskAssessmentService riskService)
    {
        _context = context;
        _riskService = riskService;
    }

    public async Task<RiskAutomationSummaryDto> RunRiskInterventionsAsync()
    {
        var summary = new RiskAutomationSummaryDto();
        var items = new List<RiskAutomationItemDto>();

        try
        {
            // Get all active students with advisors
            var students = await _context.SisStudents
                .Where(s => s.AdvisorId.HasValue)
                .Include(s => s.Advisor)
                .ToListAsync();

            summary.ProcessedStudents = students.Count;

            foreach (var student in students)
            {
                try
                {
                    // Run risk assessment
                    var assessment = _riskService.AssessStudent(student.StudentId);

                    var actionType = DetermineActionType(assessment.RiskLevel);
                    var advisorId = student.AdvisorId.Value;

                    // Check for duplicate intervention (14-day cooldown)
                    // Fetch in memory to avoid timestamp comparison issues with PostgreSQL
                    var cutoffDate = DateTime.UtcNow.AddDays(-COOLDOWN_DAYS);
                    var recentLogs = await _context.RiskInterventionLogs
                        .Where(x =>
                            x.StudentId == student.StudentId &&
                            x.ActionType == actionType)
                        .ToListAsync();
                    
                    var alreadyHandled = recentLogs.Any(x => x.CreatedAt >= cutoffDate);

                    if (alreadyHandled)
                    {
                        summary.SkippedDuplicates++;
                        items.Add(new RiskAutomationItemDto
                        {
                            StudentId = student.StudentId,
                            StudentName = assessment.StudentName,
                            AdvisorId = advisorId,
                            RiskLevel = assessment.RiskLevel,
                            RiskScore = assessment.RiskScore,
                            ActionTaken = actionType,
                            Status = "SKIPPED_DUPLICATE"
                        });
                        continue;
                    }

                    // Execute action based on risk level
                    var (actionTaken, status) = await ExecuteInterventionAsync(
                        student,
                        assessment,
                        actionType);

                    // Log the intervention
                    var log = new RiskInterventionLog
                    {
                        StudentId = student.StudentId,
                        AdvisorId = advisorId,
                        RiskLevel = assessment.RiskLevel,
                        RiskScore = assessment.RiskScore,
                        ActionType = actionType,
                        Status = status,
                        Notes = $"Risk score: {assessment.RiskScore}. Main reasons: {string.Join(", ", assessment.MainReasons.Take(3))}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.RiskInterventionLogs.Add(log);

                    // Update summary counters
                    UpdateSummary(summary, actionType, status);

                    items.Add(new RiskAutomationItemDto
                    {
                        StudentId = student.StudentId,
                        StudentName = assessment.StudentName,
                        AdvisorId = advisorId,
                        RiskLevel = assessment.RiskLevel,
                        RiskScore = assessment.RiskScore,
                        ActionTaken = actionTaken,
                        Status = status
                    });
                }
                catch (Exception ex)
                {
                    summary.FailedActions++;
                    var errorMsg = $"{ex.GetType().Name}: {ex.Message}";
                    Console.WriteLine($"Error processing student {student.StudentId}: {errorMsg}");
                    
                    items.Add(new RiskAutomationItemDto
                    {
                        StudentId = student.StudentId,
                        StudentName = $"{student.FirstName} {student.LastName}",
                        AdvisorId = student.AdvisorId ?? 0,
                        RiskLevel = "UNKNOWN",
                        RiskScore = 0,
                        ActionTaken = "ERROR",
                        Status = "FAILED",
                        ErrorMessage = errorMsg
                    });
                }
            }

            // Save all logs to database
            await _context.SaveChangesAsync();

            summary.Items = items;
            return summary;
        }
        catch (Exception ex)
        {
            throw new Exception($"Risk automation failed: {ex.Message}", ex);
        }
    }

    private string DetermineActionType(string riskLevel)
    {
        return riskLevel switch
        {
            "HIGH" => "HIGH_RISK_MEETING_RECOMMENDATION",
            "MEDIUM" => "MEDIUM_RISK_ADVISOR_NOTIFICATION",
            "LOW" => "LOW_RISK_MESSAGE",
            _ => "UNKNOWN"
        };
    }

    private async Task<(string actionTaken, string status)> ExecuteInterventionAsync(
        SisStudent student,
        StudentRiskAssessmentDto assessment,
        string actionType)
    {
        try
        {
            switch (assessment.RiskLevel)
            {
                case "LOW":
                    await SendLowRiskMessageAsync(student, assessment);
                    return ("LOW_RISK_MESSAGE_SENT", "COMPLETED");

                case "MEDIUM":
                    await CreateMediumRiskNotificationAsync(student, assessment);
                    return ("MEDIUM_RISK_NOTIFICATION_CREATED", "COMPLETED");

                case "HIGH":
                    await CreateHighRiskMeetingRecommendationAsync(student, assessment);
                    return ("HIGH_RISK_MEETING_RECOMMENDATION_CREATED", "COMPLETED");

                default:
                    return ("UNKNOWN_ACTION", "FAILED");
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"{ex.GetType().Name}: {ex.Message}";
            Console.WriteLine($"Error executing intervention for student {student.StudentId}: {errorMsg}");
            throw; // Re-throw to be caught by caller
        }
    }

    private async Task SendLowRiskMessageAsync(SisStudent student, StudentRiskAssessmentDto assessment)
    {
        try
        {
            // Get or create conversation between advisor and student
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.AdvisorId == student.AdvisorId &&
                    c.StudentId == student.StudentId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    AdvisorId = student.AdvisorId.Value,
                    StudentId = student.StudentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            // Get advisor's user ID to send message
            var advisorUser = await _context.Users
                .FirstOrDefaultAsync(u => u.LinkedAdvisorId == student.AdvisorId);

            if (advisorUser == null)
                throw new Exception($"No user found for advisor {student.AdvisorId}");

            // Create automated message
            var messageContent =
                $"Hi {student.FirstName}! Just wanted to let you know that your academic progress is on track. " +
                $"Keep up the great work and don't hesitate to reach out if you need any support along the way.";

            var message = new ChatMessage
            {
                ConversationId = conversation.ConversationId,
                SenderUserId = advisorUser.UserId,
                Content = messageContent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send low risk message for student {student.StudentId}: {ex.Message}", ex);
        }
    }

    private async Task CreateMediumRiskNotificationAsync(SisStudent student, StudentRiskAssessmentDto assessment)
    {
        try
        {
            // For MEDIUM risk, we create a notification note that the advisor can see
            // This is logged in RiskInterventionLog for now; could be expanded to create tasks
            
            // Optionally: Create a system message in the conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.AdvisorId == student.AdvisorId &&
                    c.StudentId == student.StudentId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    AdvisorId = student.AdvisorId.Value,
                    StudentId = student.StudentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            // Get system/automation user or use advisor
            var systemUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "ADMIN" || u.Role == "admin");

            // If no admin user, use the advisor
            var senderUserId = systemUser?.UserId ?? (await _context.Users
                .FirstOrDefaultAsync(u => u.LinkedAdvisorId == student.AdvisorId))?.UserId;

            if (senderUserId == null)
                throw new Exception($"No system or advisor user found for student {student.StudentId} with advisor {student.AdvisorId}");

            var reasonsText = string.Join(", ", assessment.MainReasons.Take(3));
            var messageContent =
                $"Hi! We wanted to flag that {student.FirstName} {student.LastName} is showing some academic concerns. " +
                $"Risk score: {assessment.RiskScore}/100. " +
                $"Key concerns: {reasonsText}. " +
                $"Would you be able to check in with them soon?";

            var message = new ChatMessage
            {
                ConversationId = conversation.ConversationId,
                SenderUserId = senderUserId.Value,
                Content = messageContent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create medium risk notification for student {student.StudentId}: {ex.Message}", ex);
        }
    }

    private async Task CreateHighRiskMeetingRecommendationAsync(SisStudent student, StudentRiskAssessmentDto assessment)
    {
        try
        {
            // For HIGH risk, we create a meeting recommendation note
            var reasonsText = string.Join(", ", assessment.MainReasons.Take(3));

            var meetingReason =
                $"High academic risk detected (Score: {assessment.RiskScore}). " +
                $"Main reasons: {reasonsText}. " +
                $"Advisor follow-up meeting is recommended.";

            // Create a meeting request record (not sent to student, but visible to advisor)
            // For now, store in conversation as a system notification
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.AdvisorId == student.AdvisorId &&
                    c.StudentId == student.StudentId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    AdvisorId = student.AdvisorId.Value,
                    StudentId = student.StudentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            var systemUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "ADMIN" || u.Role == "admin");

            var senderUserId = systemUser?.UserId ?? (await _context.Users
                .FirstOrDefaultAsync(u => u.LinkedAdvisorId == student.AdvisorId))?.UserId;

            if (senderUserId == null)
                throw new Exception($"No system or advisor user found for student {student.StudentId} with advisor {student.AdvisorId}");

            var messageContent =
                $"[HIGH RISK ALERT] Student {student.FirstName} {student.LastName} requires immediate advisor attention. " +
                $"Risk Score: {assessment.RiskScore}. {meetingReason}";

            var message = new ChatMessage
            {
                ConversationId = conversation.ConversationId,
                SenderUserId = senderUserId.Value,
                Content = messageContent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create high risk meeting recommendation for student {student.StudentId}: {ex.Message}", ex);
        }
    }

    private void UpdateSummary(RiskAutomationSummaryDto summary, string actionType, string status)
    {
        if (status != "COMPLETED")
            return;

        switch (actionType)
        {
            case "LOW_RISK_MESSAGE":
                summary.LowRiskMessagesSent++;
                break;
            case "MEDIUM_RISK_ADVISOR_NOTIFICATION":
                summary.MediumRiskAdvisorNotifications++;
                break;
            case "HIGH_RISK_MEETING_RECOMMENDATION":
                summary.HighRiskMeetingRecommendations++;
                break;
        }
    }
}

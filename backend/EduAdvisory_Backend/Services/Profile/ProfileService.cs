using EduAdvisory_Backend.DTOs.Profile;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services.Profile;

public class ProfileService : IProfileService
{
    private readonly EduAdvisoryDbContext _context;

    public ProfileService(EduAdvisoryDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto> GetMyProfileAsync(string keycloakId)
    {
        var user = await _context.Users
            .Include(u => u.LinkedStudent)
                .ThenInclude(s => s.Advisor)
            .Include(u => u.LinkedAdvisor)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user == null)
            throw new Exception("User not found in local database.");

        var role = user.Role?.ToLower();

        if (role == "student")
            return BuildStudentProfile(user);

        if (role == "advisor")
            return await BuildAdvisorProfileAsync(user);

        throw new Exception("Unsupported user role.");
    }

    private ProfileDto BuildStudentProfile(User user)
    {
        var student = user.LinkedStudent;

        if (student == null)
            throw new Exception("Student account is not linked to a student profile.");

        return new ProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role,
            StudentProfile = new StudentProfileDto
            {
                FullName = $"{student.FirstName} {student.LastName}".Trim(),
                StudentId = student.StudentId,
                Email = student.Email,

                ProgramCode = student.ProgramCode,
                CurrentSemester = student.CurrentSemester,
                CurrentGpa = student.CurrentGpa,
                AcademicStatus = student.AcademicStatus,

                AdvisorName = student.Advisor?.Name,
                AdvisorEmail = student.Advisor?.Email,
                AdvisorOffice = student.Advisor?.Office,
                AdvisorOfficeHours = student.Advisor?.OfficeHours
            }
        };
    }

    private async Task<ProfileDto> BuildAdvisorProfileAsync(User user)
    {
        var advisor = user.LinkedAdvisor;

        if (advisor == null)
            throw new Exception("Advisor account is not linked to an advisor profile.");

        var assignedStudents = await _context.SisStudents
            .Where(s => s.AdvisorId == advisor.AdvisorId)
            .ToListAsync();

        var programsSupervised = assignedStudents
            .Where(s => !string.IsNullOrWhiteSpace(s.ProgramCode))
            .Select(s => s.ProgramCode!)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        return new ProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role,
            AdvisorProfile = new AdvisorProfileDto
            {
                Name = advisor.Name,
                AdvisorId = advisor.AdvisorId,
                Email = advisor.Email,

                Office = advisor.Office,
                OfficeHours = advisor.OfficeHours,

                AssignedStudentsCount = assignedStudents.Count,
                ProgramsSupervised = programsSupervised
            }
        };
    }
}
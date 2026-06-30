using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services.Profile;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class ProfileServiceTests
{
    private static EduAdvisoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EduAdvisoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options);

    [Fact]
    public async Task GetMyProfileAsync_WhenUserNotFound_ThrowsException()
    {
        using var context = CreateContext();
        var sut = new ProfileService(context);

        await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyProfileAsync("nonexistent-kc-id"));
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenStudentUser_ReturnsStudentProfile()
    {
        using var context = CreateContext();
        var student = new SisStudent
        {
            StudentId = 1,
            FirstName = "Alice",
            LastName = "Smith",
            ProgramCode = "CS",
            CurrentSemester = 3,
            CurrentGpa = 82m,
            AcademicStatus = "NORMAL",
            Email = "alice@uni.edu"
        };
        var user = new User
        {
            UserId = 10,
            Username = "alice",
            KeycloakId = "kc-alice",
            Role = "student",
            LinkedStudent = student
        };
        context.SisStudents.Add(student);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);
        var profile = await sut.GetMyProfileAsync("kc-alice");

        Assert.NotNull(profile.StudentProfile);
        Assert.Null(profile.AdvisorProfile);
        Assert.Equal("alice", profile.Username);
        Assert.Equal("student", profile.Role);
        Assert.Equal("Alice Smith", profile.StudentProfile!.FullName);
        Assert.Equal("CS", profile.StudentProfile.ProgramCode);
        Assert.Equal(82m, profile.StudentProfile.CurrentGpa);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenAdvisorUser_ReturnsAdvisorProfileWithStudentCount()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 5,
            Name = "Dr. Jones",
            Email = "jones@uni.edu",
            Office = "A-101",
            OfficeHours = "Mon 10-12"
        };
        var user = new User
        {
            UserId = 20,
            Username = "drjones",
            KeycloakId = "kc-jones",
            Role = "advisor",
            LinkedAdvisor = advisor
        };
        var students = new[]
        {
            new SisStudent { StudentId = 1, AdvisorId = 5, ProgramCode = "CS" },
            new SisStudent { StudentId = 2, AdvisorId = 5, ProgramCode = "IT" }
        };
        context.Advisors.Add(advisor);
        context.Users.Add(user);
        context.SisStudents.AddRange(students);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);
        var profile = await sut.GetMyProfileAsync("kc-jones");

        Assert.NotNull(profile.AdvisorProfile);
        Assert.Null(profile.StudentProfile);
        Assert.Equal("Dr. Jones", profile.AdvisorProfile!.Name);
        Assert.Equal(2, profile.AdvisorProfile.AssignedStudentsCount);
        Assert.Contains("CS", profile.AdvisorProfile.ProgramsSupervised);
        Assert.Contains("IT", profile.AdvisorProfile.ProgramsSupervised);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenStudentNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        var user = new User
        {
            UserId = 30,
            Username = "ghost",
            KeycloakId = "kc-ghost",
            Role = "student",
            LinkedStudent = null
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);

        await Assert.ThrowsAsync<Exception>(() => sut.GetMyProfileAsync("kc-ghost"));
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenAdvisorNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        var user = new User
        {
            UserId = 31,
            Username = "ghost2",
            KeycloakId = "kc-ghost2",
            Role = "advisor",
            LinkedAdvisor = null
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);

        await Assert.ThrowsAsync<Exception>(() => sut.GetMyProfileAsync("kc-ghost2"));
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenUnsupportedRole_ThrowsException()
    {
        using var context = CreateContext();
        var user = new User
        {
            UserId = 40,
            Username = "admin",
            KeycloakId = "kc-admin",
            Role = "admin"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);

        await Assert.ThrowsAsync<Exception>(() => sut.GetMyProfileAsync("kc-admin"));
    }

    [Fact]
    public async Task GetMyProfileAsync_AdvisorProgramsSupervised_AreSortedAndDistinct()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 6, Name = "Dr. X", Email = "x@uni.edu", Office = "B-1", OfficeHours = "Tue"
        };
        var user = new User
        {
            UserId = 50, Username = "drx", KeycloakId = "kc-drx",
            Role = "advisor", LinkedAdvisor = advisor
        };
        var students = new[]
        {
            new SisStudent { StudentId = 10, AdvisorId = 6, ProgramCode = "IT" },
            new SisStudent { StudentId = 11, AdvisorId = 6, ProgramCode = "CS" },
            new SisStudent { StudentId = 12, AdvisorId = 6, ProgramCode = "IT" } // duplicate
        };
        context.Advisors.Add(advisor);
        context.Users.Add(user);
        context.SisStudents.AddRange(students);
        await context.SaveChangesAsync();

        var sut = new ProfileService(context);
        var profile = await sut.GetMyProfileAsync("kc-drx");

        var programs = profile.AdvisorProfile!.ProgramsSupervised;
        Assert.Equal(2, programs.Count); // duplicates removed
        Assert.Equal(programs.OrderBy(p => p).ToList(), programs); // sorted
    }
}

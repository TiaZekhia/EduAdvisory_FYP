using System.ComponentModel.DataAnnotations;

namespace EduAdvisory_Backend.DTOs.AI.AdminDocuments;

public class UploadAiDocumentRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string DocumentType { get; set; } = string.Empty;
    // study_guide or course_syllabus

    [Required]
    public string Scope { get; set; } = "course";
    // course, program, general

    public string? CourseCode { get; set; }

    public string? ProgramCode { get; set; }

    public string? AcademicYear { get; set; }

    public string? Semester { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;
}
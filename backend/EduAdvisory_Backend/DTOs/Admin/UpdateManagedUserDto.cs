using System.ComponentModel.DataAnnotations;

namespace EduAdvisory_Backend.DTOs.Admin;

public class UpdateManagedUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
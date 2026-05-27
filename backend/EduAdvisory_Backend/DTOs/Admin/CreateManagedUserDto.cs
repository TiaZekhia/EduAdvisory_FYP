using System.ComponentModel.DataAnnotations;

namespace EduAdvisory_Backend.DTOs.Admin;

public class CreateManagedUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int LinkedEntityId { get; set; }
}
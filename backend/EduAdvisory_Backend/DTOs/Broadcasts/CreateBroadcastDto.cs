using Microsoft.AspNetCore.Http;

namespace EduAdvisory_Backend.DTOs.Broadcasts;

public class CreateBroadcastDto
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public List<int> StudentIds { get; set; } = new();

    public List<IFormFile>? Files { get; set; }
}
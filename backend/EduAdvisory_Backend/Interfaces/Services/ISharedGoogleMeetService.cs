namespace EduAdvisory_Backend.Interfaces.Services
{
    public interface ISharedGoogleMeetService
    {
        Task<SharedGoogleMeetCreateResult> CreateMeetingSpaceAsync();
    }

    public class SharedGoogleMeetCreateResult
    {
        public string SpaceName { get; set; } = "";
        public string MeetingUri { get; set; } = "";
    }
}
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services
{
    public class SharedGoogleMeetService : ISharedGoogleMeetService
    {
        private readonly EduAdvisoryDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SharedGoogleMeetService(
            EduAdvisoryDbContext context,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<SharedGoogleMeetCreateResult> CreateMeetingSpaceAsync()
        {
            var account = await _context.AppGoogleAccounts.FirstOrDefaultAsync();
            if (account == null || string.IsNullOrWhiteSpace(account.RefreshToken))
                throw new Exception("Shared Google account is not connected.");

            var accessToken = await GetValidAccessTokenAsync(account);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://meet.googleapis.com/v2/spaces");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Google Meet API error: {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var spaceName = root.GetProperty("name").GetString() ?? "";
            var meetingUri = root.GetProperty("meetingUri").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(meetingUri))
                throw new Exception("Google Meet meetingUri was not returned.");

            return new SharedGoogleMeetCreateResult
            {
                SpaceName = spaceName,
                MeetingUri = meetingUri
            };
        }

        private async Task<string> GetValidAccessTokenAsync(AppGoogleAccount account)
        {
            if (!string.IsNullOrWhiteSpace(account.AccessToken) &&
                account.TokenExpiryUtc.HasValue &&
                account.TokenExpiryUtc.Value > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return account.AccessToken!;
            }

            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            });

            var refreshed = await flow.RefreshTokenAsync(
                "shared-google-account",
                account.RefreshToken,
                CancellationToken.None);

            if (refreshed == null || string.IsNullOrWhiteSpace(refreshed.AccessToken))
                throw new Exception("Failed to refresh shared Google access token.");

            account.AccessToken = refreshed.AccessToken;
            account.TokenExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresInSeconds ?? 3600);
            account.UpdatedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                account.RefreshToken = refreshed.RefreshToken;

            await _context.SaveChangesAsync();

            return account.AccessToken!;
        }
    }
}
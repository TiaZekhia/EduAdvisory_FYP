using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services
{
    public class SharedGoogleMeetService : ISharedGoogleMeetService
    {
        private readonly EduAdvisoryDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SharedGoogleMeetService> _logger;

        public SharedGoogleMeetService(
            EduAdvisoryDbContext context,
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<SharedGoogleMeetService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SharedGoogleMeetCreateResult> CreateMeetingSpaceAsync()
        {
            var account = await _context.AppGoogleAccounts.FirstOrDefaultAsync();

            if (account == null || string.IsNullOrWhiteSpace(account.RefreshToken))
            {
                throw new SharedGoogleAuthException(
                    "Google account is not connected.",
                    reconnectRequired: true);
            }

            var accessToken = await GetValidAccessTokenAsync(account);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://meet.googleapis.com/v2/spaces");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new SharedGoogleAuthException(
                    "Google account authorization is invalid. Please reconnect Google.",
                    reconnectRequired: true);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Meet API error. Status={StatusCode}, Body={Body}",
                    response.StatusCode, body);

                throw new SharedGoogleAuthException(
                    "Failed to create Google Meet link. Please try again.");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var spaceName = root.GetProperty("name").GetString() ?? "";
            var meetingUri = root.GetProperty("meetingUri").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(meetingUri))
            {
                throw new SharedGoogleAuthException(
                    "Google Meet link was not returned by Google.");
            }

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

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new SharedGoogleAuthException("Google OAuth configuration is missing.");
            }

            try
            {
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
                {
                    throw new SharedGoogleAuthException(
                        "Failed to refresh Google access token.",
                        reconnectRequired: true);
                }

                account.AccessToken = refreshed.AccessToken;
                account.TokenExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresInSeconds ?? 3600);
                account.UpdatedAt = DateTimeOffset.UtcNow;

                if (!string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                    account.RefreshToken = refreshed.RefreshToken;

                await _context.SaveChangesAsync();

                return account.AccessToken!;
            }
            catch (TokenResponseException ex) when (
                ex.Error != null &&
                string.Equals(ex.Error.Error, "invalid_grant", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "Stored Google refresh token is expired or revoked.");

                account.AccessToken = null;
                account.RefreshToken = null;
                account.TokenExpiryUtc = null;
                account.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();

                throw new SharedGoogleAuthException(
                    "Google connection expired or was revoked. Please reconnect Google.",
                    reconnectRequired: true);
            }
            catch (SharedGoogleAuthException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while refreshing shared Google token.");
                throw new SharedGoogleAuthException(
                    "Failed to authenticate with Google. Please try again.");
            }
        }
    }
}
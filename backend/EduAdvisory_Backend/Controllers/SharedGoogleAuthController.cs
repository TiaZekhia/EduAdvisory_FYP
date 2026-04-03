using EduAdvisory_Backend.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Controllers
{
    [ApiController]
    [Route("api/shared-google-auth")]
    public class SharedGoogleAuthController : ControllerBase
    {
        private readonly EduAdvisoryDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SharedGoogleAuthController> _logger;

        private const string SharedGoogleState = "shared-google-account-setup";

        public SharedGoogleAuthController(
            EduAdvisoryDbContext context,
            IConfiguration configuration,
            ILogger<SharedGoogleAuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize(Roles = "ADVISOR")]
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var account = await _context.AppGoogleAccounts.FirstOrDefaultAsync();

            return Ok(new
            {
                connected = account != null && !string.IsNullOrWhiteSpace(account.RefreshToken),
                googleEmail = account?.GoogleEmail
            });
        }

        [Authorize(Roles = "ADVISOR")]
        [HttpGet("connect")]
        public IActionResult Connect()
        {
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(redirectUri))
            {
                return BadRequest(new { message = "Google OAuth configuration is missing." });
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[]
                {
                    "https://www.googleapis.com/auth/meetings.space.created",
                    "openid",
                    "email",
                    "profile"
                }
            });

            var request = flow.CreateAuthorizationCodeRequest(redirectUri);
            request.State = SharedGoogleState;

            var url = request.Build().ToString();

            if (!url.Contains("access_type="))
                url += (url.Contains("?") ? "&" : "?") + "access_type=offline";

            if (!url.Contains("prompt="))
                url += "&prompt=consent";

            if (!url.Contains("include_granted_scopes="))
                url += "&include_granted_scopes=true";

            _logger.LogInformation("Shared Google connect URL generated. State={State}", SharedGoogleState);

            return Ok(new { url });
        }

        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
        {
            _logger.LogInformation(
                "Shared Google callback hit. Code present={HasCode}, State={State}",
                !string.IsNullOrWhiteSpace(code),
                state ?? "(null)");

            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Missing code.");

            if (string.IsNullOrWhiteSpace(state))
                return BadRequest("Missing state.");

            if (!string.Equals(state, SharedGoogleState, StringComparison.Ordinal))
            {
                return BadRequest($"Invalid state. Expected '{SharedGoogleState}', got '{state}'.");
            }

            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"];
            var frontendSuccessUrl =
                _configuration["GoogleOAuth:FrontendSuccessUrl"]
                ?? "http://localhost:3000/advisor/meetings?google_connected=1";

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(redirectUri))
            {
                return BadRequest("Google OAuth configuration is missing.");
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[]
                {
                    "https://www.googleapis.com/auth/meetings.space.created",
                    "openid",
                    "email",
                    "profile"
                }
            });

            var token = await flow.ExchangeCodeForTokenAsync(
                "shared-google-account",
                code,
                redirectUri,
                CancellationToken.None);

            var googleEmail = await TryGetGoogleEmailAsync(token.AccessToken);

            var account = await _context.AppGoogleAccounts.FirstOrDefaultAsync();
            if (account == null)
            {
                account = new AppGoogleAccount
                {
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _context.AppGoogleAccounts.Add(account);
            }

            account.GoogleEmail = googleEmail;
            account.AccessToken = token.AccessToken;
            account.RefreshToken = token.RefreshToken ?? account.RefreshToken;
            account.TokenExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3600);
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Redirect(frontendSuccessUrl);
        }

        [Authorize(Roles = "ADVISOR")]
        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            var account = await _context.AppGoogleAccounts.FirstOrDefaultAsync();
            if (account == null)
            {
                return Ok(new { message = "Google account already disconnected." });
            }

            account.GoogleEmail = null;
            account.AccessToken = null;
            account.RefreshToken = null;
            account.TokenExpiryUtc = null;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Google account disconnected successfully." });
        }

        private async Task<string?> TryGetGoogleEmailAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("email", out var emailProp))
                    return emailProp.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
using backend.Models;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IBlackboardService _blackboardService;
        private readonly IConfiguration _configuration;
        
        // Temporary storage for session cookies during OAuth flow (state -> cookie)
        // In production, use distributed cache (Redis) instead of in-memory
        private static readonly Dictionary<string, (string cookie, DateTime expiry)> _sessionCache = new();

        public GoogleAuthController(IUserRepository userRepository, IBlackboardService blackboardService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _blackboardService = blackboardService;
            _configuration = configuration;
        }

        /// <summary>
        /// Returns the Google OAuth2 authorization URL for the frontend to redirect the user and obtain consent.
        /// Uses the minimal calendar events scope and requests offline access (refresh token).
        /// Stores the Blackboard session temporarily to retrieve it in the callback.
        /// </summary>
        [HttpGet("connect")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Connect([FromHeader(Name = "X-Session-Cookie")] string? sessionCookieHeader)
        {
            var clientId = _configuration["Google:ClientId"] ?? Environment.GetEnvironmentVariable("Google__ClientId");
            var redirect = _configuration["Google:RedirectUri"] ?? Environment.GetEnvironmentVariable("Google__RedirectUri");
            var scope = "https://www.googleapis.com/auth/calendar.events openid email profile";

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirect))
            {
                return Problem("Missing Google OAuth configuration. Ensure Google__ClientId and Google__RedirectUri are set.", statusCode: StatusCodes.Status500InternalServerError);
            }

            // Get session cookie from header or Cookie
            var cookie = sessionCookieHeader;
            if (string.IsNullOrEmpty(cookie) && Request.Headers.TryGetValue("Cookie", out var c)) 
                cookie = c.ToString();
            if (string.IsNullOrEmpty(cookie))
                return BadRequest(new { message = "Session cookie is required in 'X-Session-Cookie' or 'Cookie' header." });

            // Generate a state token to identify this session
            var stateToken = Guid.NewGuid().ToString();
            
            // Store session cookie in cache (expires in 10 minutes)
            _sessionCache[stateToken] = (cookie, DateTime.UtcNow.AddMinutes(10));

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirect)}&scope={Uri.EscapeDataString(scope)}&access_type=offline&prompt=consent&state={Uri.EscapeDataString(stateToken)}";

            return Ok(new { url, stateToken });
        }

        /// <summary>
        /// OAuth2 callback endpoint that exchanges the authorization <paramref name="code"/> for tokens
        /// and links the Google account (stores refresh token) to the current authenticated user (identified via state token).
        /// </summary>
        [HttpGet("callback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest(new { message = "Missing code query parameter." });
            if (string.IsNullOrEmpty(state)) return BadRequest(new { message = "Missing state query parameter." });

            // Retrieve session cookie from cache using state token
            if (!_sessionCache.TryGetValue(state, out var cachedSession))
            {
                return BadRequest(new { message = "Invalid or expired state token. Please start the OAuth flow again by calling /connect." });
            }

            // Check if cache entry expired
            if (DateTime.UtcNow > cachedSession.expiry)
            {
                _sessionCache.Remove(state);
                return BadRequest(new { message = "Session expired. Please start the OAuth flow again by calling /connect." });
            }

            var cookie = cachedSession.cookie;
            _sessionCache.Remove(state); // Clean up cache entry

            var clientId = _configuration["Google:ClientId"] ?? Environment.GetEnvironmentVariable("Google__ClientId");
            var clientSecret = _configuration["Google:ClientSecret"] ?? Environment.GetEnvironmentVariable("Google__ClientSecret");
            var redirect = _configuration["Google:RedirectUri"] ?? Environment.GetEnvironmentVariable("Google__RedirectUri");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirect))
            {
                return Problem("Missing Google OAuth configuration. Ensure Google__ClientId, Google__ClientSecret and Google__RedirectUri are set.", statusCode: StatusCodes.Status500InternalServerError);
            }

            using var http = new HttpClient();
            var body = new System.Collections.Generic.Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirect },
                { "grant_type", "authorization_code" }
            };

            var tokenResp = await http.PostAsync("https://oauth2.googleapis.com/token", new System.Net.Http.FormUrlEncodedContent(body));
            if (!tokenResp.IsSuccessStatusCode)
            {
                var content = await tokenResp.Content.ReadAsStringAsync();
                return BadRequest(new { message = "Failed to exchange code for tokens.", details = content });
            }

            var tokenJson = await tokenResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(tokenJson);
            var root = doc.RootElement;
            var accessToken = root.TryGetProperty("access_token", out var atProp) ? atProp.GetString() : null;
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest(new { message = "Failed to get access token from Google.", details = tokenJson });
            }
            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = root.TryGetProperty("expires_in", out var ie) ? ie.GetInt32() : 3600;

            // Decode ID token to get user info (sub = user ID, email)
            var idToken = root.TryGetProperty("id_token", out var itProp) ? itProp.GetString() : null;
            string? googleId = null;
            string? googleEmail = null;

            if (!string.IsNullOrEmpty(idToken))
            {
                try
                {
                    // ID token format: header.payload.signature
                    var parts = idToken.Split('.');
                    if (parts.Length == 3)
                    {
                        var payload = parts[1];
                        // Add padding if needed
                        var padding = payload.Length % 4;
                        if (padding > 0) payload += new string('=', 4 - padding);
                        var decodedBytes = Convert.FromBase64String(payload);
                        var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
                        using var idTokenDoc = JsonDocument.Parse(decodedJson);
                        var idTokenRoot = idTokenDoc.RootElement;
                        googleId = idTokenRoot.TryGetProperty("sub", out var subProp) ? subProp.GetString() : null;
                        googleEmail = idTokenRoot.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Failed to decode ID token.", details = ex.Message });
                }
            }

            // Fallback: if no ID token, try userinfo endpoint
            if (string.IsNullOrEmpty(googleEmail) || string.IsNullOrEmpty(googleId))
            {
                var infoReq = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v1/userinfo");
                infoReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var infoResp = await http.SendAsync(infoReq);
                if (infoResp.IsSuccessStatusCode)
                {
                    var infoJson = await infoResp.Content.ReadAsStringAsync();
                    using var infoDoc = JsonDocument.Parse(infoJson);
                    var infoRoot = infoDoc.RootElement;
                    if (string.IsNullOrEmpty(googleId)) googleId = infoRoot.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                    if (string.IsNullOrEmpty(googleEmail)) googleEmail = infoRoot.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                }
            }

            // Get Blackboard user from cached session cookie - REQUIRED to link Google account
            var bbResult = await _blackboardService.GetUserDataAsync(cookie);
            if (bbResult?.IsSuccess != true)
            {
                return BadRequest(new { message = "Failed to validate Blackboard session.", details = bbResult?.Message });
            }

            var bbEmail = bbResult.UserData?.Email;
            if (string.IsNullOrEmpty(bbEmail))
            {
                return BadRequest(new { message = "Unable to retrieve your email from Blackboard session." });
            }

            // Find or create user by Blackboard email (primary identity)
            backend.Models.User? user = await _userRepository.GetByEmailAsync(bbEmail);
            if (user == null)
            {
                // Create user linked to Blackboard account
                await _userRepository.UpsertByUsernameAsync(bbEmail, bbEmail);
                user = await _userRepository.GetByUsernameAsync(bbEmail);
            }

            if (user == null)
            {
                return BadRequest(new { message = "Failed to create or find user for linking Google account." });
            }

            var account = new GoogleAccount
            {
                GoogleId = googleId,
                Email = googleEmail,
                RefreshToken = refreshToken,
                AccessToken = accessToken,
                AccessTokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn),
                Scopes = new[] { "https://www.googleapis.com/auth/calendar.events" }
            };

            await _userRepository.UpsertGoogleAccountAsync(user.Username, account);

            return Ok(new { message = "Google account linked successfully", userEmail = bbEmail, googleEmail = googleEmail });
        }
    }
}

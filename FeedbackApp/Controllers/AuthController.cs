using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeedbackApp.Authentication;
using FeedbackApp.Filters;

namespace FeedbackApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ServiceFilter(typeof(LoggingActionFilter))]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    // In-memory user store for demo — replace with a database in production
    private static readonly Dictionary<string, (string Password, string Role)> Users = new()
    {
        { "admin", ("admin123", "Admin") },
        { "user", ("user123", "User") }
    };

    public AuthController(IJwtTokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateModel]
    public ActionResult<TokenResponse> Login([FromBody] LoginRequest request)
    {
        if (!Users.TryGetValue(request.Username.ToLower(), out var userInfo))
        {
            _logger.LogWarning("Login failed: User {Username} not found", request.Username);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        if (userInfo.Password != request.Password)
        {
            _logger.LogWarning("Login failed: Invalid password for {Username}", request.Username);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var userId = Guid.NewGuid().ToString();
        var tokens = _tokenService.GenerateTokens(userId, request.Username, userInfo.Role);

        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public ActionResult<TokenResponse> Refresh([FromBody] RefreshRequest request)
    {
        var principal = _tokenService.ValidateToken(request.AccessToken);

        if (principal == null)
        {
            // In production, validate the refresh token against stored tokens before issuing a new one
            return Unauthorized(new { error = "Invalid token" });
        }

        var userId = principal.FindFirst("uid")?.Value ?? "";
        var username = principal.Identity?.Name ?? "";
        var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";

        var tokens = _tokenService.GenerateTokens(userId, username, role);

        return Ok(tokens);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var userId = User.FindFirst("uid")?.Value;
        var username = User.Identity?.Name;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new UserInfo
        {
            UserId = userId ?? "",
            Username = username ?? "",
            Role = role ?? ""
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public ActionResult<object> AdminOnly()
    {
        return Ok(new
        {
            message = "Welcome, Admin!",
            timestamp = DateTime.UtcNow,
            secretData = "This is admin-only content"
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using ParkShare.Application.DTOs.Auth;
using ParkShare.Application.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParkShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle(string? returnUrl = "/")
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleLoginCallback)) };
        // If you need to pass the returnUrl through the external login process:
        // properties.Items["returnUrl"] = returnUrl;
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")] // Matches options.CallbackPath or default if not set
    public async Task<IActionResult> GoogleLoginCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
        {
            return BadRequest("Google authentication failed.");
        }

        var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
        var givenNameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.GivenName);
        var surnameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Surname);
        var googleIdClaim = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier); // Google's unique ID

        if (emailClaim == null || googleIdClaim == null)
        {
            return BadRequest("Required claims not provided by Google.");
        }

        var googleUserInfo = new GoogleUserInfoDto
        {
            Email = emailClaim.Value,
            FirstName = givenNameClaim?.Value ?? string.Empty,
            LastName = surnameClaim?.Value ?? string.Empty,
            GoogleId = googleIdClaim.Value
        };

        var userDto = await _authService.GetOrCreateUserAsync(googleUserInfo);
        var token = _authService.GenerateJwtToken(userDto);

        // For a web app, you might redirect with the token or set a cookie.
        // For an API consumed by a SPA or mobile app, returning the token is common.
        return Ok(new { Token = token, User = userDto });
    }

    [HttpGet("userinfo")]
    [Microsoft.AspNetCore.Authorization.Authorize] // Use the specific Authorize attribute
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"); // "sub" is often used for subject ID in JWT
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (userId == null)
        {
            return Unauthorized("User ID not found in token.");
        }
        // In a real scenario, you might fetch more user details from your DB using userId
        // For now, just return what's in the token.
        return Ok(new { UserId = userId, Email = email, Claims = User.Claims.Select(c => new {c.Type, c.Value}) });
    }
}

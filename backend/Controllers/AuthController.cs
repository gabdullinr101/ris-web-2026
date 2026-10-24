using System.Security.Claims;
using backend.Auth;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, IPasswordHasher<User> passwordHasher, ILogger<AuthController> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    // POST /api/auth/login - проверить логин и пароль, поставить cookie входа.
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Login == request.Login.Trim());

        // Пароль сравнивается с hash библиотекой ASP.NET Identity, открытого пароля в базе нет
        var passwordOk = user?.PasswordHash != null &&
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                != PasswordVerificationResult.Failed;

        if (user == null || !passwordOk)
        {
            // Не уточняем, что именно неверно - логин или пароль. Пароль в лог не пишем.
            _logger.LogWarning("Login failed; login={Login}", request.Login);
            return Problem(statusCode: StatusCodes.Status401Unauthorized,
                title: "Неверный логин или пароль");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(UserResponse.From(user));
    }

    // POST /api/auth/logout - удалить cookie входа.
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Вы вышли из системы" });
    }

    // GET /api/auth/me - кто вошёл. Без входа - 401.
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserResponse> Me()
    {
        var current = CurrentUser.From(User);
        var user = _db.Users.FirstOrDefault(u => u.Id == current.Id);
        if (user == null)
        {
            return Unauthorized();
        }
        return Ok(UserResponse.From(user));
    }
}

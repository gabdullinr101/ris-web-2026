using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/users/trainers - тренеры для выбора при назначении. Только тем, кто назначает.
    [HttpGet("trainers")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager)]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IReadOnlyList<UserResponse>> Trainers()
    {
        var trainers = _db.Users
            .Where(u => u.Role == UserRoles.Trainer)
            .OrderBy(u => u.FullName)
            .AsEnumerable()
            .Select(UserResponse.From)
            .ToList();
        return Ok(trainers);
    }
}

using System.Security.Claims;

namespace backend.Auth;

// Кто сейчас выполняет запрос. Берётся из cookie входа, а не из заголовка или тела запроса.
public record CurrentUser(int Id, string Role)
{
    public static CurrentUser From(ClaimsPrincipal principal)
    {
        var id = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = principal.FindFirstValue(ClaimTypes.Role)!;
        return new CurrentUser(id, role);
    }
}

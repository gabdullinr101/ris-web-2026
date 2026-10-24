using backend.Models;

namespace backend.Dtos;

// Что сервер отдаёт о пользователе. Hash пароля наружу не уходит.
public class UserResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";

    public static UserResponse From(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
        };
    }
}

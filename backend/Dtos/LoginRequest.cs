using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "Укажите логин")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Укажите пароль")]
    public string Password { get; set; } = "";
}

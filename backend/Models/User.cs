namespace backend.Models;

// Пользователь системы: администратор ресепшн, тренер или управляющий клуба.
public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = "";

    public string FullName { get; set; } = "";

    // Admin - администратор ресепшн, Trainer - тренер, Manager - управляющий клуба
    public string Role { get; set; } = "";
}

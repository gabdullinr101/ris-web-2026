using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

// Что клиент присылает при создании записи. Только поля, которые заполняет человек.
// id, number, status и тренера здесь нет: их задаёт сервер.
public class CreateBookingRequest
{
    [Required(ErrorMessage = "Укажите вид тренировки")]
    [StringLength(80, MinimumLength = 5, ErrorMessage = "Длина от 5 до 80 символов")]
    public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Номер зала - целое число больше 0")]
    public int HallId { get; set; }

    [Required(ErrorMessage = "Укажите описание")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Длина от 10 до 500 символов")]
    public string Description { get; set; } = "";
}

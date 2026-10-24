namespace backend.Models;

// Запись на тренировку. Поля как в docs/data-model.md и docs/api-contract.md.
// В ЛР7 это только заготовка: создавать записи сервер научится в ЛР8.
public class Booking
{
    public int Id { get; set; }

    // Человеческий номер, например БР-0017. Не замена Id.
    public string Number { get; set; } = "";

    public string Title { get; set; } = "";

    public int HallId { get; set; }

    public string Description { get; set; } = "";

    // Только New, InProgress, Closed, Cancelled
    public string Status { get; set; } = "New";

    public int CreatedByUserId { get; set; }

    // У новой записи тренера может не быть
    public int? AssigneeUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}

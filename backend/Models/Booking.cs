namespace backend.Models;

// Запись на тренировку. Поля как в docs/data-model.md, столбцы - как в db/schema.sql.
public class Booking
{
    public int Id { get; set; }

    // Человеческий номер, например БР-0017. Не замена Id.
    public string Number { get; set; } = "";

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public int HallId { get; set; }
    public Hall? Hall { get; set; }

    // Только New, InProgress, Closed, Cancelled
    public string Status { get; set; } = BookingStatuses.New;

    public DateTime CreatedAt { get; set; }

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // У новой записи тренера может не быть
    public int? AssigneeUserId { get; set; }
    public User? AssigneeUser { get; set; }
}

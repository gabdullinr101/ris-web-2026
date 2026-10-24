using backend.Models;

namespace backend.Dtos;

// Что сервер отдаёт клиенту о записи.
public class BookingResponse
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public int HallId { get; set; }
    // Название зала - для экрана, чтобы клиент не показывал голый номер (ЛР12)
    public string? HallName { get; set; }
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public int CreatedByUserId { get; set; }
    public int? AssigneeUserId { get; set; }
    // Имя назначенного тренера - для экрана (ЛР12)
    public string? AssigneeFullName { get; set; }
    public DateTime CreatedAt { get; set; }

    public static BookingResponse From(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            Number = booking.Number,
            Title = booking.Title,
            HallId = booking.HallId,
            HallName = booking.Hall?.Name,
            Description = booking.Description,
            Status = booking.Status,
            CreatedByUserId = booking.CreatedByUserId,
            AssigneeUserId = booking.AssigneeUserId,
            AssigneeFullName = booking.AssigneeUser?.FullName,
            CreatedAt = booking.CreatedAt,
        };
    }
}

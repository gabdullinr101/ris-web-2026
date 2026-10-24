using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

// PATCH /api/bookings/{id}/assignee - только назначение тренера, статус не трогает.
public class AssignBookingRequest
{
    [Required(ErrorMessage = "Укажите тренера")]
    [Range(1, int.MaxValue, ErrorMessage = "Номер тренера - целое число больше 0")]
    public int? AssigneeUserId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

// PATCH /api/bookings/{id}/status - только смена статуса, тренера не трогает.
public class ChangeBookingStatusRequest
{
    [Required(ErrorMessage = "Укажите статус")]
    [RegularExpression("^(New|InProgress|Closed|Cancelled)$",
        ErrorMessage = "Статус: New, InProgress, Closed или Cancelled")]
    public string Status { get; set; } = "";
}

using backend.Models;

namespace backend.Services;

// Заготовка: записи хранятся в списке в памяти процесса.
// После остановки сервера (Ctrl+C) и нового dotnet run список снова пустой.
// Создание записей (POST /api/bookings) и проверка полей появятся в ЛР8.
public class BookingService : IBookingService
{
    private readonly List<Booking> _bookings = new();

    public IReadOnlyList<Booking> GetAll()
    {
        return _bookings;
    }
}

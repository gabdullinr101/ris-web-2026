using backend.Models;

namespace backend.Services;

// Что умеет сервис записей. Контроллер знает только этот интерфейс,
// поэтому позже реализацию со списком в памяти можно заменить на работу с PostgreSQL.
public interface IBookingService
{
    IReadOnlyList<Booking> GetAll();
}

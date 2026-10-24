using backend.Dtos;

namespace backend.Services;

// Что умеет сервис записей. Контроллер знает только этот интерфейс,
// поэтому реализацию со списком в памяти можно будет заменить на PostgreSQL (ЛР10), не трогая контроллер.
public interface IBookingService
{
    IReadOnlyList<BookingResponse> GetAll();

    ServiceResult<BookingResponse> GetById(int id);

    ServiceResult<BookingResponse> Create(CreateBookingRequest request, int currentUserId);

    ServiceResult<BookingResponse> Assign(int id, int assigneeUserId);

    ServiceResult<BookingResponse> ChangeStatus(int id, string status, int currentUserId);
}

using backend.Auth;
using backend.Dtos;

namespace backend.Services;

// Что умеет сервис записей. Контроллер знает только этот интерфейс.
// Кто выполняет действие - CurrentUser из cookie входа (ЛР11). Права проверяются здесь, на сервере.
public interface IBookingService
{
    IReadOnlyList<BookingResponse> GetAll(CurrentUser user);

    ServiceResult<BookingResponse> GetById(int id, CurrentUser user);

    ServiceResult<BookingResponse> Create(CreateBookingRequest request, CurrentUser user);

    ServiceResult<BookingResponse> Assign(int id, int assigneeUserId, CurrentUser user);

    ServiceResult<BookingResponse> ChangeStatus(int id, string status, CurrentUser user);
}

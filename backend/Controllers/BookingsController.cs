using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

// Заготовка контроллера записей на тренировку.
// Контроллер не создаёт сервис через new: готовый объект приходит в конструктор (DI, см. Program.cs).
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    // GET /api/bookings - список записей. Пока сервер ничего не создаёт, ответ 200 и [].
    [HttpGet]
    public ActionResult<IReadOnlyList<Booking>> GetAll()
    {
        return Ok(_service.GetAll());
    }
}

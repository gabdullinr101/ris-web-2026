using backend.Auth;
using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

// Записи на тренировку. Контроллер тонкий: принял HTTP, вызвал сервис, вернул код.
// Без входа любой адрес отвечает 401 ([Authorize]). Права по ролям проверяет сервис - 403.
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService service, ILogger<BookingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // Кто выполняет запрос - из cookie входа
    private CurrentUser Me => CurrentUser.From(User);

    // GET /api/bookings - список. Тренер видит только свои. Пустой список - 200 и [].
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IReadOnlyList<BookingResponse>> GetAll()
    {
        return Ok(_service.GetAll(Me));
    }

    // GET /api/bookings/{id} - одна запись, 403 на чужую для тренера, 404 если нет.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<BookingResponse> GetById(int id)
    {
        return ToActionResult(_service.GetById(id, Me));
    }

    // POST /api/bookings - создать. Только администратор.
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<BookingResponse> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var result = _service.Create(request, Me);
            if (result.Kind == ResultKind.Ok)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
            }
            return ToActionResult(result);
        }
        catch (Exception ex)
        {
            // Разработчику - одна строка в логе: операция, зал, тип ошибки. Без тела запроса целиком.
            _logger.LogError("CreateBooking failed; hallId={HallId}; error={Error}",
                request.HallId, ex.GetType().Name);
            // Клиенту - безопасный ответ без stack trace и путей к файлам
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Не удалось обработать запрос");
        }
    }

    // PATCH /api/bookings/{id}/assignee - назначить тренера. Статус не меняется.
    [HttpPatch("{id:int}/assignee")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<BookingResponse> Assign(int id, [FromBody] AssignBookingRequest request)
    {
        return ToActionResult(_service.Assign(id, request.AssigneeUserId!.Value, Me));
    }

    // PATCH /api/bookings/{id}/status - сменить статус. Тренера не меняет.
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<BookingResponse> ChangeStatus(int id, [FromBody] ChangeBookingStatusRequest request)
    {
        return ToActionResult(_service.ChangeStatus(id, request.Status, Me));
    }

    private ActionResult InvalidFields(Dictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError(field, message);
            }
        }
        return ValidationProblem(ModelState);
    }

    // Перевод итога сервиса в HTTP-код. Все ошибки - в одном виде ProblemDetails.
    private ActionResult ToActionResult(ServiceResult<BookingResponse> result)
    {
        return result.Kind switch
        {
            ResultKind.Ok => Ok(result.Value),
            ResultKind.Invalid => InvalidFields(result.Errors),
            ResultKind.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Недостаточно прав", detail: result.Message),
            ResultKind.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Не найдено", detail: result.Message),
            ResultKind.Conflict => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Действие противоречит правилам процесса", detail: result.Message),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Не удалось обработать запрос"),
        };
    }
}

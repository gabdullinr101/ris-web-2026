using backend.Dtos;
using backend.Models;

namespace backend.Services;

// Записи хранятся в списке в памяти процесса.
// После остановки сервера (Ctrl+C) и нового dotnet run список снова пустой - для ЛР8 это нормально.
// Правила процесса (переходы статусов, кто двигает запись) живут здесь, а не в контроллере.
public class BookingService : IBookingService
{
    private readonly List<Booking> _bookings = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    // Справочники пока тоже в памяти. Те же строки - в db/seed.sql.
    private static readonly List<Hall> Halls = new()
    {
        new Hall { Id = 1, Name = "Тренажёрный зал" },
        new Hall { Id = 2, Name = "Зал единоборств" },
        new Hall { Id = 3, Name = "Зал групповых программ" },
    };

    private static readonly List<User> Users = new()
    {
        new User { Id = 1, Login = "admin", FullName = "Анна Ковалёва", Role = UserRoles.Admin },
        new User { Id = 2, Login = "trainer", FullName = "Илья Соколов", Role = UserRoles.Trainer },
        new User { Id = 3, Login = "trainer2", FullName = "Марат Хайруллин", Role = UserRoles.Trainer },
        new User { Id = 4, Login = "manager", FullName = "Ольга Петрова", Role = UserRoles.Manager },
    };

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public BookingService(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public IReadOnlyList<BookingResponse> GetAll()
    {
        lock (_lock)
        {
            return _bookings.Select(BookingResponse.From).ToList();
        }
    }

    public ServiceResult<BookingResponse> GetById(int id)
    {
        lock (_lock)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
            }
            return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
        }
    }

    public ServiceResult<BookingResponse> Create(CreateBookingRequest request, int currentUserId)
    {
        // Длину проверяет ещё DTO, но строку из пробелов он пропустит - поэтому проверяем после Trim
        var title = request.Title.Trim();
        var description = request.Description.Trim();

        if (title.Length < 5 || title.Length > 80)
        {
            return ServiceResult<BookingResponse>.Invalid("title", "Длина от 5 до 80 символов");
        }
        if (description.Length < 10 || description.Length > 500)
        {
            return ServiceResult<BookingResponse>.Invalid("description", "Длина от 10 до 500 символов");
        }
        if (!Halls.Any(h => h.Id == request.HallId))
        {
            return ServiceResult<BookingResponse>.Invalid("hallId", "Зал с таким номером не найден");
        }
        if (!Users.Any(u => u.Id == currentUserId))
        {
            return ServiceResult<BookingResponse>.Invalid("X-User-Id", "Пользователь с таким номером не найден");
        }

        // Учебный сбой для проверки лога. Включается только в Development:
        // переменная окружения Debug__FailCreate=true (см. README). В обычном запуске выключен.
        if (_environment.IsDevelopment() && _configuration.GetValue<bool>("Debug:FailCreate"))
        {
            throw new InvalidOperationException("Учебный сбой при создании записи");
        }

        lock (_lock)
        {
            var booking = new Booking
            {
                Id = _nextId,
                Number = "БР-" + _nextId.ToString("D4"),
                Title = title,
                HallId = request.HallId,
                Description = description,
                Status = BookingStatuses.New,
                CreatedByUserId = currentUserId,
                AssigneeUserId = null,
                CreatedAt = DateTime.UtcNow,
            };
            _nextId++;
            _bookings.Add(booking);
            return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
        }
    }

    public ServiceResult<BookingResponse> Assign(int id, int assigneeUserId)
    {
        lock (_lock)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
            }

            var trainer = Users.FirstOrDefault(u => u.Id == assigneeUserId);
            if (trainer == null || trainer.Role != UserRoles.Trainer)
            {
                return ServiceResult<BookingResponse>.Invalid("assigneeUserId", "Тренер с таким номером не найден");
            }

            // Проведённую или отменённую запись переназначать нельзя - это правило процесса, поэтому 409
            if (booking.Status == BookingStatuses.Closed || booking.Status == BookingStatuses.Cancelled)
            {
                return ServiceResult<BookingResponse>.Conflict(
                    $"Нельзя назначить тренера: запись в статусе {booking.Status}");
            }

            // Назначение не меняет статус
            booking.AssigneeUserId = assigneeUserId;
            return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
        }
    }

    public ServiceResult<BookingResponse> ChangeStatus(int id, string status, int currentUserId)
    {
        lock (_lock)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
            }

            if (!BookingStatuses.CanMove(booking.Status, status))
            {
                return ServiceResult<BookingResponse>.Conflict(
                    $"Переход {booking.Status} → {status} недопустим");
            }

            if (booking.AssigneeUserId == null)
            {
                return ServiceResult<BookingResponse>.Conflict(
                    "Нельзя менять статус: тренер ещё не назначен");
            }

            // Статус двигает только назначенный тренер. Настоящий вход и 403 будут в ЛР11,
            // пока чужой переход - тоже конфликт с правилом процесса.
            if (booking.AssigneeUserId != currentUserId)
            {
                return ServiceResult<BookingResponse>.Conflict(
                    "Менять статус может только тренер, назначенный на эту запись");
            }

            booking.Status = status;
            return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
        }
    }
}

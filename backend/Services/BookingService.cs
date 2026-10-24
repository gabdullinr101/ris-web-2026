using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Services;

// Записи хранятся в PostgreSQL через EF Core (ЛР10). После перезапуска сервера они на месте.
// Правила процесса (переходы статусов, кто двигает запись) живут здесь, а не в контроллере.
public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public BookingService(AppDbContext db, IConfiguration configuration, IHostEnvironment environment)
    {
        _db = db;
        _configuration = configuration;
        _environment = environment;
    }

    public IReadOnlyList<BookingResponse> GetAll()
    {
        // SQL: SELECT ... FROM bookings ORDER BY id
        return _db.Bookings
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .AsEnumerable()
            .Select(BookingResponse.From)
            .ToList();
    }

    public ServiceResult<BookingResponse> GetById(int id)
    {
        // SQL: SELECT ... FROM bookings WHERE id = @id LIMIT 1
        var booking = _db.Bookings.AsNoTracking().FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
        }
        return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
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
        if (!_db.Halls.Any(h => h.Id == request.HallId))
        {
            return ServiceResult<BookingResponse>.Invalid("hallId", "Зал с таким номером не найден");
        }
        if (!_db.Users.Any(u => u.Id == currentUserId))
        {
            return ServiceResult<BookingResponse>.Invalid("X-User-Id", "Пользователь с таким номером не найден");
        }

        // Учебный сбой для проверки лога (ЛР8). Только в Development и только с Debug__FailCreate=true.
        if (_environment.IsDevelopment() && _configuration.GetValue<bool>("Debug:FailCreate"))
        {
            throw new InvalidOperationException("Учебный сбой при создании записи");
        }

        // Номер БР-0001 строится из id, а id выдаёт база при вставке.
        // Поэтому в одной транзакции: вставили с временным номером → узнали id → записали настоящий номер.
        using var transaction = _db.Database.BeginTransaction();

        var booking = new Booking
        {
            Number = "tmp-" + Guid.NewGuid().ToString("N"),
            Title = title,
            HallId = request.HallId,
            Description = description,
            Status = BookingStatuses.New,
            CreatedByUserId = currentUserId,
            AssigneeUserId = null,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Bookings.Add(booking);

        try
        {
            _db.SaveChanges();
            booking.Number = "БР-" + booking.Id.ToString("D4");
            _db.SaveChanges();
            transaction.Commit();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Человеческий номер уже занят - конфликт, а не 500
            return ServiceResult<BookingResponse>.Conflict("Запись с таким номером уже есть, повторите создание");
        }

        return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
    }

    public ServiceResult<BookingResponse> Assign(int id, int assigneeUserId)
    {
        var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
        }

        var trainer = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == assigneeUserId);
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

        // Назначение не меняет статус. SQL: UPDATE bookings SET assignee_user_id = @trainer WHERE id = @id
        booking.AssigneeUserId = assigneeUserId;
        _db.SaveChanges();
        return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
    }

    public ServiceResult<BookingResponse> ChangeStatus(int id, string status, int currentUserId)
    {
        var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
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

        // Статус двигает только назначенный тренер. Настоящий вход и 403 будут в ЛР11.
        if (booking.AssigneeUserId != currentUserId)
        {
            return ServiceResult<BookingResponse>.Conflict(
                "Менять статус может только тренер, назначенный на эту запись");
        }

        // SQL: UPDATE bookings SET status = @status WHERE id = @id
        booking.Status = status;
        _db.SaveChanges();
        return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}

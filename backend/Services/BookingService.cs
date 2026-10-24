using backend.Auth;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Services;

// Записи хранятся в PostgreSQL через EF Core. Правила процесса и права - здесь, на сервере.
// В ответе кроме id зала и тренера есть их имена (HallName, AssigneeFullName) - для экрана.
// Матрица прав - docs/role-matrix.md:
//   создаёт - администратор; назначает - администратор (запись New) и управляющий;
//   статус меняет только назначенный тренер своей записи; тренер видит только свои записи.
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

    public IReadOnlyList<BookingResponse> GetAll(CurrentUser user)
    {
        // Include подтягивает зал и тренера: SQL ... LEFT JOIN halls ... LEFT JOIN users ...
        IQueryable<Booking> query = _db.Bookings.AsNoTracking()
            .Include(b => b.Hall)
            .Include(b => b.AssigneeUser);

        // Тренер видит только записи, назначенные ему (ФТ-5).
        // SQL: ... WHERE assignee_user_id = @userId
        if (user.Role == UserRoles.Trainer)
        {
            query = query.Where(b => b.AssigneeUserId == user.Id);
        }

        return query
            .OrderBy(b => b.Id)
            .AsEnumerable()
            .Select(BookingResponse.From)
            .ToList();
    }

    public ServiceResult<BookingResponse> GetById(int id, CurrentUser user)
    {
        // SQL: SELECT ... FROM bookings WHERE id = @id LIMIT 1
        var booking = _db.Bookings.AsNoTracking()
            .Include(b => b.Hall)
            .Include(b => b.AssigneeUser)
            .FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
        }
        if (user.Role == UserRoles.Trainer && booking.AssigneeUserId != user.Id)
        {
            return ServiceResult<BookingResponse>.Forbidden("Это запись другого тренера");
        }
        return ServiceResult<BookingResponse>.Ok(BookingResponse.From(booking));
    }

    public ServiceResult<BookingResponse> Create(CreateBookingRequest request, CurrentUser user)
    {
        // Создаёт только администратор ресепшн
        if (user.Role != UserRoles.Admin)
        {
            return ServiceResult<BookingResponse>.Forbidden("Создавать записи может только администратор");
        }

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
            CreatedByUserId = user.Id,
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

        return ServiceResult<BookingResponse>.Ok(WithNames(booking));
    }

    public ServiceResult<BookingResponse> Assign(int id, int assigneeUserId, CurrentUser user)
    {
        // Назначают администратор и управляющий. Тренер - нет.
        if (user.Role != UserRoles.Admin && user.Role != UserRoles.Manager)
        {
            return ServiceResult<BookingResponse>.Forbidden("Назначать тренера может администратор или управляющий");
        }

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

        // Администратор назначает только новой записи (ФТ-3), переназначает запись в работе управляющий (ФТ-4)
        if (user.Role == UserRoles.Admin && booking.Status != BookingStatuses.New)
        {
            return ServiceResult<BookingResponse>.Conflict(
                "Администратор назначает тренера только записи в статусе New, переназначает управляющий");
        }

        // Назначение не меняет статус. SQL: UPDATE bookings SET assignee_user_id = @trainer WHERE id = @id
        booking.AssigneeUserId = assigneeUserId;
        _db.SaveChanges();
        return ServiceResult<BookingResponse>.Ok(WithNames(booking));
    }

    public ServiceResult<BookingResponse> ChangeStatus(int id, string status, CurrentUser user)
    {
        // Статус меняет только тренер. Кто создаёт - тот не меняет статус (ЛР1).
        if (user.Role != UserRoles.Trainer)
        {
            return ServiceResult<BookingResponse>.Forbidden("Менять статус может только тренер");
        }

        var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return ServiceResult<BookingResponse>.NotFound($"Записи с id {id} нет");
        }

        if (booking.AssigneeUserId == null)
        {
            return ServiceResult<BookingResponse>.Conflict(
                "Нельзя менять статус: тренер ещё не назначен");
        }

        // Чужая запись: тренер вошёл, но запись назначена другому - 403.
        // Проверка здесь, на сервере: подставить чужой id в адрес не поможет.
        if (booking.AssigneeUserId != user.Id)
        {
            return ServiceResult<BookingResponse>.Forbidden(
                "Менять статус может только тренер, назначенный на эту запись");
        }

        if (!BookingStatuses.CanMove(booking.Status, status))
        {
            return ServiceResult<BookingResponse>.Conflict(
                $"Переход {booking.Status} → {status} недопустим");
        }

        // SQL: UPDATE bookings SET status = @status WHERE id = @id
        booking.Status = status;
        _db.SaveChanges();
        return ServiceResult<BookingResponse>.Ok(WithNames(booking));
    }

    // Подгрузить зал и тренера, чтобы ответ содержал их имена
    private BookingResponse WithNames(Booking booking)
    {
        var entry = _db.Entry(booking);
        entry.Reference(b => b.Hall).Load();
        entry.Reference(b => b.AssigneeUser).Load();
        return BookingResponse.From(booking);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}

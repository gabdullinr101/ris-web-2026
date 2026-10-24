# ЛР10. EF Core: сервер пишет в PostgreSQL

Сервер из ЛР8 перестал хранить записи в списке в памяти: теперь он через EF Core пишет в PostgreSQL. Создали запись → остановили API → запустили снова → запись на месте. Маршруты, JSON и коды ответов ЛР8 не изменились.

## Что добавлено

| Файл | Что в нём |
|------|-----------|
| `backend/backend.csproj` | пакеты `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 и `Microsoft.EntityFrameworkCore.Design` 8.0.32, ключ `UserSecretsId` |
| `backend/Data/AppDbContext.cs` | таблицы `halls`, `users`, `bookings`: имена столбцов, ключи, CHECK, индексы, справочные строки |
| `backend/Migrations/20261024105623_InitialCreate.cs` | миграция `InitialCreate` |
| `backend/Migrations/AppDbContextModelSnapshot.cs` | снимок модели для следующих миграций |
| `backend/Services/BookingService.cs` | вместо `List<Booking>` - `AppDbContext` |
| `backend/Program.cs` | `AddDbContext` с Npgsql, строка по ключу `ConnectionStrings:Default`; сервис стал `Scoped` |
| `backend/appsettings.json` | строка подключения **без пароля** - пример |

Контроллер не менялся: он по-прежнему получает `IBookingService` через DI. Поменялась только реализация сервиса - ровно то, ради чего DI заводили в ЛР7.

## Подготовка и запуск

```bash
dotnet tool install --global dotnet-ef --version 8.0.*
cd backend
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=fitness_bookings;Username=postgres;Password=ВАШ_ПАРОЛЬ"
dotnet ef database update
dotnet run
```

Хост, порт и пароль - свои. Если база `fitness_bookings` уже создана скриптами ЛР9, её нужно удалить перед `database update`: иначе миграция упадёт с `42P07: relation "halls" already exists` - проверено.

## Где пароль

- Пароль хранится в user-secrets - это файл в профиле пользователя (`%APPDATA%\Microsoft\UserSecrets\257f96f6-…\secrets.json`), вне репозитория.
- В `appsettings.json` - строка без пароля: `Host=localhost;Port=5432;Database=fitness_bookings;Username=postgres`.
- `git grep -i password` находит только описание команды в README и этом отчёте с заглушкой `ВАШ_ПАРОЛЬ`, настоящего пароля в репозитории нет.

## Сверка с ЛР9

Имена таблиц, столбцов, ключей, индексов и ограничений в `AppDbContext` заданы явно - те же, что в `db/schema.sql`.

| C# | PostgreSQL |
|----|------------|
| `Booking.Id` | `bookings.id` |
| `Booking.Number` | `bookings.number` (уникальный) |
| `Booking.HallId` | `bookings.hall_id` → `halls.id` |
| `Booking.Status` | `bookings.status`, CHECK четырёх значений |
| `Booking.CreatedAt` | `bookings.created_at` (`timestamptz`) |
| `Booking.CreatedByUserId` | `bookings.created_by_user_id` → `users.id` |
| `Booking.AssigneeUserId` | `bookings.assignee_user_id` → `users.id`, может быть `NULL` |
| `User.FullName` | `users.full_name` |
| `User.PasswordHash` | `users.password_hash` (заполнится в ЛР11) |

Единственное отличие нашлось при генерации миграции: EF сам заводит индекс на каждый внешний ключ, в том числе на `created_by_user_id`. Ему дано понятное имя `bookings_created_by_user_id_idx`, и такой же индекс добавлен в `db/schema.sql`, чтобы схемы совпадали.

Схема после `dotnet ef database update` (`\d bookings` в psql):

```text
Indexes:
    "bookings_pkey" PRIMARY KEY, btree (id)
    "bookings_assignee_user_id_idx" btree (assignee_user_id)
    "bookings_created_by_user_id_idx" btree (created_by_user_id)
    "bookings_hall_id_idx" btree (hall_id)
    "bookings_number_key" UNIQUE, btree (number)
    "bookings_status_idx" btree (status)
Check constraints:
    "bookings_description_check" CHECK (char_length(description) >= 10 AND char_length(description) <= 500)
    "bookings_status_check" CHECK (status = ANY (ARRAY['New'::text, 'InProgress'::text, 'Closed'::text, 'Cancelled'::text]))
    "bookings_title_check" CHECK (char_length(title) >= 5 AND char_length(title) <= 80)
Foreign-key constraints:
    "bookings_assignee_user_id_fkey" FOREIGN KEY (assignee_user_id) REFERENCES users(id) ON DELETE RESTRICT
    "bookings_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE RESTRICT
    "bookings_hall_id_fkey" FOREIGN KEY (hall_id) REFERENCES halls(id) ON DELETE RESTRICT
```

Таблица истории миграций:

```text
         MigrationId          | ProductVersion
------------------------------+----------------
 20261024105623_InitialCreate | 8.0.32
```

Поскольку имена совпадают, `db/queries.sql` из ЛР9 выполняется и на базе, созданной миграцией.

## Миграция, а не schema.sql

`schema.sql` - учебник: SQL, написанный руками. Миграция - файл, который EF построил по классам C#; `dotnet ef database update` применяет его к базе и записывает в `__EFMigrationsHistory`, что он уже применён. Изменили модель - делаем новую миграцию (`dotnet ef migrations add …`), старую не правим.

## LINQ и тот же SQL

Получить одну запись - `BookingService.GetById`:

```csharp
_db.Bookings.AsNoTracking().FirstOrDefault(b => b.Id == id);
```

SQL из лога EF (`dotnet run` в Development пишет команды в терминал):

```sql
SELECT b.id, b.assignee_user_id, b.created_at, b.created_by_user_id, b.description, b.hall_id, b.number, b.status, b.title
FROM bookings AS b
WHERE b.id = @__id_0
LIMIT 1
```

Смена статуса - `booking.Status = status; _db.SaveChanges();`:

```sql
UPDATE bookings SET status = @p0
WHERE id = @p1;
```

Значения передаются параметрами (`@__id_0`, `@p0`), а не склейкой строк, поэтому SQL-инъекция невозможна.

## Номер записи

Номер `БР-0001` строится из `id`, а `id` выдаёт база при вставке. Поэтому `Create` в одной транзакции вставляет запись с временным номером, узнаёт `id` и записывает настоящий номер. Номер уникален в базе (`bookings_number_key`); если вдруг повторится - ответ 409, а не 500.

## Рестарт-тест

1. `dotnet ef database update` на пустой базе, `dotnet run`.
2. Прогнал коллекцию `docs/api-collection/bookings.postman_collection.json`: 10 запросов, 16 проверок, 0 провалов - коды ЛР8 не изменились.
3. Запись до остановки сервера - `GET /api/bookings/1`:

   ```json
   {"id":1,"number":"БР-0001","title":"Персональная тренировка, силовая","hallId":2,"status":"InProgress","assigneeUserId":2}
   ```

4. Остановил сервер (`Ctrl+C`), снова `dotnet run`.
5. `GET /api/bookings/1` - 200, та же запись: тот же `id`, `number`, `title`, статус и тренер.
6. В базе:

   ```text
    id | number  |              title               |   status   | assignee_user_id
   ----+---------+----------------------------------+------------+------------------
     1 | БР-0001 | Персональная тренировка, силовая | InProgress |                2
   (1 row)
   ```

Один и тот же `id = 1` до и после рестарта.

## Чеклист

- [x] пакеты EF Core и Npgsql подключены, `dotnet ef --version` → 8.0.32
- [x] пароль в user-secrets, не в Git
- [x] сущности и `AppDbContext` совпадают с `db/schema.sql`
- [x] миграция `InitialCreate` создана и лежит в Git
- [x] `dotnet ef database update` создаёт таблицы
- [x] POST, оба GET и оба PATCH работают через базу
- [x] нет записи - 404
- [x] рестарт-тест: тот же `id` найден
- [x] `DELETE` не добавлен

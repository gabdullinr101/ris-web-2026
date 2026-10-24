# ЛР8. REST: записи на тренировку и проверка полей

Сервер из ЛР7 научился работать с главным объектом - записью на тренировку: создать, прочитать список и одну запись, отдельно назначить тренера, отдельно сменить статус. Данные пока в памяти процесса: после перезапуска сервера список пустой, это ожидаемо.

## Запуск

```bash
cd backend
dotnet restore
dotnet run
```

Базовый адрес: `http://localhost:5000`, Swagger: `http://localhost:5000/swagger`.

## Маршруты

| Метод и путь | Успех | Отказы |
|--------------|-------|--------|
| `GET /api/bookings` | 200, массив; пустой - `[]` | - |
| `GET /api/bookings/{id}` | 200 | 404 |
| `POST /api/bookings` | 201, `id`, `number`, `status: New` | 400 |
| `PATCH /api/bookings/{id}/assignee` | 200, статус не меняется | 400, 404, 409 |
| `PATCH /api/bookings/{id}/status` | 200 | 400, 404, 409 |

Пути и поля - из docs/api-contract.md (ЛР2). `DELETE` нет, отмена - статус `Cancelled`. Общего PATCH «изменить всё» нет.

## Файлы

```text
backend/
├── Controllers/BookingsController.cs      ← HTTP: принял, вызвал сервис, вернул код
├── Dtos/
│   ├── CreateBookingRequest.cs            ← вход создания: title, hallId, description
│   ├── AssignBookingRequest.cs            ← вход назначения: assigneeUserId
│   ├── ChangeBookingStatusRequest.cs      ← вход смены статуса: status
│   └── BookingResponse.cs                 ← ответ о записи
├── Models/
│   ├── Booking.cs, Hall.cs, User.cs
│   ├── BookingStatuses.cs                 ← четыре статуса и допустимые переходы
│   └── UserRoles.cs
├── Services/
│   ├── IBookingService.cs
│   ├── BookingService.cs                  ← список в памяти и правила процесса
│   └── ServiceResult.cs                   ← Ok / Invalid / NotFound / Conflict
└── Program.cs
docs/api-collection/bookings.postman_collection.json
```

Имена по словарю ЛР2: Ticket → Booking, Site → Hall. В методичке это `TicketsController`, `CreateTicketRequest`, `TicketResponse`, `TicketService`, `/api/tickets`, `siteId`.

## Контракт данных

Создание - клиент присылает только поля, которые заполняет человек:

```json
{
  "title": "Персональная тренировка, силовая",
  "hallId": 2,
  "description": "Клиент новичок, нужна программа на 4 недели"
}
```

`id`, `number`, `status: New`, пустого тренера и время создания задаёт сервер. Если клиент пришлёт `"status": "Closed"` или `"id": 500`, сервер их игнорирует - проверено, запись создаётся в `New` со следующим по порядку `id`.

Ответ 201:

```json
{
  "id": 1,
  "number": "БР-0001",
  "title": "Персональная тренировка, силовая",
  "hallId": 2,
  "description": "Клиент новичок, нужна программа на 4 недели",
  "status": "New",
  "createdByUserId": 1,
  "assigneeUserId": null,
  "createdAt": "2026-10-24T10:33:20.7842095Z"
}
```

Назначение - `{ "assigneeUserId": 2 }`, смена статуса - `{ "status": "InProgress" }`.

## Правила

Проверка полей (ответ 400):

- `title` - 5-80 символов после обрезки пробелов;
- `description` - 10-500 символов после обрезки пробелов;
- `hallId` - целое больше 0 и есть в справочнике залов;
- `assigneeUserId` - существующий тренер;
- `status` - только `New`, `InProgress`, `Closed`, `Cancelled`.

Длины проверяют атрибуты на DTO (`[StringLength]`, `[Range]`), `[ApiController]` сам превращает ошибку в 400. Строку из пробелов и существование зала проверяет сервис.

Правила процесса (ответ 409) - в `BookingService`, не в контроллере:

- переходы только `New → InProgress`, `New → Cancelled`, `InProgress → Closed`, `InProgress → Cancelled`;
- нельзя менять статус, пока тренер не назначен;
- статус меняет только назначенный тренер своей записи;
- нельзя назначить тренера проведённой или отменённой записи.

Кто выполняет действие - заголовок `X-User-Id` (нужен для `POST` и `PATCH .../status`). Настоящий вход - в ЛР11. До этого чужой переход тоже 409, кода 403 пока нет. Ограничение «создаёт только администратор» из docs/api-contract.md (403) тоже появится в ЛР11.

## Проверка

Все запросы прогнаны на свежем сервере.

| Запрос | Ожидание | Факт |
|--------|----------|------|
| `GET /api/bookings` до создания | 200, `[]` | 200, `[]` |
| `POST` правильная запись | 201, `id`, `number`, `New` | 201, `id: 1`, `number: БР-0001`, `status: New`, заголовок `Location: /api/bookings/1` |
| `POST` вид тренировки `Сил` | 400 | 400, `errors.title: ["Длина от 5 до 80 символов"]` |
| `POST` вид тренировки из пробелов | 400 | 400, `errors.title: ["Укажите вид тренировки"]` |
| `POST` зал `99` | 400 | 400, `errors.hallId: ["Зал с таким номером не найден"]` |
| `POST` без `X-User-Id` | 400 | 400, `errors.X-User-Id` |
| `POST` битый JSON | 400 | 400 |
| `GET /api/bookings/999999` | 404 | 404, `detail: "Записи с id 999999 нет"` |
| `PATCH /1/status` до назначения | 409 | 409, «Нельзя менять статус: тренер ещё не назначен» |
| `PATCH /1/assignee` `{ "assigneeUserId": 1 }` (администратор) | 400 | 400, «Тренер с таким номером не найден» |
| `PATCH /1/assignee` `{ "assigneeUserId": 2 }` | 200, статус `New` | 200, `assigneeUserId: 2`, `status: New` |
| `PATCH /1/status` от тренера 3 | 409 | 409, «Менять статус может только тренер, назначенный на эту запись» |
| `PATCH /1/status` `Done` | 400 | 400, `errors.status` |
| `PATCH /1/status` `New → Closed` | 409 | 409, «Переход New → Closed недопустим» |
| `PATCH /1/status` `InProgress` от тренера 2 | 200 | 200, `status: InProgress` |
| `PATCH /999999/status` | 404 | 404 |

Ответ 400, пример:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "title": ["Длина от 5 до 80 символов"] }
}
```

Ответ 409, пример:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Действие противоречит правилам процесса",
  "status": 409,
  "detail": "Нельзя менять статус: тренер ещё не назначен"
}
```

## Коллекция запросов

Файл `docs/api-collection/bookings.postman_collection.json` - 10 запросов с проверками (тесты Postman). Импорт: Postman → Import → файл; Insomnia → Import → From File. Базовый URL - переменная `baseUrl` = `http://localhost:5000`. Запускать по порядку на только что запущенном сервере: запрос 02 сохраняет `id` созданной записи в переменную `bookingId`.

| № | Запрос | Ожидание |
|---|--------|----------|
| 01 | `GET` список | 200 |
| 02 | `POST` создать | 201, `id`, `number`, `New` |
| 03 | `POST` короткий вид тренировки | 400 |
| 04 | `GET` несуществующий id | 404 |
| 05 | `GET` созданная запись | 200 |
| 06 | `PATCH` статус без тренера | 409 |
| 07 | `PATCH` назначить тренера | 200, статус `New` |
| 08 | `PATCH` статус чужим тренером | 409 |
| 09 | `PATCH` статус своим тренером | 200, `InProgress` |
| 10 | `PATCH` `InProgress → New` | 409 |

Прогон всей коллекции: 10 запросов, 16 проверок, 0 провалов.

## Лог неожиданного сбоя

Учебный сбой включается переменной окружения `Debug__FailCreate=true` и работает только в Development (в cmd: `set Debug__FailCreate=true`, затем `dotnet run`). Сервис бросает `InvalidOperationException` после успешной проверки полей.

В терминале сервера - одна строка: операция, зал, тип ошибки. Тела запроса целиком нет:

```text
fail: backend.Controllers.BookingsController[0]
      CreateBooking failed; hallId=2; error=InvalidOperationException
```

Клиент получает безопасный 500 без stack trace, путей к файлам и тела запроса:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Не удалось обработать запрос",
  "status": 500
}
```

Без переменной (обычный запуск) и вне Development сбой не срабатывает: тот же `POST` даёт 201. Другие неожиданные исключения ловит `app.UseExceptionHandler()` и тоже отвечает `ProblemDetails` без stack trace.

## Чеклист

### API

- [x] POST создаёт запись и возвращает 201
- [x] ответ содержит `id`, `number`, `status: "New"`
- [x] короткий title возвращает 400
- [x] несуществующий id возвращает 404
- [x] нарушение правила процесса возвращает 409
- [x] есть два разных PATCH
- [x] DELETE отсутствует
- [x] все четыре статуса написаны одинаково

### Артефакты

- [x] коллекция лежит в `/docs/api-collection/`
- [x] в коллекции есть 201, 400, 404 и 409
- [x] README указывает `http://localhost:5000`
- [x] README говорит, что данные пока в памяти
- [x] в отчёте есть строка лога
- [x] клиент не получает stack trace

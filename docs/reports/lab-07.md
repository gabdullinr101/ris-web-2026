# ЛР7. Каркас ASP.NET Core Web API

В репозитории появилась папка `backend/`: проект ASP.NET Core Web API (.NET 8) рядом с `frontend/` и `docs/`. Сервер слушает `http://localhost:5000`, в режиме Development открывается Swagger, есть служебные адреса `/health` и `/info`. Клиент Vue сегодня не подключается.

## Запуск

```bash
cd backend
dotnet restore
dotnet run
```

Вывод терминала:

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

## Адреса

| Адрес | Ответ |
|-------|-------|
| `http://localhost:5000/swagger` | страница Swagger со списком адресов (только в Development) |
| `GET http://localhost:5000/health` | `200`, текст `ok` |
| `GET http://localhost:5000/info` | `200`, `{"name":"Бронирование тренировок · API","environment":"Development","version":"ЛР7: каркас Web API"}` |
| `GET http://localhost:5000/api/bookings` | `200`, `[]` - заготовка контроллера, записей пока нет |

`POST /health` и `POST /api/bookings` отвечают `405 Method Not Allowed`: создавать записи сервер ещё не умеет.

Порт 5000 задан в профиле `http` файла `Properties/launchSettings.json` - как на схеме из ЛР2 (docs/architecture.md). Перенаправление на HTTPS из шаблона отключено, проверка курса идёт по HTTP.

## Структура `backend/`

```text
backend/
├── backend.csproj
├── backend.http                    ← готовые запросы для проверки из IDE
├── Program.cs                      ← Swagger, DI, /health, /info
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json         ← http://localhost:5000
├── Controllers/
│   └── BookingsController.cs       ← заготовка: GET /api/bookings
├── Services/
│   ├── IBookingService.cs          ← что умеет сервис
│   └── BookingService.cs           ← заготовка: список в памяти
├── Models/
│   └── Booking.cs                  ← поля записи из docs/data-model.md
└── Data/
    └── README.md                   ← здесь будет доступ к PostgreSQL (ЛР10)
```

Имена по словарю ЛР2: Ticket → Booking. В методичке это `TicketsController`, `TicketService`, `/api/tickets`.

Шаблонный пример `WeatherForecast` удалён.

## Контроллер, сервис, DI

`/health` и `/info` сделаны через Minimal API прямо в `Program.cs`: это короткие служебные адреса. Записи на тренировку обслуживает контроллер `BookingsController`.

Контроллер только принимает HTTP-запрос, вызывает сервис и возвращает ответ. Правила и хранение - в сервисе.

Сервис зарегистрирован через DI, чтобы контроллер не создавал реализацию через `new`:

```csharp
builder.Services.AddSingleton<IBookingService, BookingService>();
```

Контроллер получает `IBookingService` в конструкторе. В ЛР10 вместо `BookingService` со списком в памяти можно будет подставить реализацию с PostgreSQL, не переписывая контроллер. Singleton - один экземпляр на всё приложение, поэтому все запросы видят один и тот же список.

## Чеклист: `/health` ≠ создание

| Утверждение | да/нет |
|-------------|--------|
| `/health` возвращает `200` без тела записи (текст `ok`) | да |
| `/health` не принимает `title`, `hallId`, `description` | да |
| Успешный `/health` не доказывает работу `POST /api/bookings` | да |
| Создание записи относится к ЛР8 | да |
| README прямо объясняет это различие | да |
| Swagger и `/health` проверены отдельно | да |

## Чеклист

### Запуск

- [x] `dotnet --version` работает
- [x] проект находится в `/backend`
- [x] `dotnet restore` проходит
- [x] `dotnet run` проходит
- [x] HTTP-порт равен `5000`
- [x] команды запуска записаны в README

### API-каркас

- [x] Swagger открывается в Development
- [x] `GET /health` возвращает `200`
- [x] `GET /info` возвращает сведения о приложении
- [x] `/health` не принимает данные записи
- [x] создание записи не выдано за готовое
- [x] папки `Controllers`, `Services`, `Models`, `Data` есть в Git
- [x] сервис зарегистрирован через DI

### Git и документация

- [x] `bin` и `obj` не отслеживаются
- [x] в README есть адрес Swagger
- [x] в README есть фраза о данных в памяти и перезапуске
- [x] в README объяснено, что `/health` - только живость
- [x] заполнен чеклист `/health` ≠ создание

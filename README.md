# Веб-сервис управления бронированием тренировок и занятостью тренеров

Курс: Б1.В.ДЭ.01.02.02, КГЭУ, семестр 1, 2026/2027
Студент: Габдуллин Радик Азатович, группа: ТРИС-1-23
Тема: Разработка веб-сервиса управления бронированием тренировок и занятостью тренеров
Статус темы: на проверке (заявка в docs/project-proposal.md)

Фитнес-клуб: администратор ресепшн записывает клиента на тренировку и назначает тренера, тренер подтверждает и проводит свои тренировки, управляющий видит все записи.

## Стек

| Узел | Технологии | Адрес |
|------|------------|-------|
| Клиент (`frontend`) | Vue 3 + Vite, JavaScript, vue-router; Node.js 22.18+ | `http://localhost:5173` |
| Сервер (`backend`) | ASP.NET Core Web API, .NET 8, EF Core 8 + Npgsql | `http://localhost:5000` |
| База | PostgreSQL 14+ (проверено на 16) | локальный порт, обычно 5432 |

Браузер ходит только в API, в базу - только сервер (docs/architecture.md).

## Подготовка (один раз)

Проверить инструменты: `node -v`, `npm -v`, `dotnet --version`. Должен быть установлен PostgreSQL с pgAdmin.

1. Инструмент миграций EF Core:

   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.*
   ```

   Если пишет, что уже установлен, - всё в порядке.

2. Строка подключения с паролем - только локально, через user-secrets (в репозиторий не попадает). Хост, порт и пароль - свои: порт виден в pgAdmin → сервер → Properties → Connection.

   ```bash
   cd backend
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=fitness_bookings;Username=postgres;Password=ВАШ_ПАРОЛЬ"
   ```

   В `backend/appsettings.json` лежит та же строка без пароля - как пример.

3. Создать таблицы миграциями (база `fitness_bookings` создастся сама):

   ```bash
   cd backend
   dotnet ef database update
   ```

   Применяются `InitialCreate` (таблицы, 3 зала, 4 пользователя) и `AddUserPasswordHashes` (hash учебных паролей). Если база `fitness_bookings` уже создана скриптами ЛР9 из папки `db`, сначала удалите её (pgAdmin → правой кнопкой по базе → Delete), иначе миграция остановится на `relation "halls" already exists`.

4. Настройки клиента:

   ```bash
   cd frontend
   copy .env.example .env
   npm install
   ```

   В Linux и macOS вместо `copy` - `cp`. В `.env`: `VITE_API_BASE_URL=http://localhost:5000` и `VITE_USE_MOCK=false` (рабочий режим - настоящий сервер; `true` включает учебный mock из ЛР6 без сервера, в нём работают только список и создание).

## Запуск

Два терминала.

Первый - сервер:

```bash
cd backend
dotnet run
```

Ждать строку `Now listening on: http://localhost:5000`. Swagger: `http://localhost:5000/swagger`, проверка живости: `http://localhost:5000/health` → `ok`.

Второй - клиент:

```bash
cd frontend
npm run dev
```

Открыть `http://localhost:5173`. После правки `.env` перезапустить `npm run dev`.

## Учебные логины

Пароли только для этого проекта, в базе хранится их hash.

| Логин | Пароль | Кто | Что может |
|-------|--------|-----|-----------|
| `admin` | `Fitness2026!` | Анна Ковалёва, администратор ресепшн | создаёт записи, назначает тренера новой записи |
| `trainer` | `Fitness2026!` | Илья Соколов, тренер | видит свои записи, подтверждает, проводит, отменяет |
| `trainer2` | `Fitness2026!` | Марат Хайруллин, тренер | то же, что `trainer` |
| `manager` | `Fitness2026!` | Ольга Петрова, управляющий клуба | видит все записи, назначает и переназначает тренера |

Полная таблица прав - `docs/role-matrix.md`.

## Проверочный сценарий

1. Войти `admin` (`/login`).
2. «Создать» → вид тренировки, номер зала (1 - тренажёрный, 2 - единоборств, 3 - групповых программ), описание → «Создать». В списке новая строка: запишите её `id` и номер `БР-…`, статус «Новая».
3. В строке выбрать тренера «Илья Соколов» → «Назначить». Статус остаётся «Новая».
4. Выйти (`Выход` → «Выйти»), войти `trainer`. В списке только его записи. В строке нажать «Подтвердить» - статус «Подтверждена тренером».
5. Тот же `id` в базе (pgAdmin → Query Tool на базе `fitness_bookings`):

   ```sql
   SELECT id, number, title, status, assignee_user_id FROM bookings WHERE id = 1;
   ```

6. Остановить сервер (`Ctrl+C`), снова `dotnet run`, обновить список - запись на месте.

Подробно, с тем, где смотреть каждый шаг, - `docs/acceptance-check.md`.

## API

| Метод и путь | Успех | Отказы |
|--------------|-------|--------|
| `POST /api/auth/login` | 200, пользователь; cookie входа | 401 |
| `POST /api/auth/logout` | 200 | - |
| `GET /api/auth/me` | 200 | 401 |
| `GET /api/users/trainers` | 200 | 401, 403 |
| `GET /api/bookings` | 200, массив или `[]` | 401 |
| `GET /api/bookings/{id}` | 200 | 401, 403, 404 |
| `POST /api/bookings` | 201, `id`, `number`, `status: New` | 400, 401, 403 |
| `PATCH /api/bookings/{id}/assignee` | 200, статус не меняется | 400, 401, 403, 404, 409 |
| `PATCH /api/bookings/{id}/status` | 200 | 400, 401, 403, 404, 409 |

Подробно - docs/api-contract.md. `DELETE` нет: отмена - статус `Cancelled`.

Вход - через cookie `fitness_auth` (HttpOnly), не JWT. CORS разрешён только для `http://localhost:5173` с cookie.

Коллекция запросов: `docs/api-collection/bookings.postman_collection.json` (Postman: Import → файл; Insomnia: Import → From File). Базовый URL - переменная `baseUrl` = `http://localhost:5000`. Запускать по порядку: запросы «Вход» ставят cookie, следующие идут от имени этого пользователя.

Учебный сбой для проверки лога (ЛР8): запустить сервер с переменной окружения `Debug__FailCreate=true` (в cmd: `set Debug__FailCreate=true`, затем `dotnet run`). Работает только в Development: `POST /api/bookings` отвечает 500 «Не удалось обработать запрос», в терминале сервера - строка `CreateBooking failed; hallId=…; error=InvalidOperationException`.

## База данных: скрипты ЛР9

Папка `db` - учебная SQL-справка по схеме, рабочую базу создаёт миграция.

- `schema.sql` - таблицы `halls`, `users`, `bookings`, ключи, CHECK статусов, индексы;
- `seed.sql` - 3 зала, 4 пользователя, 8 записей во всех четырёх статусах;
- `queries.sql` - фильтр с сортировкой, JOIN, GROUP BY и проверка ограничений (последние три вставки должны завершиться ошибкой - так задумано).

Прогнать на отдельной учебной базе (пароль `postgres` спросит сама команда; если `psql` не находится, укажите полный путь, например `"C:\Program Files\PostgreSQL\16\bin\psql.exe"`, и свой порт через `-p`):

```bash
createdb -U postgres fitness_sql
psql -U postgres -d fitness_sql -f db/schema.sql
psql -U postgres -d fitness_sql -f db/seed.sql
psql -U postgres -d fitness_sql -f db/queries.sql
```

Имена таблиц и столбцов совпадают с миграцией, поэтому `queries.sql` работает и на базе сервера `fitness_bookings`.

## Ограничения

- `/health` показывает только, что процесс сервера жив, и не проверяет создание записей и базу;
- регистрации и смены пароля нет - две роли и четыре учебные учётки заведены миграцией;
- в учебном mock (`VITE_USE_MOCK=true`) нет входа, назначения и смены статуса;
- остальное - `docs/known-defects.md`.

## Документы
- [Заявка на тему](docs/project-proposal.md)
- [Требования (ЛР1)](docs/lab-01-requirements.md)
- [Архитектура](docs/architecture.md)
- [Модель данных](docs/data-model.md)
- [API-контракт](docs/api-contract.md)
- [Матрица требований](docs/requirements-matrix.md)
- [Клиент: каркас и экраны (ЛР3)](docs/lab-03-frontend.md)
- [Клиент: компоненты и маршруты (ЛР4)](docs/reports/lab-04.md)
- [Клиент: форма и список в памяти (ЛР5)](docs/reports/lab-05.md)
- [Клиент: API-модуль, mock и состояния экранов (ЛР6)](docs/reports/lab-06.md)
- [Сервер: каркас ASP.NET Core Web API (ЛР7)](docs/reports/lab-07.md)
- [Сервер: REST записей и проверка полей (ЛР8)](docs/reports/lab-08.md)
- [База данных: PostgreSQL и SQL (ЛР9)](docs/reports/lab-09.md)
- [Сервер: EF Core и PostgreSQL (ЛР10)](docs/reports/lab-10.md)
- [Матрица ролей](docs/role-matrix.md)
- [Вход и права (ЛР11)](docs/reports/lab-11.md)
- [Проверка трёх критериев на стенде](docs/acceptance-check.md)
- [Известные дефекты](docs/known-defects.md)
- [Сквозная сборка (ЛР12)](docs/reports/lab-12.md)
- [Предзащита: стенд и план демо](docs/predefense-check.md)
- [Пояснительная записка (рабочий вариант)](docs/kursovaya-zapiska.md)
- [Предзащита (ЛР13)](docs/reports/lab-13.md)

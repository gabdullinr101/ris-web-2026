# Веб-сервис управления бронированием тренировок и занятостью тренеров

Курс: Б1.В.ДЭ.01.02.02, КГЭУ, семестр 1, 2026/2027
Студент: Габдуллин Радик Азатович, группа: ТРИС-1-23
Тема: Разработка веб-сервиса управления бронированием тренировок и занятостью тренеров
Статус темы: на проверке (заявка в docs/project-proposal.md)
Стек: Vue 3 + ASP.NET Core + PostgreSQL (клиент - в папке frontend, сервер - в папке backend, база позже)


## Запуск клиента

Нужен Node.js LTS (22.18 или новее). Проверка: `node -v` и `npm -v`.

```bash
cd frontend
copy .env.example .env
npm install
npm run dev
```

Вторая строка создаёт локальный файл настроек `.env` (в Linux и macOS вместо `copy` - `cp`):

- `VITE_API_BASE_URL=http://localhost:5000` - адрес сервера;
- `VITE_USE_MOCK=true` - список и создание отвечает учебный mock, сервер не нужен; `false` - настоящий сервер на :5000 (нужно запустить backend).

Вход (`/login`) всегда идёт на настоящий сервер. После правки `.env` нужно перезапустить `npm run dev`.

Открыть адрес Vite, обычно `http://localhost:5173`.

Адреса экранов (меню в шапке или вручную в адресной строке):

- `http://localhost:5173/` - список записей на тренировки;
- `http://localhost:5173/new` - форма новой записи;
- `http://localhost:5173/login` - вход.

С `VITE_USE_MOCK=true` список и создание работают через учебный mock (`frontend/src/api/mockBookings.js`): данные живут в памяти вкладки и после F5 пропадают. С `VITE_USE_MOCK=false` клиент работает с сервером и базой.

## Запуск сервера

Нужны .NET SDK 8 или новее (`dotnet --version`) и PostgreSQL (см. раздел «База данных» ниже). С ЛР10 сервер хранит записи в PostgreSQL через EF Core.

### Подготовка (один раз)

1. Инструмент миграций:

   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.*
   ```

   Если пишет, что уже установлен, - всё в порядке.

2. Строка подключения с паролем - только локально, через user-secrets (в репозиторий не попадает). Хост, порт и пароль - свои: порт виден в pgAdmin → сервер → Properties → Connection.

   ```bash
   cd backend
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=fitness_bookings;Username=postgres;Password=ВАШ_ПАРОЛЬ"
   ```

   В `appsettings.json` лежит та же строка без пароля - как пример.

3. Если база `fitness_bookings` уже создана скриптами ЛР9, удалите её (pgAdmin → правой кнопкой по базе → Delete, или `dropdb -U postgres fitness_bookings`). Миграция создаст базу и таблицы сама.

4. Применить миграции:

   ```bash
   cd backend
   dotnet ef database update
   ```

   Создаются таблицы `halls`, `users`, `bookings` и справочные строки: 3 зала и 4 пользователя.

### Запуск

```bash
cd backend
dotnet restore
dotnet run
```

В терминале должно появиться `Now listening on: http://localhost:5000`. Остановить сервер - `Ctrl+C`.

- API: `http://localhost:5000`
- Swagger (список адресов с кнопкой «Try it out», работает в режиме Development): `http://localhost:5000/swagger`
- Проверка живости: `http://localhost:5000/health` - ответ `ok`
- Сведения о приложении: `http://localhost:5000/info` - имя, среда, версия

### Записи на тренировку

| Метод и путь | Успех | Отказы |
|--------------|-------|--------|
| `GET /api/bookings` | 200, массив или `[]` | - |
| `GET /api/bookings/{id}` | 200 | 404 |
| `POST /api/bookings` | 201, `id`, `number`, `status: New` | 400 |
| `PATCH /api/bookings/{id}/assignee` | 200, статус не меняется | 400, 404, 409 |
| `PATCH /api/bookings/{id}/status` | 200 | 400, 404, 409 |

Имена как в ЛР2 (docs/api-contract.md): ресурс `bookings`, поля `title`, `hallId`, `description`, `assigneeUserId`, `status`. `DELETE` нет: отмена - статус `Cancelled`.

Без входа все адреса `/api/bookings` отвечают 401. Права по ролям - в `docs/role-matrix.md` (ЛР11).

Залы: 1 - тренажёрный, 2 - единоборств, 3 - групповых программ.

Коллекция запросов для проверки: `docs/api-collection/bookings.postman_collection.json`. В Postman: Import → выбрать файл; в Insomnia: Import → From File. Базовый URL в переменной `baseUrl` - `http://localhost:5000`. Запускать по порядку: запросы «Вход» ставят cookie, следующие идут от имени этого пользователя.

### Вход и права (ЛР11)

Вход - через cookie (не JWT): `POST /api/auth/login` с логином и паролем, сервер ставит cookie `fitness_auth` (HttpOnly), браузер сам присылает её дальше. Выход - `POST /api/auth/logout`, кто вошёл - `GET /api/auth/me`.

Учебные учётки (пароли только для этого проекта):

| Логин | Пароль | Кто |
|-------|--------|-----|
| `admin` | `Fitness2026!` | Анна Ковалёва, администратор ресепшн - создаёт и назначает |
| `trainer` | `Fitness2026!` | Илья Соколов, тренер - двигает статус своих записей |
| `trainer2` | `Fitness2026!` | Марат Хайруллин, тренер |
| `manager` | `Fitness2026!` | Ольга Петрова, управляющий клуба - назначает и переназначает |

В базе пароли хранятся только в виде hash (`users.password_hash`), их кладёт миграция `AddUserPasswordHashes`.

Кратко: без входа - 401; вошёл, но действие не твоё (тренер создаёт запись, тренер двигает чужую запись, администратор меняет статус) - 403. Полная таблица - `docs/role-matrix.md`.

Учебный сбой для проверки лога: запустить сервер с переменной окружения `Debug__FailCreate=true` (в cmd: `set Debug__FailCreate=true` перед `dotnet run`). Работает только в Development. Тогда `POST /api/bookings` отвечает 500 «Не удалось обработать запрос», а в терминале сервера появляется строка `CreateBooking failed; hallId=2; error=InvalidOperationException`. В обычном запуске выключен.

Ограничения:

- `/health` проверяет только, что процесс сервера жив, и не доказывает, что создание записи работает;
- клиент Vue к серверу ещё не подключён (по-прежнему работает на mock), стыковка - в ЛР12.

## База данных (ЛР9)

Нужен PostgreSQL 14 или новее (pgAdmin ставится вместе с ним). Пароль пользователя `postgres` - тот, что задали при установке; в файлы проекта его не кладём.

Скрипты в папке `db`:

- `schema.sql` - таблицы `halls`, `users`, `bookings`, ключи, CHECK статусов, индексы. Можно прогонять повторно: в начале удаляет только свои три таблицы;
- `seed.sql` - учебные данные: 3 зала, 4 пользователя, 8 записей во всех четырёх статусах;
- `queries.sql` - запросы: фильтр с сортировкой, JOIN, GROUP BY и проверка ограничений (последние три вставки должны завершиться ошибкой - так и задумано).

Из корня репозитория (если `psql` не находится, укажите полный путь, например `"C:\Program Files\PostgreSQL\16\bin\psql.exe"`):

```bash
createdb -U postgres fitness_bookings
psql -U postgres -d fitness_bookings -f db/schema.sql
psql -U postgres -d fitness_bookings -f db/seed.sql
psql -U postgres -d fitness_bookings -f db/queries.sql
```

В pgAdmin: создать базу `fitness_bookings` → Query Tool → открыть и выполнить по очереди `schema.sql`, `seed.sql`, `queries.sql`.

Скрипты `db` - учебная справка по схеме (ЛР9). Рабочую базу для сервера создаёт миграция EF Core (см. «Запуск сервера»); имена таблиц и столбцов у них одинаковые, поэтому `queries.sql` можно выполнять и на базе сервера.

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

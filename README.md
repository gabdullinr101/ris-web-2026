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

Вторая строка создаёт локальный файл настроек `.env` с адресом сервера (`VITE_API_BASE_URL=http://localhost:5000`). В Linux и macOS вместо `copy` - `cp`. После правки `.env` нужно перезапустить `npm run dev`.

Открыть адрес Vite, обычно `http://localhost:5173`.

Адреса экранов (меню в шапке или вручную в адресной строке):

- `http://localhost:5173/` - список записей на тренировки;
- `http://localhost:5173/new` - форма новой записи;
- `http://localhost:5173/login` - вход.

Сервер и база для запуска не нужны: список и создание работают через учебный mock (`frontend/src/api/mockBookings.js`). Данные живут в памяти вкладки, после перезагрузки страницы (F5) добавленные записи исчезают, это ожидаемо. Кнопка входа пока ничего не делает.

## Запуск сервера

Нужен .NET SDK 8 или новее. Проверка: `dotnet --version`.

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

### Записи на тренировку (ЛР8)

| Метод и путь | Успех | Отказы |
|--------------|-------|--------|
| `GET /api/bookings` | 200, массив или `[]` | - |
| `GET /api/bookings/{id}` | 200 | 404 |
| `POST /api/bookings` | 201, `id`, `number`, `status: New` | 400 |
| `PATCH /api/bookings/{id}/assignee` | 200, статус не меняется | 400, 404, 409 |
| `PATCH /api/bookings/{id}/status` | 200 | 400, 404, 409 |

Имена как в ЛР2 (docs/api-contract.md): ресурс `bookings`, поля `title`, `hallId`, `description`, `assigneeUserId`, `status`. `DELETE` нет: отмена - статус `Cancelled`.

Кто выполняет действие - заголовок `X-User-Id` (нужен для `POST` и `PATCH .../status`). Настоящий вход появится в ЛР11, до тех пор учебные пользователи такие:

| X-User-Id | Кто |
|-----------|-----|
| 1 | Анна Ковалёва, администратор ресепшн |
| 2 | Илья Соколов, тренер |
| 3 | Марат Хайруллин, тренер |
| 4 | Ольга Петрова, управляющий клуба |

Залы: 1 - тренажёрный, 2 - единоборств, 3 - групповых программ.

Коллекция запросов для проверки: `docs/api-collection/bookings.postman_collection.json`. В Postman: Import → выбрать файл; в Insomnia: Import → From File. Базовый URL в переменной `baseUrl` - `http://localhost:5000`. Запускать по порядку на только что запущенном сервере.

Учебный сбой для проверки лога: запустить сервер с переменной окружения `Debug__FailCreate=true` (в cmd: `set Debug__FailCreate=true` перед `dotnet run`). Работает только в Development. Тогда `POST /api/bookings` отвечает 500 «Не удалось обработать запрос», а в терминале сервера появляется строка `CreateBooking failed; hallId=2; error=InvalidOperationException`. В обычном запуске выключен.

Ограничения:

- данные записей пока в памяти, после перезапуска сервера пропадут;
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

Сервер пока к базе не подключён и хранит записи в памяти - это ЛР10.

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

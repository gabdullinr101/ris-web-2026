# ЛР12. Сквозная сборка

Все три части работают вместе: Vue на `:5173` → ASP.NET Core на `:5000` → PostgreSQL. Mock выключен. Одна и та же запись (`id = 1`, `БР-0001`) прослежена на экране, во вкладке Network и в таблице `bookings`, и пережила перезапуск API.

## Что сделано

| Часть | Изменение |
|-------|-----------|
| `frontend/.env.example` | `VITE_USE_MOCK=false` - рабочий режим с сервером; `VITE_API_BASE_URL=http://localhost:5000` |
| `frontend/src/api/bookings.js` | mock включается только явным `VITE_USE_MOCK=true`; добавлены `assignBooking`, `changeBookingStatus` |
| `frontend/src/api/users.js` | `getTrainers` - список тренеров для назначения |
| `frontend/src/components/BookingTable.vue` | колонки `id`, зал по названию, тренер, статус по-русски; в строке - выбор тренера и «Назначить» (администратор, управляющий), «Подтвердить» / «Провести» / «Отменить» (назначенный тренер). Таблица сама ничего не отправляет - сообщает странице событиями |
| `frontend/src/pages/BookingList.vue` | выполняет назначение и смену статуса через API, показывает результат и текст отказа |
| `frontend/src/statuses.js` | названия статусов из ЛР1: «Новая», «Подтверждена тренером», «Проведена», «Отменена» |
| `backend/Dtos/BookingResponse.cs` | в ответе также `hallName` и `assigneeFullName`, чтобы экран не показывал голые номера |
| `backend/Program.cs` | CORS для `http://localhost:5173` с cookie - с ЛР11 |
| `README.md` | переписан как инструкция поставки: стек, подготовка, запуск, логины, сценарий |
| `docs/acceptance-check.md` | три критерия ЛР1: шаг, ожидание, где смотреть |
| `docs/known-defects.md` | найденные дефекты: 5 открытых, 2 закрыты |

## Порты

| Узел | Адрес |
|------|-------|
| Vue | `http://localhost:5173` |
| ASP.NET Core | `http://localhost:5000` |
| PostgreSQL | локальный, по строке подключения в user-secrets |

## CORS

Vue на `:5173` и API на `:5000` для браузера - разные origin (другой порт). Сервер разрешает читать ответы только `http://localhost:5173` и с cookie (`AllowCredentials`), не `*`: с `*` браузер не прислал бы cookie входа. Проверено preflight-запросом: для `http://localhost:5173` приходит `Access-Control-Allow-Origin: http://localhost:5173` и `Access-Control-Allow-Credentials: true`, для чужого origin разрешения нет.

## Один id через экран, Network и базу

Прогон 24.10.2026 на чистой базе (`dotnet ef database update`), в браузере.

```text
id = 1 · number = БР-0001
```

### 1. Администратор создаёт запись на экране

Строка в списке после «Создать»:

```text
1 | БР-0001 | Персональная тренировка, силовая | Тренажёрный зал | - | Клиент новичок, нужна программа на 4 недели | Новая
```

Network:

```text
POST /api/bookings 201
{"id":1,"number":"БР-0001","title":"Персональная тренировка, силовая","hallId":1,"hallName":"Тренажёрный зал",…,"status":"New",…,"assigneeUserId":null,…}
```

### 2. Администратор назначает тренера

Выбор «Илья Соколов» → «Назначить»: «Запись БР-0001: назначен тренер Илья Соколов», статус «Новая».

```text
PATCH /api/bookings/1/assignee 200
```

### 3. Тренер переводит в работу

Вход `trainer2`: в списке записей нет; запрос на смену статуса записи 1 от его имени:

```text
PATCH /api/bookings/1/status 403  «Менять статус может только тренер, назначенный на эту запись»
```

Вход `trainer`: в списке одна запись, «Подтвердить»:

```text
1 | БР-0001 | … | Илья Соколов | … | Подтверждена тренером | Провести, Отменить
PATCH /api/bookings/1/status 200
```

### 4. Та же строка в PostgreSQL

```sql
SELECT id, number, title, status, assignee_user_id FROM bookings WHERE id = 1;
```

```text
 id | number  |              title               |   status   | assignee_user_id
----+---------+----------------------------------+------------+------------------
  1 | БР-0001 | Персональная тренировка, силовая | InProgress |                2
(1 row)
```

### 5. Рестарт API

Остановил API, снова `dotnet run`, обновил страницу:

```text
1 | БР-0001 | Персональная тренировка, силовая | Тренажёрный зал | Илья Соколов | … | Подтверждена тренером
```

После выхода `POST /api/bookings` → 401.

Все запросы прогона на порт 5000 по порядку:

```text
GET   /api/auth/me              401   ← открыли /login до входа
POST  /api/auth/login           200   ← admin
GET   /api/users/trainers       200
GET   /api/bookings             200   ← пусто
POST  /api/bookings             201   ← id 1, БР-0001
GET   /api/users/trainers       200
GET   /api/bookings             200
PATCH /api/bookings/1/assignee  200
POST  /api/auth/logout          200
POST  /api/auth/login           200   ← trainer2
GET   /api/bookings             200   ← пусто: чужие записи не показываются
PATCH /api/bookings/1/status    403
POST  /api/auth/logout          200
POST  /api/auth/login           200   ← trainer
GET   /api/bookings             200
PATCH /api/bookings/1/status    200   ← InProgress
GET   /api/bookings             200   ← после рестарта API
POST  /api/auth/logout          200
```

Таблица по трём критериям - `docs/acceptance-check.md`: все три выполняются.

## Коллекция

`docs/api-collection/bookings.postman_collection.json` после изменений: 24 запроса, 34 проверки, 0 провалов.

## Проверка README

README проверен прогоном «с нуля» по его же командам: пустая база → `dotnet ef database update` → `dotnet run` → `npm run dev` → сценарий из раздела «Проверочный сценарий». Молчаливый запуск соседом на другом компьютере - на паре.

## Дефекты

Записаны в `docs/known-defects.md`. Открыты: зал вводится номером, а не выбором (D-01), mock умеет только список и создание (D-02), миграция не идёт поверх таблиц из скриптов ЛР9 - закрыто инструкцией в README (D-03), нет страниц и фильтров (D-04), нет сводки по статусам на экране (D-05). Закрыты с повторной проверкой: мигание формы входа после F5 (D-06), разъезжалась граница строки в колонке «Действия» (D-07).

## Чеклист

- [x] БД запущена, миграции применены, API видит таблицы
- [x] API на 5000, Swagger отвечает
- [x] `VITE_USE_MOCK=false`, запросы идут на API
- [x] CORS для `http://localhost:5173`, не `*`
- [x] создание и назначение через экран, записан один id
- [x] второй вход и `InProgress` - права и статус работают
- [x] id найден в Network и SQL
- [x] после рестарта API запись на месте
- [x] README, `docs/acceptance-check.md`, `docs/known-defects.md`
- [x] `.env`, `node_modules`, `bin`, `obj`, дампы не в Git

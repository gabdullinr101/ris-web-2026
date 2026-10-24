# ЛР9. PostgreSQL и SQL

Записи на тренировку переезжают в настоящую базу. В папке `db` три скрипта: схема, учебные данные и запросы. Сервер к базе сегодня не подключается - это ЛР10. Таблицы и связи - те же, что в модели данных ЛР2 (docs/data-model.md).

## Запуск

Учебная база - `fitness_bookings` (не служебная `postgres`). Пароль `postgres` вводится при запуске команды и в Git не попадает.

```bash
createdb -U postgres fitness_bookings
psql -U postgres -d fitness_bookings -f db/schema.sql
psql -U postgres -d fitness_bookings -f db/seed.sql
psql -U postgres -d fitness_bookings -f db/queries.sql
```

В pgAdmin то же самое: база `fitness_bookings` → Query Tool → по очереди `schema.sql`, `seed.sql`, `queries.sql`.

Проверено на PostgreSQL 16.

## Схема

| Таблица | Что хранит | Ключи и ограничения |
|---------|------------|---------------------|
| `halls` | залы клуба (справочник) | PK `id`, `name` уникален |
| `users` | администратор, тренеры, управляющий | PK `id`, `login` уникален, CHECK `role IN ('Admin','Trainer','Manager')` |
| `bookings` | записи на тренировку | PK `id`; `number` уникален; FK `hall_id → halls`, `created_by_user_id → users`, `assignee_user_id → users`; CHECK статуса и длин `title`, `description` |

Столбцы `bookings`: `id`, `number`, `title`, `description`, `hall_id`, `status`, `created_at` (`timestamptz`), `created_by_user_id`, `assignee_user_id`. У новой записи тренера нет - `assignee_user_id` равен `NULL`, а не 0.

Название зала хранится только в `halls`, в записи - ссылка `hall_id` (3НФ, как в ЛР2). Переименовали зал - меняется одна строка.

Статус ограничен четырьмя кодами:

```sql
status text NOT NULL DEFAULT 'New'
    CHECK (status IN ('New', 'InProgress', 'Closed', 'Cancelled'))
```

`password_hash` в `users` пока пустой: hash посчитает сервер в ЛР11. Открытый пароль в базе не хранится.

### Соответствие с JSON из ЛР8

| JSON (API) | Столбец (SQL) |
|------------|---------------|
| `id` | `id` |
| `number` | `number` |
| `title` | `title` |
| `description` | `description` |
| `hallId` | `hall_id` |
| `status` | `status` |
| `createdAt` | `created_at` |
| `createdByUserId` | `created_by_user_id` |
| `assigneeUserId` | `assignee_user_id` |

### Индексы

| Индекс | Зачем |
|--------|-------|
| `bookings_status_idx (status)` | фильтр списка по статусу и сводка `GROUP BY status` |
| `bookings_hall_id_idx (hall_id)` | соединение с залами и выборка записей одного зала |
| `bookings_assignee_user_id_idx (assignee_user_id)` | тренер видит только свои записи (ФТ-5 из ЛР1) |

По описанию индекса нет: по нему не ищут, а индекс замедлял бы каждую вставку.

### Повторный запуск

В начале `schema.sql` удаляются только свои таблицы (`DROP TABLE IF EXISTS bookings, users, halls CASCADE`), поэтому `schema.sql` и `seed.sql` можно прогнать сколько угодно раз - проверено: второй прогон проходит без ошибок, в базе снова 8 записей. Это учебное пересоздание схемы, а не команда приложения: в API `DELETE` по-прежнему нет.

## Учебные данные

- 3 зала: тренажёрный, единоборств, групповых программ;
- 4 пользователя: администратор, два тренера, управляющий;
- 8 записей - по две в каждом статусе. Записи `InProgress` и `Closed` всегда с назначенным тренером.

## Результаты запросов

Вывод `psql -f db/queries.sql`, время по Москве.

### 1. Фильтр и сортировка - новые записи, свежие сверху

```sql
SELECT number, title, status, created_at
FROM bookings
WHERE status = 'New'
ORDER BY created_at DESC;
```

```text
 number  |              title               | status |       created_at       
---------+----------------------------------+--------+------------------------
 БР-0002 | Персональная тренировка, бокс    | New    | 2026-10-23 12:40:00+03
 БР-0001 | Персональная тренировка, силовая | New    | 2026-10-22 10:15:00+03
(2 rows)
```

### 2. JOIN - название зала и имя тренера вместо номеров

```sql
SELECT b.id, b.number, b.title, h.name AS hall_name, u.full_name AS trainer, b.status
FROM bookings AS b
JOIN halls AS h ON h.id = b.hall_id
LEFT JOIN users AS u ON u.id = b.assignee_user_id
ORDER BY b.created_at DESC;
```

`LEFT JOIN` к пользователям - потому что у части записей тренера нет; обычный `JOIN` такие записи потерял бы.

```text
 id | number  |              title               |       hall_name        |     trainer     |   status   
----+---------+----------------------------------+------------------------+-----------------+------------
  2 | БР-0002 | Персональная тренировка, бокс    | Зал единоборств        | Марат Хайруллин | New
  4 | БР-0004 | Функциональный тренинг           | Зал групповых программ | Марат Хайруллин | InProgress
  1 | БР-0001 | Персональная тренировка, силовая | Тренажёрный зал        |                 | New
  3 | БР-0003 | Групповое занятие, йога          | Зал групповых программ | Илья Соколов    | InProgress
  8 | БР-0008 | Групповое занятие, пилатес       | Зал групповых программ |                 | Cancelled
  7 | БР-0007 | Персональная тренировка, бокс    | Зал единоборств        | Марат Хайруллин | Cancelled
  6 | БР-0006 | Растяжка после травмы колена     | Зал групповых программ | Марат Хайруллин | Closed
  5 | БР-0005 | Персональная тренировка, кардио  | Тренажёрный зал        | Илья Соколов    | Closed
(8 rows)
```

### 3. GROUP BY - сколько записей в каждом статусе (сводка управляющего)

```sql
SELECT status, COUNT(*) AS count
FROM bookings
GROUP BY status
ORDER BY status;
```

```text
   status   | count 
------------+-------
 Cancelled  |     2
 Closed     |     2
 InProgress |     2
 New        |     2
(4 rows)
```

### 4. GROUP BY с JOIN - нагрузка тренеров и отмены

```sql
SELECT u.full_name AS trainer,
       COUNT(*) AS total,
       COUNT(*) FILTER (WHERE b.status = 'Cancelled') AS cancelled
FROM bookings AS b
JOIN users AS u ON u.id = b.assignee_user_id
GROUP BY u.full_name
ORDER BY total DESC, trainer;
```

```text
     trainer     | total | cancelled 
-----------------+-------+-----------
 Марат Хайруллин |     4 |         1
 Илья Соколов    |     2 |         0
(2 rows)
```

### 5. Записи одного тренера

```sql
SELECT b.number, b.title, b.status
FROM bookings AS b
WHERE b.assignee_user_id = 2
ORDER BY b.created_at DESC;
```

```text
 number  |              title              |   status   
---------+---------------------------------+------------
 БР-0003 | Групповое занятие, йога         | InProgress
 БР-0005 | Персональная тренировка, кардио | Closed
(2 rows)
```

## Проверка ограничений

Три вставки в конце `queries.sql` должны завершиться ошибкой. Каждая обёрнута в `BEGIN … ROLLBACK`, в рабочем `seed.sql` их нет.

| Что вставляем | Ожидание | Факт |
|---------------|----------|------|
| статус `Done` | отказ по CHECK | `violates check constraint "bookings_status_check"` |
| зал `99` | отказ по внешнему ключу | `violates foreign key constraint "bookings_hall_id_fkey"`, `Key (hall_id)=(99) is not present in table "halls"` |
| вид тренировки `Сил` | отказ по CHECK длины | `violates check constraint "bookings_title_check"` |

```text
BEGIN
psql:db/queries.sql:49: ERROR:  new row for relation "bookings" violates check constraint "bookings_status_check"
DETAIL:  Failing row contains (9, БР-9001, Проверка статуса, Статус не из списка четырёх, 1, Done, 2026-10-24 13:37:11.57628+03, 1, null).
ROLLBACK
BEGIN
psql:db/queries.sql:55: ERROR:  insert or update on table "bookings" violates foreign key constraint "bookings_hall_id_fkey"
DETAIL:  Key (hall_id)=(99) is not present in table "halls".
ROLLBACK
BEGIN
psql:db/queries.sql:61: ERROR:  new row for relation "bookings" violates check constraint "bookings_title_check"
DETAIL:  Failing row contains (11, БР-9003, Сил, Слишком короткий вид тренировки, 1, New, 2026-10-24 13:37:11.577312+03, 1, null).
ROLLBACK
```

Неудачные вставки всё равно забирают номера из счётчика `id` (9, 10, 11) - так работает PostgreSQL, следующая запись получит `id` 12. На данные это не влияет.

## Сохранность

После перезапуска службы PostgreSQL запись на месте:

```text
 id | number  |   status
----+---------+------------
  3 | БР-0003 | InProgress
(1 row)
```

## Чеклист

- [x] учебная база `fitness_bookings`, не служебная `postgres`
- [x] `schema.sql`: PK, FK, CHECK статусов, индексы по статусу и залу с комментариями
- [x] схема прогоняется повторно
- [x] `seed.sql`: 4 пользователя, 3 зала, все четыре статуса
- [x] `queries.sql`: фильтр, сортировка, JOIN с названием зала, GROUP BY с COUNT
- [x] вставка статуса `Done` отклонена CHECK
- [x] имена в `snake_case`, совпадают с полями JSON из ЛР8
- [x] паролей в `db` и README нет

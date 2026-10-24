-- Запросы к учебной базе (ЛР9). Запускать после schema.sql и seed.sql:
-- psql -U postgres -d fitness_bookings -f db/queries.sql

-- Время показываем по Москве, как в клубе
SET timezone = 'Europe/Moscow';

-- 1. Фильтр и сортировка: новые записи, свежие сверху.
--    Так администратор видит, кому ещё не назначен или не подтвердил тренер.
SELECT number, title, status, created_at
FROM bookings
WHERE status = 'New'
ORDER BY created_at DESC;

-- 2. JOIN: записи с названием зала и именем тренера, а не только с номерами.
--    LEFT JOIN к users - потому что тренера может не быть (assignee_user_id IS NULL).
SELECT b.id, b.number, b.title, h.name AS hall_name, u.full_name AS trainer, b.status
FROM bookings AS b
JOIN halls AS h ON h.id = b.hall_id
LEFT JOIN users AS u ON u.id = b.assignee_user_id
ORDER BY b.created_at DESC;

-- 3. GROUP BY: сколько записей в каждом статусе. Сводка для управляющего клуба.
SELECT status, COUNT(*) AS count
FROM bookings
GROUP BY status
ORDER BY status;

-- 4. GROUP BY с JOIN: сколько записей у каждого тренера и сколько из них отменено.
SELECT u.full_name AS trainer,
       COUNT(*) AS total,
       COUNT(*) FILTER (WHERE b.status = 'Cancelled') AS cancelled
FROM bookings AS b
JOIN users AS u ON u.id = b.assignee_user_id
GROUP BY u.full_name
ORDER BY total DESC, trainer;

-- 5. Записи одного тренера (то, что увидит тренер «Илья Соколов» в ЛР11).
SELECT b.number, b.title, b.status
FROM bookings AS b
WHERE b.assignee_user_id = 2
ORDER BY b.created_at DESC;

-- 6. Проверка ограничений. Каждая вставка ниже ДОЛЖНА завершиться ошибкой.
--    Они в транзакции с ROLLBACK, поэтому в базе ничего не остаётся.

-- 6.1. Статус Done не входит в четыре разрешённых - нарушение CHECK bookings_status_check.
BEGIN;
INSERT INTO bookings (number, title, description, hall_id, status, created_by_user_id)
VALUES ('БР-9001', 'Проверка статуса', 'Статус не из списка четырёх', 1, 'Done', 1);
ROLLBACK;

-- 6.2. Зала 99 нет - нарушение внешнего ключа bookings_hall_id_fkey.
BEGIN;
INSERT INTO bookings (number, title, description, hall_id, created_by_user_id)
VALUES ('БР-9002', 'Проверка зала', 'Ссылка на несуществующий зал', 99, 1);
ROLLBACK;

-- 6.3. Вид тренировки из трёх символов - нарушение CHECK bookings_title_check.
BEGIN;
INSERT INTO bookings (number, title, description, hall_id, created_by_user_id)
VALUES ('БР-9003', 'Сил', 'Слишком короткий вид тренировки', 1, 1);
ROLLBACK;

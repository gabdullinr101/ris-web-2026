# API-контракт

Адрес сервера в разработке: http://localhost:5000. Формат тел: JSON. Статусы: New, InProgress, Closed, Cancelled.

| Метод и путь                      | Тело                       | Успех                        | Возможная ошибка        |
| --------------------------------- | -------------------------- | ---------------------------- | ----------------------- |
| POST /api/auth/login              | login, password            | 200, пользователь и роль; cookie входа | 401           |
| POST /api/auth/logout             | -                          | 200, cookie удалена          | -                       |
| GET /api/auth/me                  | -                          | 200, кто вошёл               | 401                     |
| GET /api/users/trainers           | -                          | 200, список тренеров         | 401, 403                |
| GET /api/bookings                 | -                          | 200, массив или []           | 401                     |
| GET /api/bookings/{id}            | -                          | 200, объект                  | 401, 403, 404           |
| POST /api/bookings                | title, hallId, description | 201, id, number, status: New | 400, 401, 403           |
| PATCH /api/bookings/{id}/assignee | assigneeUserId             | 200, объект                  | 400, 401, 403, 404, 409 |
| PATCH /api/bookings/{id}/status   | status                     | 200, объект                  | 400, 401, 403, 404, 409 |

Вход - через cookie `fitness_auth` (ЛР11): после `POST /api/auth/login` браузер сам присылает её с каждым запросом. Без входа все адреса `/api/bookings` отвечают 401.

Правила:
- При создании клиент не присылает id, number, status и тренера. Их определяет сервер.
- Пустой список возвращается как 200 и []. Код 404 только для несуществующего id.
- POST создаёт только администратор (иначе 403). Назначает тренера администратор или управляющий (иначе 403).
- Статус меняет только назначенный тренер своей записи (чужая запись - 403). Недопустимый переход (например, Closed → New) - 409.
- DELETE не используется: отмена - это статус Cancelled.

## Примеры

POST /api/bookings

```
{
  "title": "Персональная тренировка, силовая",
  "hallId": 2,
  "description": "Клиент новичок, нужна программа на 4 недели"
}
```

Ответ 201:

```
{
  "id": 17,
  "number": "БР-0017",
  "title": "Персональная тренировка, силовая",
  "hallId": 2,
  "description": "Клиент новичок, нужна программа на 4 недели",
  "status": "New",
  "createdByUserId": 1,
  "assigneeUserId": null
}
```

Ответ 400 (не прошла проверка полей):

```
{ "errors": { "title": "Длина от 5 до 80 символов" } }
```

PATCH /api/bookings/17/assignee

```
{ "assigneeUserId": 5 }
```

Ответ 200: объект записи с assigneeUserId: 5, статус New.

PATCH /api/bookings/17/status

```
{ "status": "InProgress" }
```

Ответ 200: объект записи со статусом InProgress. Ответ 409:

```
{ "error": "Переход Closed → New недопустим" }
```
# Матрица требований

| Требование ЛР1                             | Поле или сущность                   | Запрос                                | Критерий |
| ------------------------------------------ | ----------------------------------- | ------------------------------------- | -------- |
| Администратор создаёт запись на тренировку | Booking: title, hallId, description | POST /api/bookings                    | 1        |
| Администратор назначает тренера            | Booking.assigneeUserId              | PATCH /api/bookings/{id}/assignee     | 2        |
| Тренер переводит свою запись в работу      | Booking.status                      | PATCH /api/bookings/{id}/status       | 3        |
| Запрет: тренер не двигает чужую запись     | Booking.assigneeUserId, status      | PATCH /api/bookings/{id}/status → 403 | 3        |
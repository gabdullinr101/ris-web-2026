// Коды статусов и то, как они называются на экране (таблица соответствия из ЛР1)
export const statusNames = {
  New: 'Новая',
  InProgress: 'Подтверждена тренером',
  Closed: 'Проведена',
  Cancelled: 'Отменена',
}

// Какие кнопки показать тренеру на его записи. Сервер всё равно проверит переход сам (409).
export const trainerActions = {
  New: [
    { status: 'InProgress', label: 'Подтвердить' },
    { status: 'Cancelled', label: 'Отменить' },
  ],
  InProgress: [
    { status: 'Closed', label: 'Провести' },
    { status: 'Cancelled', label: 'Отменить' },
  ],
  Closed: [],
  Cancelled: [],
}

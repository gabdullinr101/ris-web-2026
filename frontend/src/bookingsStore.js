import { ref } from 'vue'

// Единственный общий массив записей на тренировку.
// Сервера пока нет, поэтому данные тестовые и после перезагрузки страницы возвращаются к этим.
// Имена полей как в docs/api-contract.md: number, title, hallId, status.
export const bookings = ref([
  { id: 1, number: 'БР-0001', title: 'Персональная тренировка, силовая', hallId: 2, status: 'New' },
  { id: 2, number: 'БР-0002', title: 'Групповое занятие, йога', hallId: 1, status: 'InProgress' },
  { id: 3, number: 'БР-0003', title: 'Функциональный тренинг', hallId: 3, status: 'Closed' },
  { id: 4, number: 'БР-0004', title: 'Персональная тренировка, бокс', hallId: 2, status: 'Cancelled' },
])

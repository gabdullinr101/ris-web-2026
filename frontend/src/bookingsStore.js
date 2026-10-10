import { ref } from 'vue'

// Единственный общий массив записей на тренировку.
// Сервера пока нет: данные живут в памяти вкладки и после перезагрузки (F5) возвращаются к тестовым.
// Имена полей как в docs/api-contract.md.
export const bookings = ref([
  { id: 1, number: 'БР-0001', title: 'Персональная тренировка, силовая', hallId: 2, description: 'Клиент новичок, нужна программа на 4 недели', status: 'New' },
  { id: 2, number: 'БР-0002', title: 'Групповое занятие, йога', hallId: 1, description: 'Группа до 10 человек, коврики свои', status: 'InProgress' },
  { id: 3, number: 'БР-0003', title: 'Функциональный тренинг', hallId: 3, description: 'Круговая тренировка на 45 минут', status: 'Closed' },
  { id: 4, number: 'БР-0004', title: 'Персональная тренировка, бокс', hallId: 2, description: 'Клиент отменил запись по телефону', status: 'Cancelled' },
])

// Учебный счётчик: id следующей записи. В ЛР6 id и номер будет выдавать сервер.
let nextId = 5

// Добавляет запись в общий массив. Статус всегда New, из формы он не берётся.
export function addBooking({ title, hallId, description }) {
  const booking = {
    id: nextId,
    number: 'TMP-' + nextId,
    title,
    hallId,
    description,
    status: 'New',
  }
  nextId = nextId + 1
  bookings.value.push(booking)
  return booking
}

import { mockGetBookings, mockCreateBooking } from './mockBookings'
import { send, unwrap } from './http'

export { errorMessage } from './http'

// Рабочий режим - настоящий сервер. Учебный mock из ЛР6 включается в .env: VITE_USE_MOCK=true.
// После правки .env перезапустить npm run dev.
const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true'

// Список записей
export async function getBookings() {
  const response = USE_MOCK ? await mockGetBookings() : await send('GET', '/api/bookings')
  return unwrap(response)
}

// Создание записи. body: { title, hallId, description }. Возвращает созданную запись.
export async function createBooking(body) {
  const response = USE_MOCK
    ? await mockCreateBooking(body)
    : await send('POST', '/api/bookings', body)
  return unwrap(response)
}

// Назначение и смена статуса - только на сервере (mock их не умеет).

// PATCH /api/bookings/{id}/assignee - назначить тренера, статус не меняется
export async function assignBooking(id, assigneeUserId) {
  return unwrap(await send('PATCH', `/api/bookings/${id}/assignee`, { assigneeUserId }))
}

// PATCH /api/bookings/{id}/status - сменить статус, тренер не меняется
export async function changeBookingStatus(id, status) {
  return unwrap(await send('PATCH', `/api/bookings/${id}/status`, { status }))
}

import { mockGetBookings, mockCreateBooking } from './mockBookings'
import { send, unwrap } from './http'

export { errorMessage } from './http'

// true - отвечает mock (без HTTP), false - настоящий сервер.
// Задаётся в .env: VITE_USE_MOCK=true или false. После правки .env перезапустить npm run dev.
const USE_MOCK = import.meta.env.VITE_USE_MOCK !== 'false'

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

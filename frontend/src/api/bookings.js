import { mockGetBookings, mockCreateBooking } from './mockBookings'

// true - отвечает mock (без HTTP), false - настоящий запрос fetch к серверу
const USE_MOCK = true

// Адрес сервера задан только здесь, берётся из файла .env
const baseUrl = import.meta.env.VITE_API_BASE_URL

// Настоящий HTTP-запрос. Возвращает ответ в том же виде, что и mock: { ok, status, data }.
// Если сервер недоступен, fetch сам выбросит ошибку - она уйдёт в catch на странице.
async function send(method, path, body) {
  const options = { method }
  if (body !== undefined) {
    options.headers = { 'Content-Type': 'application/json' }
    options.body = JSON.stringify(body)
  }

  const response = await fetch(`${baseUrl}${path}`, options)

  let data = null
  try {
    data = await response.json()
  } catch {
    // у ответа может не быть тела в формате JSON
  }

  return { ok: response.ok, status: response.status, data }
}

// Ответы 400 и 500 - это тоже ответы, в catch они сами не попадают.
// Поэтому проверяем ok и выбрасываем ошибку с HTTP-статусом.
function unwrap(response) {
  if (!response.ok) {
    const error = new Error('HTTP ' + response.status)
    error.status = response.status
    error.data = response.data
    throw error
  }
  return response.data
}

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

// Понятный человеку текст для каждого вида отказа
export function errorMessage(error) {
  if (error.status === 400) {
    return 'Проверьте поля'
  }
  if (error.status) {
    return 'Сервер не смог обработать запрос'
  }
  // статуса нет - ответа не было вообще
  return 'Нет связи с сервером'
}

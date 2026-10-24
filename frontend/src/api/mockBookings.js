// Учебная замена сервера (mock). Настоящего HTTP-запроса здесь нет.
// Отвечает так же, как будет отвечать сервер из docs/api-contract.md:
// объект { ok, status, data }.

// Учебные переключатели для проверки отказов. Перед сдачей оба false.
export const mockOptions = {
  force500: false, // true - любой запрос отвечает 500
  emptyList: false, // true - список приходит пустым
}

// Справочник залов: зал с другим номером «сервер» не примет
const hallIds = [1, 2, 3]

// «Таблица в базе». Живёт в памяти вкладки, после F5 возвращается к этим строкам.
const rows = [
  { id: 1, number: 'БР-0001', title: 'Персональная тренировка, силовая', hallId: 2, description: 'Клиент новичок, нужна программа на 4 недели', status: 'New' },
  { id: 2, number: 'БР-0002', title: 'Групповое занятие, йога', hallId: 1, description: 'Группа до 10 человек, коврики свои', status: 'InProgress' },
  { id: 3, number: 'БР-0003', title: 'Функциональный тренинг', hallId: 3, description: 'Круговая тренировка на 45 минут', status: 'Closed' },
  { id: 4, number: 'БР-0004', title: 'Персональная тренировка, бокс', hallId: 2, description: 'Клиент отменил запись по телефону', status: 'Cancelled' },
]

let nextId = 5

// Задержка, чтобы успеть увидеть «Загрузка…» и заблокированную кнопку
function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

// GET /api/bookings
export async function mockGetBookings() {
  await delay(500)

  if (mockOptions.force500) {
    return { ok: false, status: 500, data: { error: 'Внутренняя ошибка сервера' } }
  }
  if (mockOptions.emptyList) {
    return { ok: true, status: 200, data: [] }
  }
  // Отдаём копии, чтобы страница не могла поменять «базу» напрямую
  return { ok: true, status: 200, data: rows.map((row) => ({ ...row })) }
}

// POST /api/bookings
export async function mockCreateBooking(body) {
  await delay(500)

  if (mockOptions.force500) {
    return { ok: false, status: 500, data: { error: 'Внутренняя ошибка сервера' } }
  }

  // Сервер проверяет поля сам, даже если форма их уже проверила
  const title = String(body.title ?? '').trim()
  const description = String(body.description ?? '').trim()
  const errors = {}

  if (title.length < 5 || title.length > 80) {
    errors.title = 'Длина от 5 до 80 символов'
  }
  if (!hallIds.includes(body.hallId)) {
    errors.hallId = 'Зал с таким номером не найден'
  }
  if (description.length < 10 || description.length > 500) {
    errors.description = 'Длина от 10 до 500 символов'
  }
  if (Object.keys(errors).length > 0) {
    return { ok: false, status: 400, data: { errors } }
  }

  // id, number и status назначает «сервер», клиент их не присылает
  const row = {
    id: nextId,
    number: 'БР-' + String(nextId).padStart(4, '0'),
    title,
    hallId: body.hallId,
    description,
    status: 'New',
  }
  nextId = nextId + 1
  rows.push(row)

  return { ok: true, status: 201, data: { ...row } }
}

// Общий HTTP-клиент для всех запросов к серверу.
// Адрес сервера задан только здесь, берётся из файла .env.
const baseUrl = import.meta.env.VITE_API_BASE_URL

// Настоящий HTTP-запрос. Возвращает ответ в виде { ok, status, data } - так же отвечает и mock.
// credentials: 'include' - браузер прикладывает cookie входа (сервер на другом порту).
// Если сервер недоступен, fetch сам выбросит ошибку - она уйдёт в catch на странице.
export async function send(method, path, body) {
  const options = { method, credentials: 'include' }
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

// Ответы 400, 401, 403, 409, 500 - это тоже ответы, в catch они сами не попадают.
// Поэтому проверяем ok и выбрасываем ошибку с HTTP-статусом.
export function unwrap(response) {
  if (!response.ok) {
    const error = new Error('HTTP ' + response.status)
    error.status = response.status
    error.data = response.data
    throw error
  }
  return response.data
}

// Понятный человеку текст для каждого вида отказа
export function errorMessage(error) {
  if (error.status === 400) {
    return 'Проверьте поля'
  }
  if (error.status === 401) {
    return 'Войдите в систему'
  }
  if (error.status === 403) {
    return error.data?.detail || 'Недостаточно прав'
  }
  if (error.status === 404) {
    return 'Запись не найдена'
  }
  if (error.status === 409) {
    return error.data?.detail || 'Действие сейчас недоступно'
  }
  if (error.status) {
    return 'Сервер не смог обработать запрос'
  }
  // статуса нет - ответа не было вообще
  return 'Нет связи с сервером'
}

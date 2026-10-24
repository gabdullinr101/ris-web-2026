import { send, unwrap } from './http'

// Вход всегда идёт на настоящий сервер: mock не умеет проверять пароль.
// Сервер в ответ ставит cookie fitness_auth (HttpOnly - JavaScript её не видит).

// POST /api/auth/login → { id, login, fullName, role }
export async function login(loginName, password) {
  return unwrap(await send('POST', '/api/auth/login', { login: loginName, password }))
}

// POST /api/auth/logout - сервер удаляет cookie
export async function logout() {
  return unwrap(await send('POST', '/api/auth/logout'))
}

// GET /api/auth/me - кто вошёл; без входа сервер ответит 401
export async function me() {
  return unwrap(await send('GET', '/api/auth/me'))
}

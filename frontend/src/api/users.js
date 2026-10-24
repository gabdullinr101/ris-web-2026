import { send, unwrap } from './http'

// GET /api/users/trainers - тренеры для выбора при назначении (администратор и управляющий)
export async function getTrainers() {
  return unwrap(await send('GET', '/api/users/trainers'))
}

import { ref } from 'vue'
import { me } from './api/auth'

// Кто вошёл: { id, login, fullName, role } или null.
// Это только для экрана (показать имя, спрятать лишние кнопки). Права всё равно проверяет сервер.
export const currentUser = ref(null)

export const roleNames = {
  Admin: 'администратор',
  Trainer: 'тренер',
  Manager: 'управляющий',
}

// Спросить у сервера, есть ли действующая cookie входа (после F5 вход не теряется)
export async function loadCurrentUser() {
  try {
    currentUser.value = await me()
  } catch {
    currentUser.value = null
  }
}

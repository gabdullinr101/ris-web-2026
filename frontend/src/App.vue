<script setup>
import { ref, watch, nextTick } from 'vue'

// Какой экран показан сейчас: 'list' | 'new' | 'login'
const screen = ref('list')

// Тестовые записи на тренировку. Сервера пока нет (появится в следующих ЛР).
// Имена полей как в docs/api-contract.md: number, title, hallId, status.
const items = ref([
  { id: 1, number: 'БР-0001', title: 'Персональная тренировка, силовая', hallId: 2, status: 'New' },
  { id: 2, number: 'БР-0002', title: 'Групповое занятие, йога', hallId: 1, status: 'InProgress' },
  { id: 3, number: 'БР-0003', title: 'Функциональный тренинг', hallId: 3, status: 'Closed' },
  { id: 4, number: 'БР-0004', title: 'Персональная тренировка, бокс', hallId: 2, status: 'Cancelled' },
])

// Ссылки на первые поля форм, чтобы ставить в них курсор
const titleInput = ref(null)
const loginInput = ref(null)

// При смене экрана ждём, пока Vue нарисует форму, и ставим фокус в первое поле
watch(screen, async () => {
  await nextTick()
  if (screen.value === 'new') titleInput.value?.focus()
  if (screen.value === 'login') loginInput.value?.focus()
})
</script>

<template>
  <div class="page">
    <header class="top">
      <h1>Бронирование тренировок</h1>
      <nav aria-label="Экраны">
        <button type="button" :class="{ active: screen === 'list' }" @click="screen = 'list'">
          Список
        </button>
        <button type="button" :class="{ active: screen === 'new' }" @click="screen = 'new'">
          Создать
        </button>
        <button type="button" :class="{ active: screen === 'login' }" @click="screen = 'login'">
          Вход
        </button>
      </nav>
    </header>

    <!-- Экран 1. Список записей -->
    <main v-if="screen === 'list'">
      <h2>Записи на тренировки</h2>
      <table>
        <thead>
          <tr>
            <th scope="col">Номер</th>
            <th scope="col">Вид тренировки (title)</th>
            <th scope="col">Зал (hallId)</th>
            <th scope="col">Статус</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td>{{ item.number }}</td>
            <td>{{ item.title }}</td>
            <td>{{ item.hallId }}</td>
            <td>{{ item.status }}</td>
          </tr>
        </tbody>
      </table>
    </main>

    <!-- Экран 2. Создание записи -->
    <main v-else-if="screen === 'new'">
      <h2>Новая запись на тренировку</h2>
      <form @submit.prevent>
        <label for="title">Вид тренировки, 5–80 символов</label>
        <input id="title" ref="titleInput" name="title" type="text" autofocus />

        <label for="hallId">Номер зала (hallId), число больше 0</label>
        <input id="hallId" name="hallId" type="number" min="1" />

        <label for="description">Описание, 10–500 символов</label>
        <textarea id="description" name="description" rows="4"></textarea>

        <button type="submit">Создать</button>
      </form>
    </main>

    <!-- Экран 3. Вход -->
    <main v-else>
      <h2>Вход в систему</h2>
      <form @submit.prevent>
        <label for="login">Логин</label>
        <input id="login" ref="loginInput" name="login" type="text" autocomplete="username" />

        <label for="password">Пароль</label>
        <input id="password" name="password" type="password" autocomplete="current-password" />

        <button type="submit">Войти</button>
      </form>
    </main>
  </div>
</template>

<style scoped>
.page {
  max-width: 860px;
  margin: 0 auto;
  padding: 24px 16px 48px;
}

.top {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 24px;
}

h1 {
  margin: 0;
  font-size: 22px;
}

h2 {
  margin: 0 0 16px;
  font-size: 18px;
}

nav {
  display: flex;
  gap: 8px;
}

button {
  padding: 8px 16px;
  font: inherit;
  color: #1c2530;
  background: #ffffff;
  border: 1px solid #b6c0cc;
  border-radius: 6px;
  cursor: pointer;
}

button:hover {
  border-color: #1f5fbf;
}

button.active,
form button {
  color: #ffffff;
  background: #1f5fbf;
  border-color: #1f5fbf;
}

main {
  padding: 20px;
  background: #ffffff;
  border: 1px solid #d8dee6;
  border-radius: 8px;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid #e3e8ee;
}

th {
  font-size: 14px;
  color: #51606f;
}

form {
  display: flex;
  flex-direction: column;
  max-width: 420px;
}

label {
  margin-bottom: 4px;
  font-size: 14px;
  color: #51606f;
}

input,
textarea {
  margin-bottom: 16px;
  padding: 8px 10px;
  font: inherit;
  color: inherit;
  background: #ffffff;
  border: 1px solid #b6c0cc;
  border-radius: 6px;
}

input:focus,
textarea:focus,
button:focus-visible {
  outline: 2px solid #1f5fbf;
  outline-offset: 1px;
}

form button {
  align-self: flex-start;
}
</style>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { addBooking } from '../bookingsStore'

const router = useRouter()

// Состояние формы. Имена полей как в docs/api-contract.md.
const form = ref({
  title: '',
  hallId: 0,
  description: '',
})

// Тексты ошибок по полям. Пустая строка — ошибки нет.
const errors = ref({
  title: '',
  hallId: '',
  description: '',
})

// Ссылка на первое поле, чтобы поставить в него курсор при открытии страницы
const titleInput = ref(null)

onMounted(() => {
  titleInput.value?.focus()
})

function onCreate() {
  // trim обрезает пробелы по краям, поэтому строка из одних пробелов не пройдёт
  const title = form.value.title.trim()
  const hallId = form.value.hallId
  const description = form.value.description.trim()

  errors.value = { title: '', hallId: '', description: '' }

  if (title.length < 5 || title.length > 80) {
    errors.value.title = 'Вид тренировки: от 5 до 80 символов'
  }
  if (!Number.isInteger(hallId) || hallId <= 0) {
    errors.value.hallId = 'Зал: целое число больше 0'
  }
  if (description.length < 10 || description.length > 500) {
    errors.value.description = 'Описание: от 10 до 500 символов'
  }

  // Если есть хотя бы одна ошибка, выходим до добавления записи
  if (errors.value.title || errors.value.hallId || errors.value.description) {
    return
  }

  addBooking({ title, hallId, description })

  form.value = { title: '', hallId: 0, description: '' }
  router.push('/')
}
</script>

<template>
  <h1>Новая запись на тренировку</h1>
  <form @submit.prevent="onCreate">
    <label for="title">Вид тренировки, 5–80 символов</label>
    <input id="title" ref="titleInput" v-model="form.title" name="title" type="text" autofocus />
    <p v-if="errors.title" class="error">{{ errors.title }}</p>

    <label for="hallId">Номер зала (hallId), число больше 0</label>
    <input id="hallId" v-model.number="form.hallId" name="hallId" type="number" />
    <p v-if="errors.hallId" class="error">{{ errors.hallId }}</p>

    <label for="description">Описание, 10–500 символов</label>
    <textarea id="description" v-model="form.description" name="description" rows="4"></textarea>
    <p v-if="errors.description" class="error">{{ errors.description }}</p>

    <button type="submit">Создать</button>
  </form>
</template>

<style scoped>
.error {
  margin: -10px 0 16px;
  font-size: 14px;
  color: #b3261e;
}
</style>

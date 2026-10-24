<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { createBooking, errorMessage } from '../api/bookings'

const router = useRouter()

// Состояние формы. Имена полей как в docs/api-contract.md.
const form = ref({
  title: '',
  hallId: 0,
  description: '',
})

// Тексты ошибок по полям. Пустая строка - ошибки нет.
const errors = ref({
  title: '',
  hallId: '',
  description: '',
})

// Общее сообщение над кнопкой: «Проверьте поля», «Нет связи с сервером» и т. п.
const formError = ref('')

// true, пока запрос на создание не завершился
const isSubmitting = ref(false)

// Ссылка на первое поле, чтобы поставить в него курсор при открытии страницы
const titleInput = ref(null)

onMounted(() => {
  titleInput.value?.focus()
})

async function onCreate() {
  // Защита от двойной отправки: второй Enter или клик во время запроса ничего не делает
  if (isSubmitting.value) {
    return
  }

  // trim обрезает пробелы по краям, поэтому строка из одних пробелов не пройдёт
  const title = form.value.title.trim()
  const hallId = form.value.hallId
  const description = form.value.description.trim()

  errors.value = { title: '', hallId: '', description: '' }
  formError.value = ''

  if (title.length < 5 || title.length > 80) {
    errors.value.title = 'Вид тренировки: от 5 до 80 символов'
  }
  if (!Number.isInteger(hallId) || hallId <= 0) {
    errors.value.hallId = 'Зал: целое число больше 0'
  }
  if (description.length < 10 || description.length > 500) {
    errors.value.description = 'Описание: от 10 до 500 символов'
  }

  // Если есть хотя бы одна ошибка, выходим до запроса
  if (errors.value.title || errors.value.hallId || errors.value.description) {
    formError.value = 'Проверьте поля'
    return
  }

  isSubmitting.value = true
  try {
    // id, number и status приходят в ответе, форма их не придумывает
    await createBooking({ title, hallId, description })

    form.value = { title: '', hallId: 0, description: '' }
    // Список на странице / сам загрузит свежие данные вместе с новой записью
    router.push('/')
  } catch (error) {
    formError.value = errorMessage(error)
    // При ответе 400 сервер присылает ошибки по полям - показываем их под полями
    if (error.status === 400 && error.data?.errors) {
      errors.value = { ...errors.value, ...error.data.errors }
    }
    // Поля не очищаем: человек исправит и отправит ещё раз
  } finally {
    // Снимаем блокировку и после успеха, и после ошибки
    isSubmitting.value = false
  }
}
</script>

<template>
  <h1>Новая запись на тренировку</h1>
  <form @submit.prevent="onCreate">
    <label for="title">Вид тренировки, 5-80 символов</label>
    <input id="title" ref="titleInput" v-model="form.title" name="title" type="text" autofocus />
    <p v-if="errors.title" class="error">{{ errors.title }}</p>

    <label for="hallId">Номер зала (hallId), число больше 0</label>
    <input id="hallId" v-model.number="form.hallId" name="hallId" type="number" />
    <p v-if="errors.hallId" class="error">{{ errors.hallId }}</p>

    <label for="description">Описание, 10-500 символов</label>
    <textarea id="description" v-model="form.description" name="description" rows="4"></textarea>
    <p v-if="errors.description" class="error">{{ errors.description }}</p>

    <p v-if="formError" class="form-error" role="alert">{{ formError }}</p>

    <button type="submit" :disabled="isSubmitting">
      {{ isSubmitting ? 'Создаём…' : 'Создать' }}
    </button>
  </form>
</template>

<style scoped>
.error {
  margin: -10px 0 16px;
  font-size: 14px;
  color: #b3261e;
}

.form-error {
  margin: 0 0 16px;
  padding: 8px 12px;
  color: #8c1d18;
  background: #fdecea;
  border: 1px solid #f1b5b0;
  border-radius: 6px;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

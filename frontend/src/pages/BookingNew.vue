<script setup>
import { ref, onMounted } from 'vue'

// Ссылка на первое поле, чтобы поставить в него курсор при открытии страницы
const titleInput = ref(null)

onMounted(() => {
  titleInput.value?.focus()
})
</script>

<template>
  <h1>Новая запись на тренировку</h1>
  <!-- Кнопка пока ничего не сохраняет: проверка полей будет в ЛР5, запрос к серверу позже -->
  <form @submit.prevent>
    <label for="title">Вид тренировки, 5–80 символов</label>
    <input id="title" ref="titleInput" name="title" type="text" />

    <label for="hallId">Номер зала (hallId), число больше 0</label>
    <input id="hallId" name="hallId" type="number" min="1" />

    <label for="description">Описание, 10–500 символов</label>
    <textarea id="description" name="description" rows="4"></textarea>

    <button type="submit">Создать</button>
  </form>
</template>

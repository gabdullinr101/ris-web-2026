<script setup>
import { ref, onMounted } from 'vue'
import BookingTable from '../components/BookingTable.vue'
import { bookings } from '../bookingsStore'
import { getBookings, errorMessage } from '../api/bookings'

// Одно поле состояния экрана: 'loading' | 'empty' | 'data' | 'error'
const screen = ref('loading')
const errorText = ref('')

async function load() {
  screen.value = 'loading'
  try {
    bookings.value = await getBookings()
    screen.value = bookings.value.length === 0 ? 'empty' : 'data'
  } catch (error) {
    errorText.value = errorMessage(error)
    screen.value = 'error'
  }
}

onMounted(load)
</script>

<template>
  <h1>Записи на тренировки</h1>

  <p v-if="screen === 'loading'">Загрузка…</p>
  <p v-else-if="screen === 'empty'">Записей пока нет</p>
  <p v-else-if="screen === 'error'" class="error" role="alert">{{ errorText }}</p>
  <BookingTable v-else :items="bookings" />
</template>

<style scoped>
.error {
  color: #b3261e;
}
</style>

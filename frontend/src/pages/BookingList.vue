<script setup>
import BookingTable from '../components/BookingTable.vue'
import { bookings } from '../bookingsStore'
</script>

<template>
  <h1>Записи на тренировки</h1>
  <!-- Страница берёт массив из bookingsStore.js и передаёт таблице через props -->
  <BookingTable :items="bookings" />
</template>

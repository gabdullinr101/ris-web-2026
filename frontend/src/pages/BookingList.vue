<script setup>
import { ref, onMounted } from 'vue'
import BookingTable from '../components/BookingTable.vue'
import { bookings } from '../bookingsStore'
import { getBookings, assignBooking, changeBookingStatus, errorMessage } from '../api/bookings'
import { getTrainers } from '../api/users'
import { currentUser, loadCurrentUser } from '../authStore'

// Одно поле состояния экрана: 'loading' | 'empty' | 'data' | 'error'
const screen = ref('loading')
const errorText = ref('')

// Тренеры для выпадающего списка - только тем, кто назначает
const trainers = ref([])

// Результат последнего действия над строкой
const actionMessage = ref('')
const actionError = ref('')
const busyId = ref(null)

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

async function loadTrainers() {
  if (currentUser.value?.role === 'Admin' || currentUser.value?.role === 'Manager') {
    try {
      trainers.value = await getTrainers()
    } catch {
      trainers.value = []
    }
  }
}

// Заменить строку в общем массиве на ту, что вернул сервер
function replaceRow(updated) {
  const index = bookings.value.findIndex((b) => b.id === updated.id)
  if (index !== -1) {
    bookings.value[index] = updated
  }
}

async function runAction(id, action, okText) {
  if (busyId.value !== null) {
    return
  }
  busyId.value = id
  actionMessage.value = ''
  actionError.value = ''
  try {
    const updated = await action()
    replaceRow(updated)
    actionMessage.value = okText(updated)
  } catch (error) {
    actionError.value = errorMessage(error)
  } finally {
    busyId.value = null
  }
}

function onAssign(id, trainerId) {
  runAction(
    id,
    () => assignBooking(id, trainerId),
    (b) => `Запись ${b.number}: назначен тренер ${b.assigneeFullName}`,
  )
}

function onChangeStatus(id, status) {
  runAction(
    id,
    () => changeBookingStatus(id, status),
    (b) => `Запись ${b.number}: статус изменён`,
  )
}

onMounted(async () => {
  if (!currentUser.value) {
    await loadCurrentUser()
  }
  await Promise.all([load(), loadTrainers()])
})
</script>

<template>
  <h1>Записи на тренировки</h1>

  <p v-if="actionMessage" class="ok" role="status">{{ actionMessage }}</p>
  <p v-if="actionError" class="error" role="alert">{{ actionError }}</p>

  <p v-if="screen === 'loading'">Загрузка…</p>
  <p v-else-if="screen === 'empty'">Записей пока нет</p>
  <p v-else-if="screen === 'error'" class="error" role="alert">
    {{ errorText }}
    <router-link v-if="errorText === 'Войдите в систему'" to="/login">Перейти ко входу</router-link>
  </p>
  <BookingTable
    v-else
    :items="bookings"
    :trainers="trainers"
    :user="currentUser"
    :busy-id="busyId"
    @assign="onAssign"
    @change-status="onChangeStatus"
  />
</template>

<style scoped>
.error {
  color: #b3261e;
}

.ok {
  color: #1d6b34;
}
</style>

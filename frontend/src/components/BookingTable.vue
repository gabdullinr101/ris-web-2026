<script setup>
import { ref } from 'vue'
import { statusNames, trainerActions } from '../statuses'

// Таблица не хранит записи сама: массив приходит от страницы через props.
// Действия (назначить, сменить статус) таблица не выполняет - сообщает о них странице событиями.
const props = defineProps({
  items: {
    type: Array,
    required: true,
  },
  // тренеры для выбора при назначении; пустой массив - назначать нельзя
  trainers: {
    type: Array,
    default: () => [],
  },
  // кто вошёл: { id, role } или null
  user: {
    type: Object,
    default: null,
  },
  // id записи, по которой сейчас идёт запрос - её кнопки заблокированы
  busyId: {
    type: Number,
    default: null,
  },
})

const emit = defineEmits(['assign', 'change-status'])

// Выбранный в выпадающем списке тренер для каждой строки: { [bookingId]: trainerId }
const chosenTrainer = ref({})

function canAssign(item) {
  if (!props.user || props.trainers.length === 0) {
    return false
  }
  if (props.user.role === 'Admin') {
    return item.status === 'New'
  }
  if (props.user.role === 'Manager') {
    return item.status === 'New' || item.status === 'InProgress'
  }
  return false
}

function statusButtons(item) {
  if (!props.user || props.user.role !== 'Trainer' || item.assigneeUserId !== props.user.id) {
    return []
  }
  return trainerActions[item.status] ?? []
}

function onAssign(item) {
  const trainerId = chosenTrainer.value[item.id] ?? item.assigneeUserId
  if (trainerId) {
    emit('assign', item.id, trainerId)
  }
}
</script>

<template>
  <table>
    <thead>
      <tr>
        <th scope="col">id</th>
        <th scope="col">Номер</th>
        <th scope="col">Вид тренировки</th>
        <th scope="col">Зал</th>
        <th scope="col">Тренер</th>
        <th scope="col">Описание</th>
        <th scope="col">Статус</th>
        <th scope="col">Действия</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.id }}</td>
        <td>{{ item.number }}</td>
        <td>{{ item.title }}</td>
        <td>{{ item.hallName ?? item.hallId }}</td>
        <td>{{ item.assigneeFullName ?? '-' }}</td>
        <!-- Только интерполяция {{ }}, не v-html: текст вроде <script> показывается как текст -->
        <td>{{ item.description }}</td>
        <td>{{ statusNames[item.status] ?? item.status }}</td>
        <td>
          <div class="actions">
          <template v-if="canAssign(item)">
            <label class="visually-hidden" :for="'trainer-' + item.id">Тренер для записи {{ item.number }}</label>
            <select
              :id="'trainer-' + item.id"
              :value="chosenTrainer[item.id] ?? item.assigneeUserId ?? ''"
              :disabled="busyId === item.id"
              @change="chosenTrainer[item.id] = Number($event.target.value)"
            >
              <option value="" disabled>Выберите тренера</option>
              <option v-for="trainer in trainers" :key="trainer.id" :value="trainer.id">
                {{ trainer.fullName }}
              </option>
            </select>
            <button type="button" :disabled="busyId === item.id" @click="onAssign(item)">
              Назначить
            </button>
          </template>
          <button
            v-for="action in statusButtons(item)"
            :key="action.status"
            type="button"
            :disabled="busyId === item.id"
            @click="emit('change-status', item.id, action.status)"
          >
            {{ action.label }}
          </button>
          </div>
        </td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 10px 12px;
  text-align: left;
  vertical-align: top;
  border-bottom: 1px solid #e3e8ee;
}

th {
  font-size: 14px;
  color: #51606f;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  min-width: 180px;
}

select,
button {
  padding: 4px 8px;
  font: inherit;
  font-size: 14px;
  background: #ffffff;
  border: 1px solid #b6c0cc;
  border-radius: 6px;
}

button {
  cursor: pointer;
}

button:disabled,
select:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
  white-space: nowrap;
}
</style>

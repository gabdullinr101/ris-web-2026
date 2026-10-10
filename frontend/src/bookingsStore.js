import { ref } from 'vue'

// Единственный общий массив записей на клиенте.
// Заполняется ответом getBookings() из src/api/bookings.js, своих тестовых данных здесь больше нет.
export const bookings = ref([])

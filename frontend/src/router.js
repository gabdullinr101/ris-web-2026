import { createRouter, createWebHistory } from 'vue-router'

import BookingList from './pages/BookingList.vue'
import BookingNew from './pages/BookingNew.vue'
import Login from './pages/Login.vue'

// Адреса экранов в браузере. Это не адреса API:
// /new только показывает форму, запись создаст будущий запрос POST /api/bookings.
const routes = [
  { path: '/', component: BookingList },
  { path: '/new', component: BookingNew },
  { path: '/login', component: Login },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router

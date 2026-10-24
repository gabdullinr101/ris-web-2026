<script setup>
import { currentUser, roleNames } from '../authStore'
</script>

<template>
  <header class="top">
    <div>
      <p class="brand">Бронирование тренировок</p>
      <p v-if="currentUser" class="who">
        {{ currentUser.fullName }} · {{ roleNames[currentUser.role] }}
      </p>
    </div>
    <nav aria-label="Экраны">
      <router-link to="/">Список</router-link>
      <!-- Подсказка для экрана: создаёт администратор. Запрет всё равно проверяет сервер (403). -->
      <router-link v-if="!currentUser || currentUser.role === 'Admin'" to="/new">Создать</router-link>
      <router-link to="/login">{{ currentUser ? 'Выход' : 'Вход' }}</router-link>
    </nav>
  </header>
</template>

<style scoped>
.top {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 24px;
}

.brand {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
}

.who {
  margin: 2px 0 0;
  font-size: 14px;
  color: #51606f;
}

nav {
  display: flex;
  gap: 8px;
}

a {
  padding: 8px 16px;
  color: #1c2530;
  text-decoration: none;
  background: #ffffff;
  border: 1px solid #b6c0cc;
  border-radius: 6px;
}

a:hover {
  border-color: #1f5fbf;
}

a:focus-visible {
  outline: 2px solid #1f5fbf;
  outline-offset: 1px;
}

/* Этот класс роутер сам ставит ссылке, адрес которой сейчас открыт */
a.router-link-exact-active {
  color: #ffffff;
  background: #1f5fbf;
  border-color: #1f5fbf;
}
</style>

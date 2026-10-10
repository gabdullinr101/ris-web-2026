<script setup>
import { ref, onMounted } from 'vue'

const loginInput = ref(null)

onMounted(() => {
  loginInput.value?.focus()
})
</script>

<template>
  <h1>Вход в систему</h1>
  <!-- Настоящей авторизации ещё нет, это заготовка экрана -->
  <form @submit.prevent>
    <label for="login">Логин</label>
    <input id="login" ref="loginInput" name="login" type="text" autocomplete="username" />

    <label for="password">Пароль</label>
    <input id="password" name="password" type="password" autocomplete="current-password" />

    <button type="submit">Войти</button>
  </form>
</template>

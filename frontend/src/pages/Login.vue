<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { login, logout } from '../api/auth'
import { errorMessage } from '../api/http'
import { currentUser, authChecked, roleNames } from '../authStore'

const router = useRouter()

const form = ref({ login: '', password: '' })
const formError = ref('')
const isSubmitting = ref(false)

const loginInput = ref(null)

onMounted(() => {
  loginInput.value?.focus()
})

// Форма появляется после проверки входа - тогда и ставим курсор в первое поле
watch(authChecked, async () => {
  await nextTick()
  loginInput.value?.focus()
})

async function onLogin() {
  if (isSubmitting.value) {
    return
  }
  formError.value = ''

  if (!form.value.login.trim() || !form.value.password) {
    formError.value = 'Введите логин и пароль'
    return
  }

  isSubmitting.value = true
  try {
    currentUser.value = await login(form.value.login.trim(), form.value.password)
    form.value = { login: '', password: '' }
    router.push('/')
  } catch (error) {
    // 401 - неверная пара логин/пароль; остальное - общие тексты
    formError.value = error.status === 401 ? 'Неверный логин или пароль' : errorMessage(error)
    form.value.password = ''
  } finally {
    isSubmitting.value = false
  }
}

async function onLogout() {
  try {
    await logout()
  } finally {
    currentUser.value = null
  }
}
</script>

<template>
  <h1>Вход в систему</h1>

  <p v-if="!authChecked">Проверяем вход…</p>

  <div v-else-if="currentUser" class="signed-in">
    <p>
      Вы вошли как {{ currentUser.fullName }} ({{ roleNames[currentUser.role] }}).
    </p>
    <button type="button" @click="onLogout">Выйти</button>
  </div>

  <form v-else @submit.prevent="onLogin">
    <label for="login">Логин</label>
    <input
      id="login"
      ref="loginInput"
      v-model="form.login"
      name="login"
      type="text"
      autocomplete="username"
    />

    <label for="password">Пароль</label>
    <input
      id="password"
      v-model="form.password"
      name="password"
      type="password"
      autocomplete="current-password"
    />

    <p v-if="formError" class="form-error" role="alert">{{ formError }}</p>

    <button type="submit" :disabled="isSubmitting">
      {{ isSubmitting ? 'Входим…' : 'Войти' }}
    </button>
  </form>
</template>

<style scoped>
.form-error {
  margin: 0 0 16px;
  padding: 8px 12px;
  color: #8c1d18;
  background: #fdecea;
  border: 1px solid #f1b5b0;
  border-radius: 6px;
}

.signed-in button {
  padding: 8px 16px;
  font: inherit;
  background: #ffffff;
  border: 1px solid #b6c0cc;
  border-radius: 6px;
  cursor: pointer;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

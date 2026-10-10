<template>
  <header class="top">
    <p class="brand">Бронирование тренировок</p>
    <nav aria-label="Экраны">
      <router-link to="/">Список</router-link>
      <router-link to="/new">Создать</router-link>
      <router-link to="/login">Вход</router-link>
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

<script setup>
import AppHeader from './components/AppHeader.vue'
</script>

<template>
  <div class="page">
    <!-- Шапка одна на всё приложение и стоит снаружи router-view -->
    <AppHeader />

    <!-- Сюда роутер подставляет страницу по текущему адресу -->
    <main>
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.page {
  max-width: 860px;
  margin: 0 auto;
  padding: 24px 16px 48px;
}

main {
  padding: 20px;
  background: #ffffff;
  border: 1px solid #d8dee6;
  border-radius: 8px;
  overflow-x: auto;
}
</style>

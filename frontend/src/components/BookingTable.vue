<script setup>
// Таблица не хранит данные сама: массив приходит от страницы через props
defineProps({
  items: {
    type: Array,
    required: true,
  },
})
</script>

<template>
  <table>
    <thead>
      <tr>
        <th scope="col">Номер</th>
        <th scope="col">Вид тренировки (title)</th>
        <th scope="col">Зал (hallId)</th>
        <th scope="col">Статус</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.number }}</td>
        <td>{{ item.title }}</td>
        <td>{{ item.hallId }}</td>
        <td>{{ item.status }}</td>
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
  border-bottom: 1px solid #e3e8ee;
}

th {
  font-size: 14px;
  color: #51606f;
}
</style>

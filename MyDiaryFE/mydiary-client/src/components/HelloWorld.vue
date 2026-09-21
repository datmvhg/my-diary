<script setup lang="ts">
import { onMounted, ref } from "vue";
import { getApiDiaryMoments } from "../api/generated";

const diaries = ref();
const loading = ref(false);
const error = ref<string | null>(null);

const loadDiaries = async () => {
  loading.value = true;
  error.value = null;

  try {
    const response = await getApiDiaryMoments();

    diaries.value = response.data;

    console.log("Diary response:", response);
    console.log("Diary data:", response.data);
  } catch (e) {
    console.error(e);
    error.value = "Không thể tải danh sách diary";
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadDiaries();
});
</script>

<template>
  <div class="diary-page">
    <h1>My Diary</h1>

    <p v-if="loading">
      Đang tải...
    </p>

    <p v-else-if="error">
      {{ error }}
    </p>

    <div v-else>
      <pre>{{ diaries }}</pre>
    </div>
  </div>
</template>
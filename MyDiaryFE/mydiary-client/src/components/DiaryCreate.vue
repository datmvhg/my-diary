<template>
  <div class="diary-create">
    <h1>Create Diary Entry</h1>
    <form @submit.prevent="onSubmit">
      <div>
        <label for="title">Title</label>
        <input id="title" v-model="title" type="text" />
      </div>

      <div>
        <label for="description">Description</label>
        <textarea id="description" v-model="description" rows="6"></textarea>
      </div>

      <div>
        <label for="emotion">Emotion</label>
        <select id="emotion" v-model="emotion">
          <option :value="undefined">(unset)</option>
          <option :value="0">Neutral</option>
          <option :value="1">Happy</option>
          <option :value="2">Sad</option>
          <option :value="3">Excited</option>
          <option :value="4">Angry</option>
          <option :value="5">Anxious</option>
          <option :value="6">Grateful</option>
        </select>
      </div>

      <div>
        <label for="momentAt">Moment at</label>
        <input id="momentAt" v-model="momentAt" type="datetime-local" />
      </div>

      <div>
        <label for="image">Image</label>
        <input id="image" ref="imageInput" @change="onFileChange" type="file" accept="image/*" />
      </div>

      <div style="margin-top:12px">
        <button type="submit" class="create-button" :disabled="loading">Save</button>
        <span v-if="loading">Saving...</span>
      </div>

      <div v-if="successMessage" style="color:green;margin-top:8px">{{ successMessage }}</div>
      <div v-if="errorMessage" style="color:red;margin-top:8px">{{ errorMessage }}</div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { postApiDiaryMoments } from '../api/generated'

const title = ref('')
const description = ref('')
const emotion = ref<number | null>(null)
const momentAt = ref('')
const imageFile = ref<File | null>(null)
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')
const imageInput = ref<HTMLInputElement | null>(null)

function onFileChange(e: Event) {
  const el = e.target as HTMLInputElement
  const file = el.files && el.files[0]
  imageFile.value = file ?? null
}

async function onSubmit() {
  loading.value = true
  successMessage.value = ''
  errorMessage.value = ''

  try {
    // Build the body expected by the generated SDK. It will serialize as form-data.
    // Convert local datetime-local value to UTC ISO string
    let momentAtUtc: string | undefined = undefined
    if (momentAt.value) {
      const d = new Date(momentAt.value)
      if (!Number.isNaN(d.getTime())) {
        momentAtUtc = d.toISOString()
      }
    }

    // Emotion is selected as server enum (0..6)
    const mappedEmotion = emotion.value
    if (mappedEmotion !== undefined && mappedEmotion !== null) {
      const v = Number(mappedEmotion)
      if (Number.isNaN(v) || v < 0 || v > 6) {
        errorMessage.value = 'Invalid emotion selection.'
        loading.value = false
        return
      }
    }

    const body: any = {
      Title: title.value || undefined,
      Description: description.value || undefined,
      Emotion: mappedEmotion ?? undefined,
      MomentAt: momentAtUtc || undefined,
      Image: imageFile.value || undefined,
    }

    await postApiDiaryMoments({ body })
    // SDK returns request result; assume success if no throw
    successMessage.value = 'Entry saved successfully.'
    // reset form
    title.value = ''
    description.value = ''
    emotion.value = null
    momentAt.value = ''
    if (imageInput.value) imageInput.value.value = ''
    imageFile.value = null
  } catch (err: any) {
    console.error('Failed to save diary entry', err)
    errorMessage.value = err?.message ?? String(err)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.diary-create {
  max-width: 720px;
  margin: 24px auto;
  padding: 16px;
  border: 1px solid #ddd;
  border-radius: 6px;
}
.diary-create label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
}
.diary-create input[type="text"],
.diary-create input[type="number"],
.diary-create input[type="datetime-local"],
.diary-create textarea {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
  margin-bottom: 8px;
}
</style>

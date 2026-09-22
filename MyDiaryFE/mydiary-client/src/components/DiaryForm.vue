<template>
  <div class="modal-overlay">
    <div class="modal-card" role="dialog" aria-modal="true">
      <!-- Modal Header -->
      <div class="modal-header">
        <div class="header-text">
          <div class="header-badge">{{ isEdit ? 'Chỉnh sửa' : 'Khoảnh khắc mới' }}</div>
          <h2 class="modal-title">{{ isEdit ? 'Cập nhật nhật ký' : 'Viết nhật ký mới' }}</h2>
          <p class="modal-subtitle">Ghi lại cảm xúc và suy nghĩ của bạn trong khoảnh khắc này.</p>
        </div>
        <button class="close-btn" @click="onCancel" title="Đóng (Esc)" aria-label="Close">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>

      <!-- Modal Body Form -->
      <form class="modal-body" @submit.prevent="onSubmit">
        <!-- Title Input -->
        <div class="form-group">
          <label class="form-label" for="diary-title">Tiêu đề</label>
          <input
            id="diary-title"
            v-model="form.Title"
            type="text"
            class="form-input"
            placeholder="Đặt một tiêu đề ý nghĩa..."
            maxlength="150"
            required
          />
        </div>

        <!-- Emotion Selector -->
        <div class="form-group">
          <label class="form-label">Cảm xúc hiện tại</label>
          <div class="emotion-picker">
            <button
              type="button"
              v-for="item in emotionOptions"
              :key="item.value"
              class="emotion-chip"
              :class="{
                active: form.Emotion === item.value,
                [item.key]: true
              }"
              @click="selectEmotion(item.value)"
            >
              <span class="chip-emoji">{{ item.icon }}</span>
              <span class="chip-label">{{ item.label }}</span>
            </button>
          </div>
        </div>

        <!-- Moment Time & Date -->
        <div class="form-group">
          <label class="form-label" for="diary-time">Thời điểm ghi nhận</label>
          <input
            id="diary-time"
            v-model="form.MomentAt"
            type="datetime-local"
            class="form-input"
          />
        </div>

        <!-- Description Textarea -->
        <div class="form-group">
          <label class="form-label" for="diary-desc">Nội dung nhật ký</label>
          <textarea
            id="diary-desc"
            v-model="form.Description"
            rows="5"
            class="form-textarea"
            placeholder="Chia sẻ câu chuyện, bài học hoặc cảm nhận của bạn..."
          ></textarea>
        </div>

        <!-- Image Upload & Management Section -->
        <div class="form-group">
          <label class="form-label">Hình ảnh kỷ niệm</label>
          <input
            ref="fileInputRef"
            type="file"
            accept="image/*"
            class="file-input-hidden"
            @change="onFileChange"
          />

          <!-- Image Preview & Controls if image exists or selected -->
          <div v-if="currentPreviewUrl" class="image-preview-wrapper">
            <div class="image-preview-card">
              <img :src="currentPreviewUrl" alt="Hình ảnh nhật ký" class="preview-img" />
              <!-- Status Badge -->
              <div class="preview-status-badge" :class="{ 'is-new': isNewFileSelected }">
                <span class="status-dot"></span>
                <span>{{ isNewFileSelected ? 'Ảnh mới đã chọn' : 'Ảnh hiện tại của nhật ký' }}</span>
              </div>
            </div>

            <!-- Action Buttons for Existing/Selected Image -->
            <div class="image-action-bar">
              <button
                type="button"
                class="img-btn-change"
                @click="triggerFileInput"
                title="Chọn ảnh khác từ máy"
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"/>
                  <path d="M15 5l4 4"/>
                </svg>
                <span>{{ isNewFileSelected ? 'Chọn ảnh khác' : 'Thay đổi hình ảnh' }}</span>
              </button>
              <button
                type="button"
                class="img-btn-remove"
                @click="removeImage"
                title="Xóa hình ảnh này khỏi bài viết"
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                  <line x1="10" y1="11" x2="10" y2="17"></line>
                  <line x1="14" y1="11" x2="14" y2="17"></line>
                </svg>
                <span>Xóa hình ảnh</span>
              </button>
            </div>
          </div>

          <!-- Dropzone if no image -->
          <div v-else class="upload-dropzone-container">
            <div class="upload-dropzone" @click="triggerFileInput">
              <div class="dropzone-icon">
                <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                  <circle cx="8.5" cy="8.5" r="1.5"></circle>
                  <polyline points="21 15 16 10 5 21"></polyline>
                </svg>
              </div>
              <div class="dropzone-text">
                <span class="highlight">Thêm hình ảnh</span> hoặc kéo thả vào đây
              </div>
              <span class="dropzone-hint">Hỗ trợ JPG, PNG, WEBP (Tối đa 10MB)</span>
            </div>

            <!-- Restore Button if user removed an existing image in this edit session -->
            <button
              v-if="isEdit && initialImageUrl && isImageRemoved"
              type="button"
              class="restore-image-btn"
              @click="restoreOriginalImage"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path>
                <path d="M3 3v5h5"></path>
              </svg>
              Khôi phục ảnh ban đầu
            </button>
          </div>
        </div>

        <!-- Error Alert if any -->
        <div v-if="error" class="error-banner">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
          <span>{{ error }}</span>
        </div>

        <!-- Form Actions -->
        <div class="modal-footer">
          <button type="button" class="btn-secondary" @click="onCancel">
            Hủy bỏ
          </button>
          <button type="submit" class="btn-primary" :disabled="loading">
            <svg v-if="loading" class="spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
            </svg>
            <span>{{ loading ? 'Đang lưu...' : (isEdit ? 'Lưu thay đổi' : 'Tạo khoảnh khắc') }}</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onBeforeUnmount } from 'vue'

const props = defineProps<{
  initial?: any
  isEdit?: boolean
}>()

const emit = defineEmits(['saved', 'close'])

const isEdit = !!props.isEdit

const emotionOptions = [
  { value: 1, label: 'Vui vẻ', icon: '😊', key: 'happy' },
  { value: 3, label: 'Hào hứng', icon: '🤩', key: 'excited' },
  { value: 6, label: 'Biết ơn', icon: '🙏', key: 'grateful' },
  { value: 0, label: 'Bình thản', icon: '😐', key: 'neutral' },
  { value: 5, label: 'Lo âu', icon: '😟', key: 'anxious' },
  { value: 2, label: 'Buồn bã', icon: '😢', key: 'sad' },
  { value: 4, label: 'Giận dữ', icon: '😠', key: 'angry' },
]

function toLocalDatetimeInput(val: any) {
  if (!val) {
    const now = new Date()
    const pad = (n: number) => String(n).padStart(2, '0')
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`
  }
  const d = val instanceof Date ? val : new Date(String(val))
  if (Number.isNaN(d.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const form = reactive<any>({
  Title: props.initial?.Title ?? props.initial?.title ?? '',
  Description: props.initial?.Description ?? props.initial?.description ?? '',
  Emotion: props.initial?.Emotion ?? props.initial?.emotion ?? 1, // default Happy
  MomentAt: toLocalDatetimeInput(props.initial?.MomentAt ?? props.initial?.momentAt ?? ''),
  Image: undefined,
})

const loading = ref(false)
const error = ref('')
const fileInputRef = ref<HTMLInputElement | null>(null)

// Resolve initial existing image URL
const initialImageUrl = computed(() => {
  if (props.initial?.imageUrl || props.initial?.ImageUrl) {
    return props.initial.imageUrl || props.initial.ImageUrl
  }
  const id = props.initial?.Id ?? props.initial?.id
  const hasImg = props.initial?.HasImage ?? props.initial?.hasImage
  if (isEdit && hasImg && id != null) {
    const apiBase = (import.meta.env.VITE_API_BASE_URL as string)?.replace(/\/+$/, '') 
      ?? (import.meta.env.DEV ? 'http://localhost:5224' : '')
    return `${apiBase}/api/DiaryMoments/${id}/image`
  }
  return null
})

const isImageRemoved = ref(false)
const filePreviewUrl = ref<string | null>(null)

const isNewFileSelected = computed(() => !!filePreviewUrl.value)

const currentPreviewUrl = computed(() => {
  if (filePreviewUrl.value) return filePreviewUrl.value
  if (!isImageRemoved.value && initialImageUrl.value) return initialImageUrl.value
  return null
})

function selectEmotion(val: number) {
  form.Emotion = val
}

function triggerFileInput() {
  fileInputRef.value?.click()
}

function onFileChange(e: Event) {
  const el = e.target as HTMLInputElement
  const file = el.files && el.files[0]
  if (file) {
    if (filePreviewUrl.value) {
      URL.revokeObjectURL(filePreviewUrl.value)
      filePreviewUrl.value = null
    }
    form.Image = file
    filePreviewUrl.value = URL.createObjectURL(file)
    isImageRemoved.value = false
  }
}

function removeImage() {
  if (filePreviewUrl.value) {
    URL.revokeObjectURL(filePreviewUrl.value)
    filePreviewUrl.value = null
  }
  form.Image = undefined
  isImageRemoved.value = true
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

function restoreOriginalImage() {
  if (filePreviewUrl.value) {
    URL.revokeObjectURL(filePreviewUrl.value)
    filePreviewUrl.value = null
  }
  form.Image = undefined
  isImageRemoved.value = false
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

function onCancel() {
  emit('close')
}

// Keydown listener for Esc
function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    onCancel()
  }
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
  if (filePreviewUrl.value) {
    URL.revokeObjectURL(filePreviewUrl.value)
    filePreviewUrl.value = null
  }
})

async function onSubmit() {
  loading.value = true
  error.value = ''
  try {
    let momentAtUtc: string | undefined = undefined
    if (form.MomentAt) {
      const d = new Date(form.MomentAt)
      if (!Number.isNaN(d.getTime())) {
        momentAtUtc = d.toISOString()
      }
    }

    const rawEmotion = (form.Emotion ?? props.initial?.Emotion ?? props.initial?.emotion) ?? 0
    const finalTitle = form.Title !== '' ? form.Title : (props.initial?.Title ?? props.initial?.title) || 'Untitled'
    const finalDesc = form.Description !== '' ? form.Description : (props.initial?.Description ?? props.initial?.description) || ''
    const finalMomentAt = momentAtUtc ?? (props.initial?.MomentAt ?? props.initial?.momentAt) ?? new Date().toISOString()

    const formData = new FormData()
    formData.append('Title', finalTitle)
    formData.append('Description', finalDesc)
    formData.append('Emotion', String(rawEmotion))
    formData.append('MomentAt', finalMomentAt)

    if (form.Image instanceof File) {
      formData.append('Image', form.Image)
    } else if (isEdit && isImageRemoved.value) {
      formData.append('RemoveImage', 'true')
    }

    const apiBase = (import.meta.env.VITE_API_BASE_URL as string)?.replace(/\/+$/, '') 
      ?? (import.meta.env.DEV ? 'http://localhost:5224' : '')

    if (isEdit && props.initial && (props.initial.Id ?? props.initial.id) != null) {
      const id = Number(props.initial.Id ?? props.initial.id)
      const res = await fetch(`${apiBase}/api/DiaryMoments/${id}`, {
        method: 'PUT',
        body: formData,
      })
      if (!res.ok) {
        const text = await res.text().catch(() => '')
        throw new Error(`Cập nhật thất bại (${res.status}): ${text}`)
      }
      const imageChanged = !!form.Image || isImageRemoved.value
      const imageRemoved = isImageRemoved.value && !form.Image
      emit('saved', { id, imageChanged, imageRemoved, isNew: false })
      emit('close')
      return
    }

    const res = await fetch(`${apiBase}/api/DiaryMoments`, {
      method: 'POST',
      body: formData,
    })
    if (!res.ok) {
      const text = await res.text().catch(() => '')
      throw new Error(`Thêm mới thất bại (${res.status}): ${text}`)
    }
    const d = await res.json().catch(() => null)
    const createdId = d ? Number(d.id ?? d.Id) : undefined

    emit('saved', { id: createdId, imageChanged: !!form.Image, isNew: true })
    emit('close')
  } catch (err: any) {
    console.error('save failed', err)
    error.value = err?.message ?? 'Đã xảy ra lỗi khi lưu nhật ký. Vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 1000;
  animation: fadeIn 0.2s ease-out;
}

.modal-card {
  background: var(--bg-card);
  width: 100%;
  max-width: 580px;
  max-height: 90vh;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-modal);
  border: 1px solid var(--border-light);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalScaleIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  padding: 1.5rem 1.75rem 1rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  border-bottom: 1px solid var(--border-subtle);
}

.header-badge {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--primary);
  background: var(--primary-light);
  padding: 2px 8px;
  border-radius: var(--radius-full);
  margin-bottom: 0.375rem;
}

.modal-title {
  font-size: 1.35rem;
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1.25;
}

.modal-subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin-top: 0.25rem;
}

.close-btn {
  background: transparent;
  color: var(--text-muted);
  border-radius: var(--radius-full);
  padding: 0.5rem;
  border: none;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.close-btn:hover {
  background: var(--bg-subtle);
  color: var(--text-primary);
}

.modal-body {
  padding: 1.5rem 1.75rem;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
}

.form-input, .form-textarea {
  width: 100%;
  padding: 0.75rem 1rem;
  font-size: 0.9375rem;
  border: 1px solid var(--border-light);
  border-radius: var(--radius-md);
  background: var(--bg-card);
  color: var(--text-primary);
  box-sizing: border-box;
}

.form-textarea {
  resize: vertical;
  min-height: 110px;
  line-height: 1.6;
}

/* Interactive Emotion Picker */
.emotion-picker {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.emotion-chip {
  padding: 0.45rem 0.85rem;
  border-radius: var(--radius-full);
  border: 1px solid var(--border-light);
  background: var(--bg-subtle);
  color: var(--text-secondary);
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  transition: all var(--transition-fast);
}

.chip-emoji {
  font-size: 1.1rem;
  line-height: 1;
}

.emotion-chip:hover {
  border-color: var(--border-hover);
  background: #e2e8f0;
}

.emotion-chip.active {
  border-color: transparent;
  transform: scale(1.03);
  box-shadow: var(--shadow-sm);
}

.emotion-chip.active.happy {
  background: #dcfce7;
  color: #15803d;
  border-color: #86efac;
}

.emotion-chip.active.excited {
  background: #fef3c7;
  color: #b45309;
  border-color: #fde68a;
}

.emotion-chip.active.grateful {
  background: #ede9fe;
  color: #6d28d9;
  border-color: #ddd6fe;
}

.emotion-chip.active.neutral {
  background: #f1f5f9;
  color: #475569;
  border-color: #cbd5e1;
}

.emotion-chip.active.anxious {
  background: #ffedd5;
  color: #c2410c;
  border-color: #fed7aa;
}

.emotion-chip.active.sad {
  background: #e0f2fe;
  color: #0369a1;
  border-color: #bae6fd;
}

.emotion-chip.active.angry {
  background: #ffe4e6;
  color: #be123c;
  border-color: #fecdd3;
}

/* Image Upload & Dropzone */
.file-input-hidden {
  display: none;
}

.upload-dropzone {
  border: 2px dashed var(--border-hover);
  border-radius: var(--radius-lg);
  padding: 1.75rem 1rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  cursor: pointer;
  background: var(--bg-subtle);
  transition: all var(--transition-fast);
}

.upload-dropzone:hover {
  border-color: var(--primary);
  background: var(--primary-light);
}

.dropzone-icon {
  color: var(--primary);
  margin-bottom: 0.5rem;
}

.dropzone-text {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.dropzone-text .highlight {
  color: var(--primary);
  font-weight: 600;
}

.dropzone-hint {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-top: 0.25rem;
}

.image-preview-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.image-preview-card {
  position: relative;
  border-radius: var(--radius-lg);
  overflow: hidden;
  aspect-ratio: 16 / 9;
  max-height: 240px;
  background: var(--bg-subtle);
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow-sm);
}

.preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.preview-status-badge {
  position: absolute;
  top: 0.75rem;
  left: 0.75rem;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: 0.75rem;
  font-weight: 600;
  background: rgba(15, 23, 42, 0.82);
  color: white;
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.18);
}

.preview-status-badge .status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #38bdf8;
}

.preview-status-badge.is-new .status-dot {
  background: #4ade80;
}

.image-action-bar {
  display: flex;
  gap: 0.5rem;
}

.img-btn-change, .img-btn-remove {
  flex: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  padding: 0.55rem 0.875rem;
  border-radius: var(--radius-md);
  font-size: 0.8125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.img-btn-change {
  background: var(--bg-subtle);
  color: var(--text-primary);
  border: 1px solid var(--border-light);
}

.img-btn-change:hover {
  background: var(--primary-light);
  color: var(--primary);
  border-color: var(--primary);
}

.img-btn-remove {
  background: var(--danger-light);
  color: var(--danger-hover);
  border: 1px solid #fecaca;
}

.img-btn-remove:hover {
  background: #fee2e2;
  color: #b91c1c;
  border-color: #f87171;
}

html.dark .img-btn-remove {
  background: rgba(239, 68, 68, 0.15);
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.35);
}

html.dark .img-btn-remove:hover {
  background: rgba(239, 68, 68, 0.25);
  color: #fecaca;
  border-color: rgba(239, 68, 68, 0.5);
}

.upload-dropzone-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.restore-image-btn {
  align-self: flex-start;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  background: transparent;
  border: none;
  color: var(--primary);
  font-size: 0.8125rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  transition: all var(--transition-fast);
}

.restore-image-btn:hover {
  background: var(--primary-light);
  text-decoration: underline;
}

/* Error Banner */
.error-banner {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  background: var(--danger-light);
  color: var(--danger-hover);
  font-size: 0.875rem;
  border: 1px solid #fecaca;
}

/* Modal Footer */
.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-subtle);
}

.btn-secondary {
  background: var(--bg-subtle);
  color: var(--text-secondary);
  padding: 0.7rem 1.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-light);
}

.btn-secondary:hover {
  background: #e2e8f0;
  color: var(--text-primary);
}

.btn-primary {
  background: var(--primary);
  color: white;
  padding: 0.7rem 1.5rem;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.btn-primary:hover:not(:disabled) {
  background: var(--primary-hover);
  box-shadow: var(--shadow-md);
  transform: translateY(-1px);
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>

<template>
  <div class="lightbox-overlay" @click.self="onClose">
    <!-- Top Action Bar -->
    <div class="lightbox-topbar">
      <div class="lightbox-title">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
          <circle cx="8.5" cy="8.5" r="1.5"></circle>
          <polyline points="21 15 16 10 5 21"></polyline>
        </svg>
        <span>{{ title || 'Xem hình ảnh kỷ niệm' }}</span>
      </div>

      <div class="lightbox-actions">
        <!-- Open original in new tab -->
        <a
          :href="imageUrl"
          target="_blank"
          rel="noopener noreferrer"
          class="action-btn"
          title="Mở ảnh gốc trong tab mới"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path>
            <polyline points="15 3 21 3 21 9"></polyline>
            <line x1="10" y1="14" x2="21" y2="3"></line>
          </svg>
        </a>

        <!-- Close Button -->
        <button class="action-btn close" @click="onClose" title="Đóng (Esc)">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>
    </div>

    <!-- Image Display Container -->
    <div class="lightbox-stage" @click.self="onClose">
      <div class="lightbox-image-wrapper">
        <img
          :src="imageUrl"
          :alt="title || 'Hình ảnh nhật ký'"
          class="lightbox-img"
          @click.stop
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount } from 'vue'

defineProps<{
  imageUrl: string
  title?: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

function onClose() {
  emit('close')
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    onClose()
  }
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
})
</script>

<style scoped>
.lightbox-overlay {
  position: fixed;
  inset: 0;
  background: rgba(10, 15, 29, 0.92);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  z-index: 2100;
  display: flex;
  flex-direction: column;
  animation: fadeIn 0.2s ease-out;
  user-select: none;
}

.lightbox-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: linear-gradient(180deg, rgba(0, 0, 0, 0.7) 0%, transparent 100%);
  z-index: 10;
}

.lightbox-title {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  color: rgba(255, 255, 255, 0.92);
  font-size: 0.95rem;
  font-weight: 600;
  max-width: 70%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.lightbox-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.action-btn {
  background: rgba(255, 255, 255, 0.12);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  text-decoration: none;
  transition: all var(--transition-fast);
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.25);
  transform: scale(1.06);
}

.action-btn.close:hover {
  background: rgba(239, 68, 68, 0.85);
  border-color: #ef4444;
}

.lightbox-stage {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem 1.5rem 2rem;
  overflow: hidden;
  cursor: zoom-out;
}

.lightbox-image-wrapper {
  max-width: 92vw;
  max-height: 85vh;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: zoomIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  cursor: default;
}

.lightbox-img {
  max-width: 92vw;
  max-height: 85vh;
  width: auto;
  height: auto;
  object-fit: contain;
  border-radius: var(--radius-lg);
  box-shadow: 0 25px 60px -15px rgba(0, 0, 0, 0.85);
  border: 1px solid rgba(255, 255, 255, 0.15);
}

@keyframes zoomIn {
  from {
    opacity: 0;
    transform: scale(0.92);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@media (max-width: 640px) {
  .lightbox-topbar {
    padding: 0.75rem 1rem;
  }
  .lightbox-stage {
    padding: 0.5rem 0.5rem 1.5rem;
  }
  .lightbox-img {
    max-width: 96vw;
    max-height: 80vh;
    border-radius: var(--radius-md);
  }
}
</style>

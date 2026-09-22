<template>
  <div class="modal-overlay">
    <div class="modal-card" role="dialog" aria-modal="true">
      <!-- Modal Header -->
      <div class="modal-header">
        <div class="header-left">
          <!-- Emotion Pill -->
          <div class="emotion-pill" :class="emotionInfo.className">
            <span class="emotion-emoji">{{ emotionInfo.icon }}</span>
            <span class="emotion-name">{{ emotionInfo.label }}</span>
          </div>
          <!-- Date & Time -->
          <div class="header-datetime">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
            <span>{{ formattedDate }}</span>
          </div>
        </div>

        <button class="close-btn" @click="onClose" title="Đóng" aria-label="Đóng">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>

      <!-- Modal Body -->
      <div class="modal-body">
        <!-- Hero Image if present -->
        <div
          v-if="imageUrl"
          class="detail-media"
          @click="showLightbox = true"
          title="Nhấn để xem ảnh phóng to"
        >
          <img :src="imageUrl" alt="Hình ảnh khoảnh khắc" class="detail-img" />
          <div class="media-hover-overlay">
            <span class="zoom-pill">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="11" cy="11" r="8"></circle>
                <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                <line x1="11" y1="8" x2="11" y2="14"></line>
                <line x1="8" y1="11" x2="14" y2="11"></line>
              </svg>
              Phóng to ảnh
            </span>
          </div>
        </div>

        <!-- Detail Content -->
        <div class="detail-content">
          <h2 class="detail-title">{{ entry.Title ?? entry.title ?? 'Không tiêu đề' }}</h2>
          <div class="detail-text">
            {{ entry.Description ?? entry.description ?? 'Không có nội dung mô tả.' }}
          </div>
        </div>
      </div>

      <!-- Modal Footer Actions -->
      <div class="modal-footer">
        <div class="footer-left">
          <button class="btn-action edit" @click="onEdit">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
            </svg>
            <span>Chỉnh sửa</span>
          </button>
          <button class="btn-action delete" @click="onDelete">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            </svg>
            <span>Xóa</span>
          </button>
        </div>

        <button class="btn-close-main" @click="onClose">
          Đóng
        </button>
      </div>
    </div>

    <!-- Fullscreen Image Lightbox Modal -->
    <Teleport to="body">
      <ImageLightbox
        v-if="showLightbox && imageUrl"
        :image-url="imageUrl"
        :title="entry.Title ?? entry.title ?? 'Hình ảnh nhật ký'"
        @close="showLightbox = false"
      />
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import ImageLightbox from './ImageLightbox.vue'

const showLightbox = ref(false)

const props = defineProps<{
  entry: any
  imageUrl?: string | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'edit', entry: any): void
  (e: 'delete', entry: any): void
}>()

const emotionMap: Record<number, { label: string; icon: string; className: string }> = {
  0: { label: 'Bình thản', icon: '😐', className: 'neutral' },
  1: { label: 'Vui vẻ', icon: '😊', className: 'happy' },
  2: { label: 'Buồn bã', icon: '😢', className: 'sad' },
  3: { label: 'Hào hứng', icon: '🤩', className: 'excited' },
  4: { label: 'Giận dữ', icon: '😠', className: 'angry' },
  5: { label: 'Lo âu', icon: '😟', className: 'anxious' },
  6: { label: 'Biết ơn', icon: '🙏', className: 'grateful' },
}

const emotionInfo = computed(() => {
  const em = Number(props.entry?.Emotion ?? props.entry?.emotion ?? 0)
  return emotionMap[em] || emotionMap[0]
})

const formattedDate = computed(() => {
  const raw = props.entry?.MomentAt ?? props.entry?.momentAt ?? props.entry?.createdAt
  if (!raw) return 'Không rõ thời gian'
  try {
    const d = new Date(raw)
    if (Number.isNaN(d.getTime())) return String(raw)
    return d.toLocaleDateString('vi-VN', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return String(raw)
  }
})

function onClose() {
  emit('close')
}

function onEdit() {
  emit('edit', props.entry)
}

function onDelete() {
  emit('delete', props.entry)
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  animation: fadeIn 0.2s ease-out;
}

.modal-card {
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-modal);
  width: 100%;
  max-width: 680px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalScaleIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--border-subtle);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  background: var(--bg-card);
}

.header-left {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.emotion-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.3rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: 0.8125rem;
  font-weight: 600;
  border: 1px solid var(--border-light);
  background: var(--bg-subtle);
  color: var(--text-primary);
}

.emotion-pill.happy { background: rgba(34, 197, 94, 0.12); color: #16a34a; border-color: rgba(34, 197, 94, 0.25); }
.emotion-pill.excited { background: rgba(245, 158, 11, 0.12); color: #d97706; border-color: rgba(245, 158, 11, 0.25); }
.emotion-pill.grateful { background: rgba(168, 85, 247, 0.12); color: #9333ea; border-color: rgba(168, 85, 247, 0.25); }
.emotion-pill.sad { background: rgba(59, 130, 246, 0.12); color: #2563eb; border-color: rgba(59, 130, 246, 0.25); }
.emotion-pill.angry { background: rgba(239, 68, 68, 0.12); color: #dc2626; border-color: rgba(239, 68, 68, 0.25); }
.emotion-pill.anxious { background: rgba(249, 115, 22, 0.12); color: #ea580c; border-color: rgba(249, 115, 22, 0.25); }
.emotion-pill.neutral { background: var(--bg-subtle); color: var(--text-secondary); border-color: var(--border-light); }

.header-datetime {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--text-muted);
  font-weight: 500;
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  padding: 0.4rem;
  border-radius: var(--radius-full);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
}

.close-btn:hover {
  background: var(--bg-subtle);
  color: var(--text-primary);
}

.modal-body {
  padding: 1.5rem;
  overflow-y: auto;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.detail-media {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  flex-shrink: 0; /* CRITICAL: Never shrink regardless of description length */
  min-height: 220px;
  max-height: 420px;
  border-radius: var(--radius-lg);
  overflow: hidden;
  background: var(--bg-subtle);
  box-shadow: var(--shadow-sm);
  cursor: zoom-in;
  border: 1px solid var(--border-light);
}

.detail-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  transition: transform 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

.detail-media:hover .detail-img {
  transform: scale(1.025);
}

.media-hover-overlay {
  position: absolute;
  inset: 0;
  background: rgba(15, 23, 42, 0.25);
  opacity: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity var(--transition-fast);
  pointer-events: none;
}

.detail-media:hover .media-hover-overlay {
  opacity: 1;
}

.zoom-pill {
  background: rgba(15, 23, 42, 0.78);
  color: white;
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  padding: 0.45rem 0.95rem;
  border-radius: var(--radius-full);
  font-size: 0.8125rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.detail-content {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.detail-title {
  font-size: 1.5rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  line-height: 1.3;
}

.detail-text {
  font-size: 1rem;
  line-height: 1.75;
  color: var(--text-secondary);
  white-space: pre-wrap;
  word-break: break-word;
}

.modal-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--border-subtle);
  background: var(--bg-card);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.footer-left {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.btn-action {
  padding: 0.55rem 0.95rem;
  border-radius: var(--radius-md);
  font-size: 0.85rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-action.edit {
  background: var(--bg-subtle);
  color: var(--text-primary);
  border: 1px solid var(--border-light);
}

.btn-action.edit:hover {
  background: var(--primary-light);
  color: var(--primary);
  border-color: var(--primary);
}

.btn-action.delete {
  background: var(--bg-subtle);
  color: var(--text-primary);
  border: 1px solid var(--border-light);
}

.btn-action.delete:hover {
  background: var(--danger-light);
  color: var(--danger);
  border-color: var(--danger);
}

.btn-close-main {
  background: var(--primary);
  color: #ffffff;
  padding: 0.55rem 1.25rem;
  border-radius: var(--radius-md);
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-close-main:hover {
  background: var(--primary-hover);
}

@media (max-width: 640px) {
  .modal-card {
    max-height: 95vh;
  }
  .modal-body {
    padding: 1rem;
    gap: 1rem;
  }
  .detail-media {
    aspect-ratio: 16 / 9;
    min-height: 180px;
    border-radius: var(--radius-md);
  }
  .media-hover-overlay {
    opacity: 1;
    background: transparent;
    align-items: flex-end;
    justify-content: flex-end;
    padding: 0.65rem;
  }
  .zoom-pill {
    padding: 0.35rem 0.65rem;
    font-size: 0.75rem;
    background: rgba(15, 23, 42, 0.85);
  }
  .detail-title {
    font-size: 1.25rem;
  }
}
</style>

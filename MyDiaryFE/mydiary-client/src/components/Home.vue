<template>
  <div class="diary-app">
    <!-- Top Header -->
    <header class="app-header">
      <div class="header-container">
        <!-- Brand / Title -->
        <div class="brand">
          <div class="brand-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path>
              <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path>
              <line x1="8" y1="7" x2="16" y2="7"></line>
              <line x1="8" y1="11" x2="14" y2="11"></line>
            </svg>
          </div>
          <div class="brand-text">
            <h1 class="brand-title">My Diary</h1>
            <span class="brand-subtitle">{{ todayFormatted }} • {{ entries.length }} khoảnh khắc</span>
          </div>
        </div>

        <!-- Search Bar -->
        <div class="search-box">
          <svg class="search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8"></circle>
            <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm kiếm theo tiêu đề hoặc nội dung..."
            class="search-input"
          />
          <button v-if="searchQuery" class="clear-search-btn" @click="searchQuery = ''" title="Xóa tìm kiếm">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <!-- Action Controls -->
        <div class="header-actions">
          <!-- Theme Switcher Button -->
          <button
            class="btn-theme"
            @click="toggleTheme"
            :title="isDark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối'"
            aria-label="Toggle theme"
          >
            <svg v-if="isDark" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="5"></circle>
              <line x1="12" y1="1" x2="12" y2="3"></line>
              <line x1="12" y1="21" x2="12" y2="23"></line>
              <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
              <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
              <line x1="1" y1="12" x2="3" y2="12"></line>
              <line x1="21" y1="12" x2="23" y2="12"></line>
              <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
              <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
            </svg>
            <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
            </svg>
          </button>

          <button class="btn-refresh" @click="load" :disabled="loading" title="Làm mới">
            <svg :class="{ 'spin-icon': loading }" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="23 4 23 10 17 10"></polyline>
              <polyline points="1 20 1 14 7 14"></polyline>
              <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
            </svg>
            <span class="action-text">Làm mới</span>
          </button>

          <button class="btn-add-moment" @click="openAdd">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"></line>
              <line x1="5" y1="12" x2="19" y2="12"></line>
            </svg>
            <span>Viết nhật ký</span>
          </button>
        </div>
      </div>

      <!-- Filter Tabs by Emotion -->
      <div class="filter-strip">
        <div class="filter-container">
          <button
            class="filter-pill"
            :class="{ active: selectedEmotionFilter === null }"
            @click="selectedEmotionFilter = null"
          >
            Tất cả ({{ entries.length }})
          </button>
          <button
            v-for="filter in emotionFilters"
            :key="filter.value"
            class="filter-pill"
            :class="{ active: selectedEmotionFilter === filter.value }"
            @click="selectedEmotionFilter = filter.value"
          >
            <span>{{ filter.icon }}</span>
            <span>{{ filter.label }}</span>
            <span class="filter-count">({{ getEmotionCount(filter.value) }})</span>
          </button>
        </div>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="main-content">
      <!-- Error Banner -->
      <div v-if="error" class="error-card">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"></circle>
          <line x1="12" y1="8" x2="12" y2="12"></line>
          <line x1="12" y1="16" x2="12.01" y2="16"></line>
        </svg>
        <div class="error-content">
          <strong>Đã xảy ra lỗi tải dữ liệu</strong>
          <p>{{ error }}</p>
        </div>
        <button class="retry-btn" @click="load">Thử lại</button>
      </div>

      <!-- Skeleton Loading Grid -->
      <div v-if="loading && entries.length === 0" class="diary-grid">
        <div v-for="n in 8" :key="n" class="skeleton-card">
          <div class="skeleton-thumb skeleton-shimmer"></div>
          <div class="skeleton-body">
            <div class="skeleton-meta skeleton-shimmer"></div>
            <div class="skeleton-title skeleton-shimmer"></div>
            <div class="skeleton-text skeleton-shimmer"></div>
            <div class="skeleton-text short skeleton-shimmer"></div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredEntries.length === 0 && !loading" class="empty-state">
        <div class="empty-icon-wrap">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <polyline points="10 9 9 9 8 9"></polyline>
          </svg>
        </div>
        <h3 class="empty-title">
          {{ searchQuery || selectedEmotionFilter !== null ? 'Không tìm thấy nhật ký phù hợp' : 'Chưa có nhật ký nào' }}
        </h3>
        <p class="empty-desc">
          {{ searchQuery || selectedEmotionFilter !== null
            ? 'Hãy thử tìm kiếm với từ khóa khác hoặc xóa bộ lọc cảm xúc.'
            : 'Hãy bắt đầu lưu giữ những kỷ niệm, suy nghĩ và cảm xúc đáng nhớ của bạn ngay hôm nay.' }}
        </p>
        <div class="empty-actions">
          <button v-if="searchQuery || selectedEmotionFilter !== null" class="btn-clear-filters" @click="clearFilters">
            Xóa bộ lọc
          </button>
          <button class="btn-create-first" @click="openAdd">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="12" y1="5" x2="12" y2="19"></line>
              <line x1="5" y1="12" x2="19" y2="12"></line>
            </svg>
            <span>Viết bài đầu tiên</span>
          </button>
        </div>
      </div>

      <!-- Diary Moments Grid -->
      <div v-else class="grid-section">
        <!-- Section Bar (Count & Sort Order) -->
        <div class="section-bar">
          <div class="section-stats">
            <span class="count-badge">{{ filteredEntries.length }} khoảnh khắc</span>
            <span v-if="searchQuery" class="filter-indicator">• Tìm kiếm: "{{ searchQuery }}"</span>
            <span v-if="selectedEmotionFilter !== null" class="filter-indicator">• Cảm xúc: {{ getEmotionTitle(selectedEmotionFilter) }}</span>
          </div>

          <div class="sort-box">
            <span class="sort-title">Thứ tự ngày:</span>
            <div class="sort-toggle-group">
              <button
                type="button"
                class="sort-toggle-btn"
                :class="{ active: sortOrder === 'desc' }"
                @click="sortOrder = 'desc'"
                title="Mới nhất trước (Giảm dần theo thời gian)"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="12" y1="5" x2="12" y2="19"></line>
                  <polyline points="19 12 12 19 5 12"></polyline>
                </svg>
                <span>Mới nhất (Giảm dần)</span>
              </button>
              <button
                type="button"
                class="sort-toggle-btn"
                :class="{ active: sortOrder === 'asc' }"
                @click="sortOrder = 'asc'"
                title="Cũ nhất trước (Tăng dần theo thời gian)"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="12" y1="19" x2="12" y2="5"></line>
                  <polyline points="5 12 12 5 19 12"></polyline>
                </svg>
                <span>Cũ nhất (Tăng dần)</span>
              </button>
            </div>
          </div>
        </div>

        <div class="diary-grid">
          <article
            v-for="(entry, i) in pagedEntries"
            :key="getEntryKey(entry, startIndex + i)"
            class="diary-card"
            @click="openDetail(entry, startIndex + i)"
          >
            <!-- Card Thumbnail / Visual Area -->
            <div class="card-media">
              <!-- Emotion Badge Pill -->
              <div
                class="emotion-pill"
                :class="getEmotionClass(entry.Emotion ?? entry.emotion)"
                :title="getEmotionTitle(entry.Emotion ?? entry.emotion)"
              >
                <span class="emotion-emoji">{{ getEmotionIcon(entry.Emotion ?? entry.emotion) }}</span>
                <span class="emotion-name">{{ getEmotionTitle(entry.Emotion ?? entry.emotion) }}</span>
              </div>

              <!-- Quick Action Hover Controls -->
              <div class="card-quick-actions" @click.stop>
                <button
                  class="quick-action-btn edit"
                  title="Chỉnh sửa bài viết"
                  @click.stop="openEdit(entry)"
                >
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                  </svg>
                </button>
                <button
                  class="quick-action-btn delete"
                  title="Xóa bài viết"
                  @click.stop="requestDelete(entry)"
                >
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="3 6 5 6 21 6"></polyline>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                  </svg>
                </button>
              </div>

              <!-- Actual Image if present -->
              <img
                v-if="getImageUrl(entry, startIndex + i)"
                :src="getImageUrl(entry, startIndex + i)"
                alt="Hình ảnh khoảnh khắc"
                class="card-img"
                loading="lazy"
              />

              <!-- Abstract Minimalist Placeholder if no image -->
              <div v-else class="card-placeholder" :class="getPlaceholderTheme(entry.Emotion ?? entry.emotion)">
                <div class="placeholder-pattern"></div>
                <div class="placeholder-icon">
                  <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                    <path d="M12 20h9"></path>
                    <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path>
                  </svg>
                </div>
                <span class="placeholder-tag">Khoảnh khắc suy ngẫm</span>
              </div>
            </div>

            <!-- Card Content -->
            <div class="card-content">
              <div class="card-meta">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                  <line x1="16" y1="2" x2="16" y2="6"></line>
                  <line x1="8" y1="2" x2="8" y2="6"></line>
                  <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
                <span>{{ formatDate(entry.MomentAt ?? entry.momentAt) }}</span>
              </div>

              <h2 class="card-title" :title="entry.Title ?? entry.title ?? 'Không tiêu đề'">
                {{ entry.Title ?? entry.title ?? 'Không tiêu đề' }}
              </h2>

              <p class="card-desc">
                {{ entry.Description ?? entry.description ?? 'Không có nội dung mô tả.' }}
              </p>
            </div>
          </article>
        </div>

        <!-- Modern Pagination Bar -->
        <div class="pagination-bar" v-if="totalPages > 1">
          <button
            class="page-btn nav-btn"
            @click="prevPage"
            :disabled="currentPage === 1"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="15 18 9 12 15 6"></polyline>
            </svg>
            <span>Trước</span>
          </button>

          <div class="page-indicator">
            Trang <span class="page-current">{{ currentPage }}</span> / {{ totalPages }}
          </div>

          <button
            class="page-btn nav-btn"
            @click="nextPage"
            :disabled="currentPage === totalPages"
          >
            <span>Sau</span>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="9 18 15 12 9 6"></polyline>
            </svg>
          </button>
        </div>
      </div>
    </main>

    <!-- Modals -->
    <DiaryForm
      v-if="showAdd"
      @saved="onSaved"
      @close="showAdd = false"
    />

    <DiaryForm
      v-if="showEdit"
      :initial="editingEntry"
      :isEdit="true"
      @saved="onSaved"
      @close="closeEdit"
    />

    <!-- Reading Detail Modal -->
    <DiaryDetailModal
      v-if="selectedDetailEntry"
      :entry="selectedDetailEntry"
      :image-url="selectedDetailImageUrl"
      @close="closeDetail"
      @edit="onDetailEdit"
      @delete="onDetailDelete"
    />

    <!-- Custom In-App Delete Confirmation Modal -->
    <div v-if="entryToDelete" class="confirm-overlay">
      <div class="confirm-card" role="alertdialog">
        <div class="confirm-icon">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="3 6 5 6 21 6"></polyline>
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            <line x1="10" y1="11" x2="10" y2="17"></line>
            <line x1="14" y1="11" x2="14" y2="17"></line>
          </svg>
        </div>
        <h3 class="confirm-title">Xóa khoảnh khắc này?</h3>
        <p class="confirm-message">
          Bạn có chắc chắn muốn xóa bài viết "<strong>{{ entryToDelete.Title ?? entryToDelete.title ?? 'Không tiêu đề' }}</strong>"?
          Hành động này không thể hoàn tác.
        </p>
        <div class="confirm-actions">
          <button class="btn-cancel" @click="entryToDelete = null">Giữ lại</button>
          <button class="btn-confirm-delete" :disabled="deleting" @click="executeDelete">
            {{ deleting ? 'Đang xóa...' : 'Xác nhận xóa' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch, nextTick, reactive } from 'vue'
import DiaryForm from './DiaryForm.vue'
import DiaryDetailModal from './DiaryDetailModal.vue'
import { getApiDiaryMoments, deleteApiDiaryMomentsById } from '../api/generated'

const isDark = ref(false)
const entries = ref<Array<any>>([])
const loading = ref(false)
const error = ref('')
const searchQuery = ref('')
const selectedEmotionFilter = ref<number | null>(null)
const sortOrder = ref<'desc' | 'asc'>('desc')

const imageMap = reactive<Record<string, string | null>>({})
const objectUrls: Record<string, string> = {}

const showAdd = ref(false)
const showEdit = ref(false)
const editingEntry = ref<any>(null)
const selectedDetailEntry = ref<any>(null)
const selectedDetailImageUrl = ref<string | null>(null)
const entryToDelete = ref<any>(null)
const deleting = ref(false)

const perPage = 8
const currentPage = ref(1)

const todayFormatted = computed(() => {
  const now = new Date()
  return now.toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' })
})

const emotionFilters = [
  { value: 1, label: 'Vui vẻ', icon: '😊' },
  { value: 3, label: 'Hào hứng', icon: '🤩' },
  { value: 6, label: 'Biết ơn', icon: '🙏' },
  { value: 0, label: 'Bình thản', icon: '😐' },
  { value: 5, label: 'Lo âu', icon: '😟' },
  { value: 2, label: 'Buồn bã', icon: '😢' },
  { value: 4, label: 'Giận dữ', icon: '😠' },
]

function getEmotionCount(val: number) {
  return entries.value.filter((e) => {
    const em = Number(e.Emotion ?? e.emotion)
    return em === val
  }).length
}

const filteredEntries = computed(() => {
  let list = entries.value.slice()

  // Filter by emotion
  if (selectedEmotionFilter.value !== null) {
    list = list.filter((e) => {
      const em = Number(e.Emotion ?? e.emotion)
      return em === selectedEmotionFilter.value
    })
  }

  // Filter by search query
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter((e) => {
      const title = String(e.Title ?? e.title ?? '').toLowerCase()
      const desc = String(e.Description ?? e.description ?? '').toLowerCase()
      return title.includes(q) || desc.includes(q)
    })
  }

  // Sort by date (MomentAt or createdAt)
  list.sort((a: any, b: any) => {
    const ta = Date.parse(String(a?.MomentAt ?? a?.momentAt ?? a?.moment_at ?? a?.createdAt ?? a?.created_at ?? '')) || 0
    const tb = Date.parse(String(b?.MomentAt ?? b?.momentAt ?? b?.moment_at ?? b?.createdAt ?? b?.created_at ?? '')) || 0
    return sortOrder.value === 'desc' ? tb - ta : ta - tb
  })

  return list
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredEntries.value.length / perPage)))
const startIndex = computed(() => (currentPage.value - 1) * perPage)
const pagedEntries = computed(() => filteredEntries.value.slice(startIndex.value, startIndex.value + perPage))

watch([searchQuery, selectedEmotionFilter, sortOrder], async () => {
  currentPage.value = 1
  await nextTick()
  const start = startIndex.value
  const end = Math.min(filteredEntries.value.length, start + perPage)
  loadImagesForRange(start, end).catch(() => {})
})

function clearFilters() {
  searchQuery.value = ''
  selectedEmotionFilter.value = null
}

function getEntryKey(e: any, i: number) {
  return String(e?.Id ?? e?.id ?? i)
}

function getImageUrl(e: any, i: number): string | undefined {
  const key = getEntryKey(e, i)
  const v = imageMap[key]
  return v ?? undefined
}

function formatDate(d: unknown) {
  if (!d) return ''
  try {
    const dt = new Date(String(d))
    return dt.toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return String(d)
  }
}

function getEmotionIcon(v: any) {
  const n = Number(v)
  if (Number.isNaN(n)) return '✨'
  switch (n) {
    case 0: return '😐'
    case 1: return '😊'
    case 2: return '😢'
    case 3: return '🤩'
    case 4: return '😠'
    case 5: return '😟'
    case 6: return '🙏'
    default: return '✨'
  }
}

function getEmotionTitle(v: any) {
  const n = Number(v)
  if (Number.isNaN(n)) return 'Khoảnh khắc'
  switch (n) {
    case 0: return 'Bình thản'
    case 1: return 'Vui vẻ'
    case 2: return 'Buồn bã'
    case 3: return 'Hào hứng'
    case 4: return 'Giận dữ'
    case 5: return 'Lo âu'
    case 6: return 'Biết ơn'
    default: return 'Khoảnh khắc'
  }
}

function getEmotionClass(v: any) {
  const n = Number(v)
  if (Number.isNaN(n)) return 'neutral'
  switch (n) {
    case 0: return 'neutral'
    case 1: return 'happy'
    case 2: return 'sad'
    case 3: return 'excited'
    case 4: return 'angry'
    case 5: return 'anxious'
    case 6: return 'grateful'
    default: return 'neutral'
  }
}

function getPlaceholderTheme(v: any) {
  const n = Number(v)
  switch (n) {
    case 1: return 'theme-happy'
    case 3: return 'theme-excited'
    case 6: return 'theme-grateful'
    case 2: return 'theme-sad'
    case 4: return 'theme-angry'
    case 5: return 'theme-anxious'
    default: return 'theme-neutral'
  }
}

async function load() {
  loading.value = true
  error.value = ''

  try {
    const res: any = await getApiDiaryMoments()
    const data = res && 'data' in res ? res.data : res
    let list = Array.isArray(data) ? data : (Array.isArray(Object.values(data ?? {})) ? Object.values(data ?? {}) : [])
    if (!Array.isArray(list)) list = [data]

    list = (list || []).slice().sort((a: any, b: any) => {
      const ta = Date.parse(String(a?.MomentAt ?? a?.momentAt ?? a?.moment_at ?? a?.createdAt ?? a?.created_at ?? '')) || 0
      const tb = Date.parse(String(b?.MomentAt ?? b?.momentAt ?? b?.moment_at ?? b?.createdAt ?? b?.created_at ?? '')) || 0
      return tb - ta
    })
    entries.value = list || []

    const keepKeys = new Set<string>()
    for (let i = 0; i < entries.value.length; i++) {
      keepKeys.add(getEntryKey(entries.value[i], i))
    }
    for (const k in objectUrls) {
      if (!keepKeys.has(k)) {
        try { URL.revokeObjectURL(objectUrls[k]) } catch {}
        delete objectUrls[k]
      }
    }
    for (const k in imageMap) {
      if (!keepKeys.has(k)) delete imageMap[k]
    }

    if (currentPage.value > totalPages.value) currentPage.value = totalPages.value || 1

    const start = startIndex.value
    const end = Math.min(filteredEntries.value.length, start + perPage)
    await loadImagesForRange(start, end)
  } catch (err: any) {
    console.error('load failed', err)
    error.value = err?.message ?? String(err)
  } finally {
    loading.value = false
  }
}

async function loadImagesForRange(start: number, end: number) {
  const pageEntries = filteredEntries.value.slice(start, end)
  for (let idx = 0; idx < pageEntries.length; idx++) {
    const e = pageEntries[idx]
    const absIndex = start + idx
    const key = getEntryKey(e, absIndex)

    if (Object.prototype.hasOwnProperty.call(imageMap, key)) continue

    const hasImage = e?.hasImage ?? e?.HasImage ?? e?.has_image ?? e?.Hasimage
    if (hasImage === false) {
      imageMap[key] = null
      continue
    }

    const possibleUrl = e?.ImageUrl ?? e?.imageUrl ?? e?.Image ?? e?.image
    if (possibleUrl && typeof possibleUrl === 'string') {
      imageMap[key] = possibleUrl
      continue
    }

    const id = e?.Id ?? e?.id
    if (id == null) {
      imageMap[key] = null
      continue
    }

    try {
      await fetchImageById(Number(id), key)
    } catch {
      imageMap[key] = null
    }
  }
}

async function fetchImageById(id: number, key?: string) {
  const k = key ?? String(id)
  const apiBase = (import.meta.env.VITE_API_BASE_URL as string)?.replace(/\/+$/, '') 
    ?? (import.meta.env.DEV ? 'http://localhost:5224' : '')
  const imgUrl = `${apiBase}/api/DiaryMoments/${id}/image?t=${Date.now()}`
  const resp = await fetch(imgUrl, { cache: 'no-store' })
  if (resp.ok) {
    const blob = await resp.blob()
    if (blob) {
      if (objectUrls[k]) {
        try { URL.revokeObjectURL(objectUrls[k]) } catch {}
        delete objectUrls[k]
      }
      const objectUrl = URL.createObjectURL(blob)
      objectUrls[k] = objectUrl
      imageMap[k] = objectUrl
      return
    }
    imageMap[k] = null
    return
  }
  if (resp.status === 404) {
    imageMap[k] = null
    return
  }
  throw new Error(`Image fetch failed: ${resp.status}`)
}

watch(currentPage, async () => {
  await nextTick()
  const start = startIndex.value
  const end = Math.min(filteredEntries.value.length, start + perPage)
  loadImagesForRange(start, end).catch(() => {})
})

function openAdd() {
  showAdd.value = true
}

function openEdit(e: any) {
  editingEntry.value = e
  showEdit.value = true
}

function closeEdit() {
  showEdit.value = false
  editingEntry.value = null
}

function requestDelete(e: any) {
  entryToDelete.value = e
}

async function executeDelete() {
  if (!entryToDelete.value) return
  deleting.value = true
  try {
    const id = Number(entryToDelete.value.Id ?? entryToDelete.value.id)
    await deleteApiDiaryMomentsById({ path: { id }, throwOnError: true })
    entryToDelete.value = null
    await load()
  } catch (err: any) {
    alert('Không thể xóa nhật ký: ' + (err?.message ?? String(err)))
  } finally {
    deleting.value = false
  }
}

function prevPage() {
  if (currentPage.value > 1) {
    currentPage.value -= 1
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value += 1
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

async function onSaved(info?: { id?: number, imageChanged?: boolean, isNew?: boolean }) {
  if (info?.isNew) {
    currentPage.value = 1
    await load()
  } else {
    await load()
  }

  if (info?.imageChanged && info.id != null) {
    const key = String(info.id)
    if (objectUrls[key]) {
      try { URL.revokeObjectURL(objectUrls[key]) } catch {}
      delete objectUrls[key]
    }
    delete imageMap[key]
    await fetchImageById(Number(info.id), key)
  }
}

function initTheme() {
  isDark.value = document.documentElement.classList.contains('dark')
}

function toggleTheme() {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('mydiary-theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('mydiary-theme', 'light')
  }
}

function openDetail(entry: any, index: number) {
  selectedDetailEntry.value = entry
  selectedDetailImageUrl.value = getImageUrl(entry, index) ?? null
}

function closeDetail() {
  selectedDetailEntry.value = null
  selectedDetailImageUrl.value = null
}

function onDetailEdit(entry: any) {
  closeDetail()
  openEdit(entry)
}

function onDetailDelete(entry: any) {
  closeDetail()
  requestDelete(entry)
}

onMounted(() => {
  initTheme()
  load()
})

onBeforeUnmount(() => {
  for (const k in objectUrls) {
    try { URL.revokeObjectURL(objectUrls[k]) } catch {}
  }
})
</script>

<style scoped>
.diary-app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-canvas);
}

/* Header & Navigation Bar */
.app-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--bg-header);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--border-light);
}

.header-container {
  max-width: 1280px;
  margin: 0 auto;
  padding: 1rem 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  text-decoration: none;
}

.brand-icon {
  width: 42px;
  height: 42px;
  border-radius: var(--radius-md);
  background: linear-gradient(135deg, var(--primary) 0%, #3730a3 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(79, 70, 229, 0.3);
}

.brand-text {
  display: flex;
  flex-direction: column;
}

.brand-title {
  font-size: 1.25rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: var(--text-primary);
  line-height: 1.2;
}

.brand-subtitle {
  font-size: 0.775rem;
  color: var(--text-muted);
  font-weight: 500;
}

/* Search Box */
.search-box {
  position: relative;
  flex: 1;
  max-width: 440px;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 0.85rem;
  color: var(--text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 0.6rem 2.2rem 0.6rem 2.4rem;
  background: var(--bg-subtle);
  color: var(--text-primary);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-full);
  font-size: 0.875rem;
  transition: all var(--transition-fast);
}

.search-input:focus {
  background: var(--bg-card);
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-focus);
}

.clear-search-btn {
  position: absolute;
  right: 0.6rem;
  background: transparent;
  border: none;
  color: var(--text-muted);
  padding: 0.25rem;
  border-radius: var(--radius-full);
  cursor: pointer;
}

.clear-search-btn:hover {
  color: var(--text-primary);
}

/* Header Actions */
.header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.btn-theme {
  background: var(--bg-subtle);
  color: var(--text-secondary);
  border: 1px solid var(--border-light);
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-theme:hover {
  background: var(--bg-card);
  color: var(--text-primary);
  border-color: var(--border-hover);
}

.btn-refresh {
  background: var(--bg-subtle);
  color: var(--text-secondary);
  border: 1px solid var(--border-light);
  padding: 0.55rem 0.95rem;
  border-radius: var(--radius-md);
}

.btn-refresh:hover:not(:disabled) {
  background: var(--bg-card);
  color: var(--text-primary);
  border-color: var(--border-hover);
}

.btn-add-moment {
  background: var(--primary);
  color: #ffffff;
  padding: 0.6rem 1.15rem;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.btn-add-moment:hover {
  background: var(--primary-hover);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

/* Filter Strip */
.filter-strip {
  border-top: 1px solid var(--border-subtle);
  background: var(--bg-filter);
  overflow-x: auto;
}

.filter-container {
  max-width: 1280px;
  margin: 0 auto;
  padding: 0.5rem 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  white-space: nowrap;
}

.filter-pill {
  padding: 0.35rem 0.75rem;
  border-radius: var(--radius-full);
  border: 1px solid var(--border-light);
  background: var(--bg-card);
  color: var(--text-secondary);
  font-size: 0.8125rem;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.filter-pill:hover {
  border-color: var(--border-hover);
  background: var(--bg-subtle);
}

.filter-pill.active {
  background: var(--primary);
  color: #ffffff;
  border-color: var(--primary);
}

.filter-pill.active .filter-count {
  color: rgba(255, 255, 255, 0.85);
}

.filter-count {
  font-size: 0.75rem;
  color: var(--text-muted);
}

/* Main Content Area */
.main-content {
  flex: 1;
  max-width: 1280px;
  width: 100%;
  margin: 0 auto;
  padding: 2rem 1.5rem 4rem;
  box-sizing: border-box;
}

/* Error Card */
.error-card {
  background: var(--danger-light);
  border: 1px solid #fecaca;
  color: var(--danger);
  padding: 1rem 1.25rem;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.error-content {
  flex: 1;
}

.error-content strong {
  display: block;
  font-size: 0.9375rem;
}

.error-content p {
  font-size: 0.875rem;
  color: #991b1b;
}

.retry-btn {
  background: white;
  color: var(--danger);
  border: 1px solid #fca5a5;
  padding: 0.4rem 0.85rem;
  border-radius: var(--radius-md);
}

/* Section Bar (Count & Sort Controls) */
.section-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.section-stats {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.count-badge {
  font-weight: 700;
  color: var(--text-primary);
  background: var(--bg-card);
  padding: 0.35rem 0.75rem;
  border-radius: var(--radius-full);
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow-xs);
}

.filter-indicator {
  color: var(--primary);
  font-weight: 600;
  font-size: 0.8125rem;
}

.sort-box {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.sort-title {
  font-size: 0.8125rem;
  color: var(--text-muted);
  font-weight: 600;
}

.sort-toggle-group {
  display: inline-flex;
  background: var(--bg-subtle);
  padding: 3px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-light);
}

.sort-toggle-btn {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-size: 0.8125rem;
  font-weight: 600;
  padding: 0.4rem 0.85rem;
  border-radius: calc(var(--radius-md) - 2px);
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.sort-toggle-btn:hover {
  color: var(--text-primary);
}

.sort-toggle-btn.active {
  background: var(--bg-card);
  color: var(--text-primary);
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.08);
}

/* Grid Layout */
.diary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
}

/* Diary Card */
.diary-card {
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-xl);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: all var(--transition-normal);
  box-shadow: var(--shadow-xs);
  position: relative;
  cursor: pointer;
}

.diary-card:hover {
  transform: translateY(-4px);
  border-color: var(--border-hover);
  box-shadow: var(--shadow-lg);
}

/* Card Media (16:10 aspect ratio) */
.card-media {
  position: relative;
  width: 100%;
  padding-top: 60%;
  overflow: hidden;
  background-color: var(--bg-subtle);
}

.card-img {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.diary-card:hover .card-img {
  transform: scale(1.04);
}

/* Abstract Minimalist Placeholder */
.card-placeholder {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  overflow: hidden;
}

.card-placeholder.theme-happy {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  color: #16a34a;
}
.card-placeholder.theme-excited {
  background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
  color: #d97706;
}
.card-placeholder.theme-grateful {
  background: linear-gradient(135deg, #faf5ff 0%, #ede9fe 100%);
  color: #7c3aed;
}
.card-placeholder.theme-sad {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  color: #0284c7;
}
.card-placeholder.theme-angry {
  background: linear-gradient(135deg, #fff1f2 0%, #ffe4e6 100%);
  color: #e11d48;
}
.card-placeholder.theme-anxious {
  background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%);
  color: #ea580c;
}
.card-placeholder.theme-neutral {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  color: #64748b;
}

.placeholder-icon {
  opacity: 0.85;
}

.placeholder-tag {
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  opacity: 0.7;
}

/* Emotion Pill Badge (Frosted Glass) */
.emotion-pill {
  position: absolute;
  top: 0.75rem;
  left: 0.75rem;
  z-index: 10;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.3rem 0.65rem;
  border-radius: var(--radius-full);
  background: var(--bg-card);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.12);
  border: 1px solid var(--border-light);
  font-size: 0.775rem;
  font-weight: 600;
  color: var(--text-primary);
}

.emotion-emoji {
  font-size: 0.95rem;
  line-height: 1;
}

/* Quick Action Hover Controls */
.card-quick-actions {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  z-index: 10;
  display: flex;
  gap: 0.4rem;
  opacity: 0;
  transform: translateY(-4px);
  transition: all var(--transition-fast);
}

.diary-card:hover .card-quick-actions {
  opacity: 1;
  transform: translateY(0);
}

.quick-action-btn {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-full);
  background: var(--bg-card);
  backdrop-filter: blur(8px);
  border: 1px solid var(--border-light);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-secondary);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.12);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.quick-action-btn:hover {
  transform: scale(1.08);
}

.quick-action-btn.edit:hover {
  color: var(--primary);
  background: var(--primary-light);
  border-color: var(--primary);
}

.quick-action-btn.delete:hover {
  color: var(--danger);
  background: var(--danger-light);
  border-color: var(--danger);
}

/* Card Content Area */
.card-content {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.775rem;
  color: var(--text-muted);
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.card-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1.35;
  margin-bottom: 0.5rem;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-desc {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Skeleton Loading */
.skeleton-card {
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-xl);
  overflow: hidden;
}

.skeleton-thumb {
  width: 100%;
  padding-top: 60%;
}

.skeleton-body {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.skeleton-meta {
  width: 40%;
  height: 12px;
  border-radius: var(--radius-sm);
}

.skeleton-title {
  width: 80%;
  height: 18px;
  border-radius: var(--radius-sm);
}

.skeleton-text {
  width: 100%;
  height: 14px;
  border-radius: var(--radius-sm);
}

.skeleton-text.short {
  width: 60%;
}

/* Empty State */
.empty-state {
  max-width: 460px;
  margin: 4rem auto;
  text-align: center;
  padding: 2.5rem 1.5rem;
  background: var(--bg-card);
  border: 1px dashed var(--border-hover);
  border-radius: var(--radius-xl);
}

.empty-icon-wrap {
  width: 72px;
  height: 72px;
  margin: 0 auto 1.25rem;
  background: var(--bg-subtle);
  color: var(--text-muted);
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
}

.empty-title {
  font-size: 1.2rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.empty-desc {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.empty-actions {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
}

.btn-clear-filters {
  background: var(--bg-subtle);
  border: 1px solid var(--border-light);
  color: var(--text-secondary);
  padding: 0.6rem 1rem;
}

.btn-create-first {
  background: var(--primary);
  color: white;
  padding: 0.6rem 1.25rem;
}

/* Modern Pagination */
.pagination-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 3rem;
}

.page-btn {
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  color: var(--text-secondary);
  padding: 0.55rem 1rem;
  border-radius: var(--radius-full);
  box-shadow: var(--shadow-xs);
}

.page-btn:hover:not(:disabled) {
  background: var(--bg-subtle);
  color: var(--text-primary);
  border-color: var(--border-hover);
}

.page-indicator {
  font-size: 0.875rem;
  color: var(--text-muted);
  font-weight: 500;
}

.page-current {
  color: var(--text-primary);
  font-weight: 700;
}

/* Delete Confirmation Modal */
.confirm-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 1100;
  animation: fadeIn 0.2s ease-out;
}

.confirm-card {
  background: var(--bg-card);
  max-width: 420px;
  width: 100%;
  border-radius: var(--radius-xl);
  padding: 1.75rem;
  text-align: center;
  box-shadow: var(--shadow-modal);
  border: 1px solid var(--border-light);
  animation: modalScaleIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.confirm-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-full);
  background: var(--danger-light);
  color: var(--danger);
  margin: 0 auto 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.confirm-title {
  font-size: 1.2rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.confirm-message {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.5;
  margin-bottom: 1.5rem;
}

.confirm-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
}

.btn-cancel {
  background: var(--bg-subtle);
  border: 1px solid var(--border-light);
  color: var(--text-secondary);
  padding: 0.65rem 1.25rem;
  flex: 1;
}

.btn-confirm-delete {
  background: var(--danger);
  color: white;
  padding: 0.65rem 1.25rem;
  flex: 1;
}

.btn-confirm-delete:hover:not(:disabled) {
  background: var(--danger-hover);
}

/* Spinner Icon */
.spin-icon {
  animation: spin 1s linear infinite;
}

/* Responsive Breakpoints */
@media (max-width: 900px) {
  .header-container {
    flex-wrap: wrap;
  }
  .search-box {
    order: 3;
    max-width: 100%;
    width: 100%;
  }
}

@media (max-width: 640px) {
  .header-container {
    padding: 0.875rem 1rem;
  }
  .action-text {
    display: none;
  }
  .btn-refresh {
    padding: 0.55rem;
  }
  .main-content {
    padding: 1.25rem 1rem 3rem;
  }
  .diary-grid {
    grid-template-columns: 1fr;
  }
}
</style>

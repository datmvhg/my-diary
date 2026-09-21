<template>
  <div class="diary-list">
    <h2>All Diary Entries</h2>

    <div v-if="loading">Loading...</div>
    <div v-if="error" style="color:red">{{ error }}</div>

    <div v-if="entries.length === 0 && !loading">No entries yet.</div>

    <ul>
      <li v-for="(entry, index) in entries" :key="getEntryKey(entry, index)" class="entry">
        <div class="entry-header">
          <h3>{{ entry.Title ?? entry.title ?? 'Untitled' }}</h3>
          <div class="meta">{{ formatDate(entry.MomentAt ?? entry.momentAt ?? entry.moment_at) }}</div>
        </div>
        <div class="entry-body">
          <p>{{ entry.Description ?? entry.description ?? '' }}</p>
          <div v-if="getImageUrl(entry, index)" class="image-wrap">
            <img :src="getImageUrl(entry, index)" alt="entry image" />
          </div>
        </div>
        <div class="entry-footer">Emotion: {{ entry.Emotion ?? entry.emotion ?? '-' }}</div>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, reactive } from 'vue'
import { getApiDiaryMoments } from '../api/generated'

const entries = ref<Array<any>>([])
const loading = ref(false)
const error = ref('')

// Map of key -> object URL for loaded images
const imageMap = reactive<Record<string, string | null>>({})
const objectUrls: Record<string, string> = {}

function getEntryKey(entry: any, index: number) {
  // Prefer numeric id properties if present
  return String(entry?.Id ?? entry?.id ?? entry?.Id ?? index)
}

function getImageUrl(entry: any, index: number): string | undefined {
  const key = getEntryKey(entry, index)
  const v = imageMap[key]
  return v ?? undefined
}

function formatDate(d: unknown) {
  if (!d) return ''
  try {
    const dt = new Date(String(d))
    return dt.toLocaleString()
  } catch {
    return String(d)
  }
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const res: any = await getApiDiaryMoments()
    // SDK returns { data, error } when using default responseStyle
    const data = res && 'data' in res ? res.data : res
    if (!data) {
      entries.value = []
      return
    }

    // Expecting array; if it's wrapped, try to find array inside
    let list = Array.isArray(data) ? data : Object.values(data ?? {})
    if (!Array.isArray(list)) list = [data]
    // sort newest first by MomentAt or createdAt
    list = (list || []).slice().sort((a: any, b: any) => {
      const ta = Date.parse(String(a?.MomentAt ?? a?.momentAt ?? a?.moment_at ?? a?.createdAt ?? a?.created_at ?? '')) || 0
      const tb = Date.parse(String(b?.MomentAt ?? b?.momentAt ?? b?.moment_at ?? b?.createdAt ?? b?.created_at ?? '')) || 0
      return tb - ta
    })
    entries.value = list

    // Reconcile image cache: keep object URLs for entries still present, revoke others
    const keepKeys = new Set<string>()
    for (let i = 0; i < entries.value.length; i++) keepKeys.add(getEntryKey(entries.value[i], i))
    for (const k in objectUrls) {
      if (!keepKeys.has(k)) {
        try { URL.revokeObjectURL(objectUrls[k]) } catch {}
        delete objectUrls[k]
      }
    }
    for (const k in imageMap) {
      if (!keepKeys.has(k)) delete imageMap[k]
    }

    // For each entry, try to load image. Prefer ImageUrl/ImageName or call image endpoint by id.
    for (let i = 0; i < entries.value.length; i++) {
      const e = entries.value[i]
      const key = getEntryKey(e, i)

      // Respect explicit hasImage flag if provided by API
      const hasImage = e?.hasImage ?? e?.HasImage ?? e?.has_image ?? e?.Hasimage
      if (hasImage === false) {
        imageMap[key] = null
        continue
      }

      // If entry already has an image url field, use it
      const possibleUrl = e?.ImageUrl ?? e?.imageUrl ?? e?.Image ?? e?.image
      if (possibleUrl && typeof possibleUrl === 'string') {
        imageMap[key] = possibleUrl
        continue
      }

      // Try to get numeric id for image endpoint
      const id = e?.Id ?? e?.id
      if (typeof id === 'number' || (typeof id === 'string' && id.match(/^\d+$/))) {
        try {
          // Fetch image with cache-busting query param
          const imgUrl = `http://localhost:5224/api/DiaryMoments/${id}/image?t=${Date.now()}`
          const resp = await fetch(imgUrl, { cache: 'no-store' })
          if (resp.ok) {
            const blob = await resp.blob()
            if (blob) {
              const url = URL.createObjectURL(blob)
              objectUrls[key] = url
              imageMap[key] = url
            } else {
              imageMap[key] = null
            }
          } else if (resp.status === 404) {
            imageMap[key] = null
          } else {
            console.debug('image fetch failed', resp.status, resp.statusText)
            imageMap[key] = null
          }
        } catch (imgErr: any) {
          // mark missing on 404 and otherwise ignore
          const msg = imgErr?.message ?? String(imgErr)
          if (/404/.test(msg)) imageMap[key] = null
        }
      } else {
        imageMap[key] = null
      }
    }
  } catch (err: any) {
    console.error('Failed to load diary moments', err)
    error.value = err?.message ?? String(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  load()
})

onBeforeUnmount(() => {
  for (const k in objectUrls) {
    try { URL.revokeObjectURL(objectUrls[k]) } catch {}
  }
})
</script>

<style scoped>
.diary-list {
  max-width: 900px;
  margin: 20px auto;
}
.entry {
  border: 1px solid #e6e6e6;
  padding: 12px;
  margin-bottom: 12px;
  border-radius: 6px;
}
.entry-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}
.image-wrap img {
  max-width: 320px;
  max-height: 320px;
  display: block;
  margin-top: 8px;
}
.meta {
  color: #777;
  font-size: 0.9em;
}
</style>

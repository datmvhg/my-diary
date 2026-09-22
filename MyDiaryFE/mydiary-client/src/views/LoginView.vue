<template>
  <div class="auth-page">
    <!-- Background glowing orbs -->
    <div class="orb orb-1"></div>
    <div class="orb orb-2"></div>

    <!-- Header bar with Logo & Theme toggle -->
    <header class="auth-header">
      <div class="brand">
        <span class="brand-icon">📔</span>
        <span class="brand-title">My Diary</span>
      </div>
      <button class="theme-toggle-btn" @click="toggleTheme" :title="isDark ? 'Chế độ sáng' : 'Chế độ tối'">
        <span v-if="isDark">☀️</span>
        <span v-else>🌙</span>
      </button>
    </header>

    <!-- Main Login Card -->
    <main class="auth-main">
      <div class="auth-card">
        <div class="card-header">
          <div class="header-badge">Chào mừng trở lại</div>
          <h1 class="auth-title">Đăng nhập</h1>
          <p class="auth-subtitle">Ghi lại và mở khóa những khoảnh khắc quý giá của bạn.</p>
        </div>

        <form class="auth-form" @submit.prevent="handleLogin">
          <!-- Error alert banner -->
          <div v-if="errorMessage" class="error-banner" role="alert">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Username Input -->
          <div class="form-group">
            <label class="form-label" for="username">Tài khoản</label>
            <div class="input-wrapper">
              <span class="input-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                  <circle cx="12" cy="7" r="4"></circle>
                </svg>
              </span>
              <input
                id="username"
                v-model.trim="username"
                type="text"
                class="form-input"
                placeholder="Nhập tên tài khoản của bạn..."
                required
                autocomplete="username"
                :disabled="loading"
              />
            </div>
          </div>

          <!-- Password Input with Show/Hide -->
          <div class="form-group">
            <label class="form-label" for="password">Mật khẩu</label>
            <div class="input-wrapper">
              <span class="input-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                </svg>
              </span>
              <input
                id="password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="form-input"
                placeholder="Nhập mật khẩu..."
                required
                autocomplete="current-password"
                :disabled="loading"
              />
              <button
                type="button"
                class="eye-btn"
                @click="showPassword = !showPassword"
                :title="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                tabindex="-1"
              >
                <svg v-if="!showPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8z"></path>
                  <circle cx="12" cy="12" r="3"></circle>
                </svg>
                <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                  <line x1="1" y1="1" x2="23" y2="23"></line>
                </svg>
              </button>
            </div>
          </div>

          <!-- Submit Button -->
          <button type="submit" class="submit-btn" :disabled="loading">
            <svg v-if="loading" class="spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
            </svg>
            <span>{{ loading ? 'Đang xác thực...' : 'Đăng nhập' }}</span>
          </button>
        </form>

        <div class="auth-footer">
          <span>Chưa có tài khoản?</span>
          <router-link to="/register" class="auth-link">Đăng ký ngay</router-link>
        </div>
      </div>
    </main>

    <!-- Modal: Requires Email Verification -->
    <div v-if="showVerifyModal" class="modal-overlay" @click.self="closeVerifyModal">
      <div class="modal-card verify-modal-card" role="dialog" aria-modal="true">
        <div class="verify-modal-header">
          <div class="header-badge verify-badge">Yêu cầu xác thực</div>
          <h2 class="verify-modal-title">Xác thực Email</h2>
          <p class="verify-modal-subtitle">
            Tài khoản chưa được kích hoạt. Mã xác thực 6 số đã được gửi tới:
            <br />
            <strong class="email-highlight">{{ verifyMaskedEmail }}</strong>
          </p>
        </div>

        <div v-if="modalErrorMessage" class="error-banner" role="alert">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
          <span>{{ modalErrorMessage }}</span>
        </div>

        <div v-if="modalSuccessMessage" class="success-banner" role="status">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
            <polyline points="22 4 12 14.01 9 11.01"></polyline>
          </svg>
          <span>{{ modalSuccessMessage }}</span>
        </div>

        <form class="auth-form" @submit.prevent="handleModalVerify">
          <!-- 6-digit OTP code -->
          <div class="form-group otp-group">
            <label class="form-label text-center" for="login-otp">Nhập mã xác thực 6 chữ số</label>
            <div class="otp-input-wrapper">
              <input
                id="login-otp"
                v-model.trim="otpCode"
                type="text"
                inputmode="numeric"
                pattern="[0-9]*"
                maxlength="6"
                class="otp-input"
                placeholder="••••••"
                required
                autofocus
                autocomplete="one-time-code"
                :disabled="modalLoading"
                @input="onOtpInput"
              />
            </div>
          </div>

          <!-- Timer & Resend -->
          <div class="timer-box">
            <div class="timer-display" :class="{ 'timer-expired': countdown === 0 }">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"></circle>
                <polyline points="12 6 12 12 16 14"></polyline>
              </svg>
              <span v-if="countdown > 0">
                Hiệu lực còn lại: <strong>{{ formattedCountdown }}</strong>
              </span>
              <span v-else class="text-danger font-semibold">
                Mã xác thực đã hết hạn!
              </span>
            </div>

            <div class="timer-progress-track">
              <div class="timer-progress-bar" :style="{ width: `${(countdown / 60) * 100}%` }"></div>
            </div>

            <div class="resend-action">
              <span v-if="countdown > 0" class="resend-countdown-hint">
                Có thể gửi lại mã sau <strong>{{ countdown }}s</strong>
              </span>
              <button
                v-else
                type="button"
                class="resend-btn"
                :disabled="resendLoading"
                @click="handleModalResend"
              >
                <svg v-if="resendLoading" class="spinner" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
                </svg>
                <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="23 4 23 10 17 10"></polyline>
                  <polyline points="1 20 1 14 7 14"></polyline>
                  <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
                </svg>
                <span>{{ resendLoading ? 'Đang gửi lại...' : 'Gửi lại mã mới' }}</span>
              </button>
            </div>
          </div>

          <!-- Buttons -->
          <button
            type="submit"
            class="submit-btn"
            :disabled="modalLoading || otpCode.length !== 6 || countdown === 0"
          >
            <svg v-if="modalLoading" class="spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
            </svg>
            <span>{{ modalLoading ? 'Đang kích hoạt...' : 'Xác thực & Đăng nhập ngay' }}</span>
          </button>

          <button
            type="button"
            class="cancel-btn"
            :disabled="modalLoading"
            @click="closeVerifyModal"
          >
            Đóng
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { login, verifyEmail, sendVerificationCode, isLoggedIn, RequiresEmailVerificationError } from '../services/auth'

const router = useRouter()

const username = ref('')
const password = ref('')
const showPassword = ref(false)
const loading = ref(false)
const errorMessage = ref('')
const isDark = ref(false)

// Unverified account modal states
const showVerifyModal = ref(false)
const verifyUsername = ref('')
const verifyMaskedEmail = ref('')
const otpCode = ref('')
const countdown = ref(60)
let timerInterval: any = null
const modalLoading = ref(false)
const resendLoading = ref(false)
const modalErrorMessage = ref('')
const modalSuccessMessage = ref('')

const formattedCountdown = computed(() => {
  const mins = Math.floor(countdown.value / 60)
  const secs = countdown.value % 60
  return `${mins < 10 ? '0' + mins : mins}:${secs < 10 ? '0' + secs : secs}`
})

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

function startCountdown(seconds = 60) {
  stopCountdown()
  countdown.value = seconds
  timerInterval = setInterval(() => {
    if (countdown.value > 0) {
      countdown.value--
    } else {
      stopCountdown()
    }
  }, 1000)
}

function stopCountdown() {
  if (timerInterval) {
    clearInterval(timerInterval)
    timerInterval = null
  }
}

function onOtpInput(e: Event) {
  const target = e.target as HTMLInputElement
  otpCode.value = target.value.replace(/\D/g, '').slice(0, 6)
}

onMounted(() => {
  initTheme()
  if (isLoggedIn()) {
    router.replace('/')
  }
})

onUnmounted(() => {
  stopCountdown()
})

async function handleLogin() {
  if (!username.value || !password.value) {
    errorMessage.value = 'Vui lòng nhập đầy đủ tài khoản và mật khẩu.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    await login(username.value, password.value)
    router.replace('/')
  } catch (err: any) {
    console.error('Login error:', err)
    if (err instanceof RequiresEmailVerificationError || err?.requiresEmailVerification) {
      // Show OTP verification modal and auto-trigger a fresh 60s verification code
      verifyUsername.value = err.username || username.value
      verifyMaskedEmail.value = err.email || ''
      showVerifyModal.value = true
      otpCode.value = ''
      modalErrorMessage.value = ''
      modalSuccessMessage.value = 'Hệ thống đang gửi mã xác thực mới tới email...'

      // Trigger sendVerificationCode
      try {
        const sendRes = await sendVerificationCode(verifyUsername.value)
        verifyMaskedEmail.value = sendRes.email || verifyMaskedEmail.value
        modalSuccessMessage.value = 'Mã xác thực 6 số mới đã được gửi tới email của bạn!'
        startCountdown(sendRes.expiresInSeconds || 60)
      } catch (sendErr: any) {
        modalErrorMessage.value = sendErr?.message || 'Không thể gửi mã xác thực. Vui lòng bấm gửi lại.'
        modalSuccessMessage.value = ''
        startCountdown(60)
      }
    } else {
      errorMessage.value = err?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản và mật khẩu.'
    }
  } finally {
    loading.value = false
  }
}

async function handleModalVerify() {
  if (otpCode.value.length !== 6) {
    modalErrorMessage.value = 'Vui lòng nhập đúng 6 chữ số mã xác thực.'
    return
  }

  if (countdown.value === 0) {
    modalErrorMessage.value = 'Mã xác thực đã hết hạn. Vui lòng bấm "Gửi lại mã mới".'
    return
  }

  modalLoading.value = true
  modalErrorMessage.value = ''

  try {
    const res = await verifyEmail(verifyUsername.value, otpCode.value)
    stopCountdown()
    modalSuccessMessage.value = res.message || 'Xác thực thành công! Đang đăng nhập...'
    setTimeout(() => {
      router.replace('/')
    }, 1000)
  } catch (err: any) {
    console.error('Modal verify error:', err)
    modalErrorMessage.value = err?.message || 'Mã xác thực không chính xác hoặc đã hết hạn.'
  } finally {
    modalLoading.value = false
  }
}

async function handleModalResend() {
  resendLoading.value = true
  modalErrorMessage.value = ''
  modalSuccessMessage.value = ''

  try {
    const res = await sendVerificationCode(verifyUsername.value)
    verifyMaskedEmail.value = res.email || verifyMaskedEmail.value
    otpCode.value = ''
    modalSuccessMessage.value = 'Mã xác thực mới đã được gửi! Vui lòng kiểm tra email của bạn.'
    startCountdown(res.expiresInSeconds || 60)
  } catch (err: any) {
    console.error('Modal resend error:', err)
    modalErrorMessage.value = err?.message || 'Không thể gửi lại mã. Vui lòng thử lại sau.'
  } finally {
    resendLoading.value = false
  }
}

function closeVerifyModal() {
  stopCountdown()
  showVerifyModal.value = false
  otpCode.value = ''
  modalErrorMessage.value = ''
  modalSuccessMessage.value = ''
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  position: relative;
  background-color: var(--bg-app);
  color: var(--text-primary);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  font-family: 'Plus Jakarta Sans', system-ui, -apple-system, sans-serif;
  transition: background-color var(--transition-normal);
}

/* Background glowing accents */
.orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(90px);
  opacity: 0.25;
  pointer-events: none;
  z-index: 0;
}

.orb-1 {
  width: 420px;
  height: 420px;
  background: radial-gradient(circle, #3b82f6, #6366f1);
  top: -100px;
  left: -80px;
}

.orb-2 {
  width: 450px;
  height: 450px;
  background: radial-gradient(circle, #ec4899, #8b5cf6);
  bottom: -120px;
  right: -80px;
}

/* Header */
.auth-header {
  position: relative;
  z-index: 10;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 2.5rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.brand-icon {
  font-size: 1.6rem;
}

.brand-title {
  font-size: 1.25rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  background: linear-gradient(135deg, var(--primary), #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.theme-toggle-btn {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  border: 1px solid var(--border-light);
  background: var(--bg-card);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-fast);
}

.theme-toggle-btn:hover {
  transform: scale(1.05);
  background: var(--bg-subtle);
}

/* Main Area */
.auth-main {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
  position: relative;
  z-index: 10;
}

.auth-card {
  width: 100%;
  max-width: 440px;
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-xl);
  padding: 2.5rem 2.25rem;
  box-shadow: var(--shadow-modal);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  animation: modalScaleIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.card-header {
  text-align: center;
  margin-bottom: 2rem;
}

.header-badge {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--primary);
  background: var(--primary-light);
  padding: 3px 10px;
  border-radius: var(--radius-full);
  margin-bottom: 0.75rem;
}

.auth-title {
  font-size: 1.85rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  margin-bottom: 0.45rem;
}

.auth-subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.5;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.form-label {
  font-size: 0.845rem;
  font-weight: 600;
  color: var(--text-primary);
}

.text-center {
  text-align: center;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1rem;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  pointer-events: none;
}

.form-input {
  width: 100%;
  padding: 0.78rem 1rem 0.78rem 2.65rem;
  border: 1px solid var(--border-light);
  border-radius: var(--radius-md);
  background: var(--bg-subtle);
  color: var(--text-primary);
  font-size: 0.9375rem;
  font-family: inherit;
  box-sizing: border-box;
  transition: all var(--transition-fast);
}

.form-input:focus {
  outline: none;
  border-color: var(--primary);
  background: var(--bg-card);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

.eye-btn {
  position: absolute;
  right: 0.85rem;
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  padding: 0.35rem;
  border-radius: var(--radius-sm);
  transition: color var(--transition-fast);
}

.eye-btn:hover {
  color: var(--text-primary);
}

.error-banner {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  background: var(--danger-light);
  color: var(--danger-hover);
  border: 1px solid #fecaca;
  font-size: 0.845rem;
  line-height: 1.4;
}

html.dark .error-banner {
  background: rgba(239, 68, 68, 0.15);
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.35);
}

.success-banner {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #bbf7d0;
  font-size: 0.845rem;
  line-height: 1.4;
}

html.dark .success-banner {
  background: rgba(34, 197, 94, 0.15);
  color: #86efac;
  border-color: rgba(34, 197, 94, 0.35);
}

.submit-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.85rem 1.5rem;
  margin-top: 0.5rem;
  border-radius: var(--radius-md);
  border: none;
  background: var(--primary);
  color: white;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.25);
  transition: all var(--transition-fast);
}

.submit-btn:hover:not(:disabled) {
  background: var(--primary-hover);
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.cancel-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0.5rem;
  transition: color var(--transition-fast);
  text-align: center;
}

.cancel-btn:hover:not(:disabled) {
  color: var(--text-primary);
  text-decoration: underline;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.auth-footer {
  margin-top: 1.75rem;
  text-align: center;
  font-size: 0.875rem;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
}

.auth-link {
  color: var(--primary);
  font-weight: 700;
  text-decoration: none;
  transition: all var(--transition-fast);
}

.auth-link:hover {
  text-decoration: underline;
  color: var(--primary-hover);
}

/* Modal Overlay & Card */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.65);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  padding: 1.5rem;
  animation: fadeIn 0.2s ease-out;
}

.verify-modal-card {
  width: 100%;
  max-width: 460px;
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-xl);
  padding: 2.25rem 2rem;
  box-shadow: var(--shadow-modal);
  animation: modalScaleIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.verify-modal-header {
  text-align: center;
  margin-bottom: 1.5rem;
}

.verify-badge {
  color: #8b5cf6;
  background: rgba(139, 92, 246, 0.12);
}

.verify-modal-title {
  font-size: 1.6rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  margin-bottom: 0.45rem;
}

.verify-modal-subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.5;
}

.email-highlight {
  color: var(--primary);
  font-weight: 700;
  word-break: break-all;
}

/* OTP Specific Elements */
.otp-group {
  align-items: center;
}

.otp-input-wrapper {
  width: 100%;
  max-width: 280px;
  margin: 0.5rem auto;
}

.otp-input {
  width: 100%;
  text-align: center;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 2rem;
  font-weight: 800;
  letter-spacing: 0.45em;
  padding: 0.75rem 0.5rem;
  border-radius: var(--radius-md);
  border: 2px solid var(--border-light);
  background: var(--bg-subtle);
  color: var(--text-primary);
  transition: all var(--transition-fast);
  box-sizing: border-box;
}

.otp-input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.18);
  background: var(--bg-card);
}

/* Timer Box */
.timer-box {
  background: var(--bg-subtle);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-md);
  padding: 0.9rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.timer-display {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.timer-display strong {
  color: var(--primary);
  font-size: 0.95rem;
}

.timer-display.timer-expired strong,
.text-danger {
  color: #ef4444 !important;
}

.timer-progress-track {
  width: 100%;
  height: 4px;
  background: var(--border-light);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.timer-progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #3b82f6, #6366f1);
  transition: width 1s linear;
}

.resend-action {
  display: flex;
  justify-content: center;
  align-items: center;
}

.resend-countdown-hint {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.resend-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.45rem 1rem;
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-full);
  color: var(--primary);
  font-size: 0.84rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-fast);
}

.resend-btn:hover:not(:disabled) {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
  transform: translateY(-1px);
}

.resend-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 480px) {
  .auth-header {
    padding: 1.25rem 1.5rem;
  }
  .auth-card {
    padding: 2rem 1.5rem;
  }
  .verify-modal-card {
    padding: 1.75rem 1.25rem;
  }
  .otp-input {
    font-size: 1.6rem;
    letter-spacing: 0.35em;
  }
}
</style>

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

    <!-- Main Register Card -->
    <main class="auth-main">
      <div class="auth-card">
        <!-- STEP 1: Registration Form -->
        <div v-if="step === 'register'">
          <div class="card-header">
            <div class="header-badge">Tạo tài khoản mới</div>
            <h1 class="auth-title">Đăng ký</h1>
            <p class="auth-subtitle">Bắt đầu lưu giữ những kỷ niệm và cảm xúc hàng ngày của bạn.</p>
          </div>

          <form class="auth-form" @submit.prevent="handleRegister">
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
              <label class="form-label" for="reg-username">Tài khoản</label>
              <div class="input-wrapper">
                <span class="input-icon">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                    <circle cx="12" cy="7" r="4"></circle>
                  </svg>
                </span>
                <input
                  id="reg-username"
                  v-model.trim="username"
                  type="text"
                  class="form-input"
                  placeholder="Chọn tên tài khoản (tối thiểu 3 ký tự)..."
                  required
                  minlength="3"
                  maxlength="50"
                  autocomplete="username"
                  :disabled="loading"
                />
              </div>
              <span class="input-hint">Dùng để đăng nhập vào nhật ký của bạn.</span>
            </div>

            <!-- Email Input -->
            <div class="form-group">
              <label class="form-label" for="reg-email">Địa chỉ Email</label>
              <div class="input-wrapper">
                <span class="input-icon">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                    <polyline points="22,6 12,13 2,6"></polyline>
                  </svg>
                </span>
                <input
                  id="reg-email"
                  v-model.trim="email"
                  type="email"
                  class="form-input"
                  placeholder="name@example.com..."
                  required
                  autocomplete="email"
                  :disabled="loading"
                />
              </div>
              <span class="input-hint">Mã xác thực 6 số sẽ được gửi tới email này.</span>
            </div>

            <!-- Password Input -->
            <div class="form-group">
              <label class="form-label" for="reg-password">Mật khẩu</label>
              <div class="input-wrapper">
                <span class="input-icon">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                    <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                  </svg>
                </span>
                <input
                  id="reg-password"
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  class="form-input"
                  placeholder="Tối thiểu 6 ký tự..."
                  required
                  minlength="6"
                  autocomplete="new-password"
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
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                    <circle cx="12" cy="12" r="3"></circle>
                  </svg>
                  <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                    <line x1="1" y1="1" x2="23" y2="23"></line>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Confirm Password Input -->
            <div class="form-group">
              <label class="form-label" for="reg-confirm-password">Xác nhận lại mật khẩu</label>
              <div class="input-wrapper">
                <span class="input-icon">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                    <polyline points="22 4 12 14.01 9 11.01"></polyline>
                  </svg>
                </span>
                <input
                  id="reg-confirm-password"
                  v-model="confirmPassword"
                  :type="showConfirmPassword ? 'text' : 'password'"
                  class="form-input"
                  :class="{
                    'input-error': confirmPassword && !passwordsMatch,
                    'input-success': confirmPassword && passwordsMatch
                  }"
                  placeholder="Nhập lại mật khẩu..."
                  required
                  autocomplete="new-password"
                  :disabled="loading"
                />
                <button
                  type="button"
                  class="eye-btn"
                  @click="showConfirmPassword = !showConfirmPassword"
                  :title="showConfirmPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                  tabindex="-1"
                >
                  <svg v-if="!showConfirmPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8z"></path>
                    <circle cx="12" cy="12" r="3"></circle>
                  </svg>
                  <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                    <line x1="1" y1="1" x2="23" y2="23"></line>
                  </svg>
                </button>
              </div>
              <!-- Match Indicator -->
              <div v-if="confirmPassword" class="match-hint" :class="{ match: passwordsMatch }">
                <span v-if="passwordsMatch">✓ Mật khẩu khớp nhau</span>
                <span v-else>✗ Mật khẩu xác nhận chưa khớp</span>
              </div>
            </div>

            <!-- Submit Button -->
            <button type="submit" class="submit-btn" :disabled="loading || (confirmPassword !== '' && !passwordsMatch)">
              <svg v-if="loading" class="spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
              </svg>
              <span>{{ loading ? 'Đang gửi mã xác thực...' : 'Tiếp tục & Nhận mã xác thực' }}</span>
            </button>
          </form>

          <div class="auth-footer">
            <span>Đã có tài khoản?</span>
            <router-link to="/login" class="auth-link">Đăng nhập</router-link>
          </div>
        </div>

        <!-- STEP 2: Email OTP Verification -->
        <div v-else class="verify-step">
          <div class="card-header">
            <div class="header-badge verify-badge">Bước 2 / 2: Xác thực Email</div>
            <h1 class="auth-title">Xác thực Email</h1>
            <p class="auth-subtitle">
              Mã xác thực gồm 6 chữ số đã được gửi tới:
              <br />
              <strong class="email-highlight">{{ maskedEmail || email }}</strong>
            </p>
          </div>

          <!-- Alert message banner -->
          <div v-if="verifyErrorMessage" class="error-banner" role="alert">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <span>{{ verifyErrorMessage }}</span>
          </div>

          <div v-if="verifySuccessMessage" class="success-banner" role="status">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
              <polyline points="22 4 12 14.01 9 11.01"></polyline>
            </svg>
            <span>{{ verifySuccessMessage }}</span>
          </div>

          <form class="auth-form" @submit.prevent="handleVerifyOtp">
            <!-- 6-Digit OTP Input -->
            <div class="form-group otp-group">
              <label class="form-label text-center" for="reg-otp">Nhập mã xác thực 6 chữ số</label>
              <div class="otp-input-wrapper">
                <input
                  id="reg-otp"
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
                  :disabled="verifyLoading"
                  @input="onOtpInput"
                />
              </div>
            </div>

            <!-- Timer and Resend Box -->
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

              <!-- Progress bar for 60s -->
              <div class="timer-progress-track">
                <div class="timer-progress-bar" :style="{ width: `${(countdown / 60) * 100}%` }"></div>
              </div>

              <!-- Resend Button -->
              <div class="resend-action">
                <span v-if="countdown > 0" class="resend-countdown-hint">
                  Có thể gửi lại mã sau <strong>{{ countdown }}s</strong>
                </span>
                <button
                  v-else
                  type="button"
                  class="resend-btn"
                  :disabled="resendLoading"
                  @click="handleResendCode"
                >
                  <svg v-if="resendLoading" class="spinner" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
                  </svg>
                  <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="23 4 23 10 17 10"></polyline>
                    <polyline points="1 20 1 14 7 14"></polyline>
                    <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
                  </svg>
                  <span>{{ resendLoading ? 'Đang gửi lại...' : 'Gửi lại mã xác thực' }}</span>
                </button>
              </div>
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              class="submit-btn"
              :disabled="verifyLoading || otpCode.length !== 6 || countdown === 0"
            >
              <svg v-if="verifyLoading" class="spinner" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 12a9 9 0 1 1-6.219-8.56"></path>
              </svg>
              <span>{{ verifyLoading ? 'Đang xác thực...' : 'Xác nhận & Hoàn tất' }}</span>
            </button>

            <!-- Back to edit info -->
            <button
              type="button"
              class="back-btn"
              :disabled="verifyLoading"
              @click="backToForm"
            >
              ← Quay lại đổi thông tin
            </button>
          </form>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { register, verifyEmail, sendVerificationCode, isLoggedIn } from '../services/auth'

const router = useRouter()

// Steps: 'register' -> 'verify'
const step = ref<'register' | 'verify'>('register')

// Step 1 states
const username = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const loading = ref(false)
const errorMessage = ref('')

// Step 2 states
const maskedEmail = ref('')
const otpCode = ref('')
const countdown = ref(60)
let timerInterval: any = null
const verifyLoading = ref(false)
const resendLoading = ref(false)
const verifyErrorMessage = ref('')
const verifySuccessMessage = ref('')

const isDark = ref(false)

const passwordsMatch = computed(() => {
  return password.value !== '' && password.value === confirmPassword.value
})

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
  // Keep only digits
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

async function handleRegister() {
  errorMessage.value = ''

  if (!username.value || username.value.length < 3) {
    errorMessage.value = 'Tên tài khoản phải có ít nhất 3 ký tự.'
    return
  }

  if (!email.value || !email.value.includes('@')) {
    errorMessage.value = 'Vui lòng nhập địa chỉ email hợp lệ.'
    return
  }

  if (!password.value || password.value.length < 6) {
    errorMessage.value = 'Mật khẩu phải có ít nhất 6 ký tự.'
    return
  }

  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'Mật khẩu xác nhận không khớp. Vui lòng kiểm tra lại.'
    return
  }

  loading.value = true

  try {
    const res = await register(username.value, email.value, password.value, confirmPassword.value)
    maskedEmail.value = res.email || email.value
    step.value = 'verify'
    otpCode.value = ''
    verifyErrorMessage.value = ''
    verifySuccessMessage.value = 'Mã xác thực 6 số đã được gửi qua email. Vui lòng kiểm tra hộp thư!'
    startCountdown(res.expiresInSeconds || 60)
  } catch (err: any) {
    console.error('Register error:', err)
    errorMessage.value = err?.message || 'Đăng ký thất bại. Vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}

async function handleVerifyOtp() {
  if (otpCode.value.length !== 6) {
    verifyErrorMessage.value = 'Vui lòng nhập đủ 6 chữ số mã xác thực.'
    return
  }

  if (countdown.value === 0) {
    verifyErrorMessage.value = 'Mã xác thực đã hết hiệu lực (60s). Vui lòng bấm "Gửi lại mã xác thực".'
    return
  }

  verifyLoading.value = true
  verifyErrorMessage.value = ''

  try {
    const res = await verifyEmail(username.value, otpCode.value)
    stopCountdown()
    verifySuccessMessage.value = res.message || 'Xác thực thành công! Đang chuyển hướng...'
    setTimeout(() => {
      router.replace('/')
    }, 1000)
  } catch (err: any) {
    console.error('Verify error:', err)
    verifyErrorMessage.value = err?.message || 'Mã xác thực không chính xác hoặc đã hết hạn.'
  } finally {
    verifyLoading.value = false
  }
}

async function handleResendCode() {
  resendLoading.value = true
  verifyErrorMessage.value = ''
  verifySuccessMessage.value = ''

  try {
    const res = await sendVerificationCode(username.value)
    maskedEmail.value = res.email || maskedEmail.value
    otpCode.value = ''
    verifySuccessMessage.value = 'Mã xác thực mới đã được gửi! Vui lòng kiểm tra email của bạn.'
    startCountdown(res.expiresInSeconds || 60)
  } catch (err: any) {
    console.error('Resend error:', err)
    verifyErrorMessage.value = err?.message || 'Không thể gửi lại mã xác thực. Vui lòng thử lại sau.'
  } finally {
    resendLoading.value = false
  }
}

function backToForm() {
  stopCountdown()
  step.value = 'register'
  verifyErrorMessage.value = ''
  verifySuccessMessage.value = ''
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
  max-width: 480px;
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
  margin-bottom: 1.75rem;
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

.verify-badge {
  color: #8b5cf6;
  background: rgba(139, 92, 246, 0.12);
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

.email-highlight {
  color: var(--primary);
  font-weight: 700;
  word-break: break-all;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.15rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-label {
  font-size: 0.845rem;
  font-weight: 600;
  color: var(--text-primary);
}

.text-center {
  text-align: center;
}

.input-hint {
  font-size: 0.75rem;
  color: var(--text-muted);
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

.form-input.input-error {
  border-color: #ef4444;
  background: #fff5f5;
}

html.dark .form-input.input-error {
  background: rgba(239, 68, 68, 0.08);
}

.form-input.input-success {
  border-color: #22c55e;
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

.match-hint {
  font-size: 0.75rem;
  font-weight: 600;
  color: #ef4444;
  margin-top: 0.15rem;
}

.match-hint.match {
  color: #16a34a;
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

.back-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-size: 0.845rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0.5rem;
  transition: color var(--transition-fast);
  text-align: center;
}

.back-btn:hover:not(:disabled) {
  color: var(--text-primary);
  text-decoration: underline;
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

@media (max-width: 480px) {
  .auth-header {
    padding: 1.25rem 1.5rem;
  }
  .auth-card {
    padding: 2rem 1.5rem;
  }
  .otp-input {
    font-size: 1.6rem;
    letter-spacing: 0.35em;
  }
}
</style>

import { reactive } from 'vue'

const TOKEN_KEY = 'mydiary_auth_token'
const USER_KEY = 'mydiary_auth_user'

export interface AuthUser {
  username: string
  userId?: string
}

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token)
    authState.token = token
    authState.isAuthenticated = true
  } catch {}
}

export function getUser(): AuthUser | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function setUser(user: AuthUser): void {
  try {
    localStorage.setItem(USER_KEY, JSON.stringify(user))
    authState.user = user
  } catch {}
}

export function clearAuth(): void {
  try {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    authState.token = null
    authState.user = null
    authState.isAuthenticated = false
  } catch {}
}

export function isLoggedIn(): boolean {
  return !!getToken()
}

export const authState = reactive({
  token: getToken(),
  user: getUser(),
  isAuthenticated: isLoggedIn(),
})

export function getApiBase(): string {
  return (import.meta.env.VITE_API_BASE_URL as string)?.replace(/\/+$/, '') 
    ?? (import.meta.env.DEV ? 'http://localhost:5224' : '')
}

export function getAuthHeaders(): Record<string, string> {
  const token = getToken()
  return token ? { 'Authorization': `Bearer ${token}` } : {}
}

export async function login(username: string, password: string):Promise<{ token: string; username: string; userId: string }> {
  const apiBase = getApiBase()
  const res = await fetch(`${apiBase}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })

  if (!res.ok) {
    const data = await res.json().catch(() => null)
    throw new Error(data?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản và mật khẩu.')
  }

  const data = await res.json()
  setToken(data.token)
  setUser({ username: data.username, userId: data.userId })
  return data
}

export async function register(username: string, password: string, confirmPassword: string): Promise<{ token: string; username: string; userId: string }> {
  const apiBase = getApiBase()
  const res = await fetch(`${apiBase}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password, confirmPassword }),
  })

  if (!res.ok) {
    const data = await res.json().catch(() => null)
    throw new Error(data?.message || 'Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.')
  }

  const data = await res.json()
  setToken(data.token)
  setUser({ username: data.username, userId: data.userId })
  return data
}

export function logout(): void {
  clearAuth()
  window.location.href = '/login'
}

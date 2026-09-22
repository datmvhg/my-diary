import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './router'

// Initialize theme from localStorage or system preference
try {
  const savedTheme = localStorage.getItem('mydiary-theme')
  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
} catch {
  // noop in non-browser environments
}

createApp(App).use(router).mount('#app')

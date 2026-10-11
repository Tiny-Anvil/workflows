import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = { ...loadEnv(mode, process.cwd(), ''), ...process.env }
  // The guard frontend-build checks: a production bundle must never contain the auth bypass.
  if (mode === 'production' && env.VITE_AUTH_BYPASS === 'true') {
    throw new Error('Refusing VITE_AUTH_BYPASS=true in a production build')
  }
  return { plugins: [vue()] }
})

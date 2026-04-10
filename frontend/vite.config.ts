import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 3099,
    proxy: {
      '/api': {
        target: 'http://localhost:5099',
        changeOrigin: true,
      },
      '/swagger': {
        target: 'http://localhost:5099',
        changeOrigin: true,
      },
    },
  },
})

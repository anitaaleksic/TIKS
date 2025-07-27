import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {   // svi zahtevi koji počinju sa /api se preusmeravaju na backend
        target: 'http://localhost:5023',   // tvoj .NET backend URL
        changeOrigin: true,
        secure: false 
      }
    }
  }
})

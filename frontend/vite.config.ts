/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    host: true, // Crucial for Docker
    port: 5173,
    watch: {
      usePolling: true
    },
    // Forward API calls to the backend started with `dotnet run` (or Docker on port 5080)
    proxy: {
      '/api': 'http://localhost:5080'
    }
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})

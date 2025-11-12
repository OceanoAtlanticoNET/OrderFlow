import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    // Allow Aspire to configure the port via environment variable
    port: parseInt(process.env.PORT || '5173'),
    strictPort: true,
    host: true, // Listen on all network interfaces
  }
})

console.log('Vite server will use PORT:', process.env.PORT );

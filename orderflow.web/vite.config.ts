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
    // Aspire configures the port via VITE_PORT environment variable
      port: parseInt(process.env.VITE_PORT!),
    strictPort: true,
    host: true, // Listen on all network interfaces
  }
})

console.log('Vite server will use VITE_PORT:', process.env.VITE_PORT);

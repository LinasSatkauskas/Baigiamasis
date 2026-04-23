import { fileURLToPath, URL } from "node:url"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from "vite"
import plugin from "@vitejs/plugin-react"
import { env } from "process"

const aspNetTarget = env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(";")[0]
  : undefined

const target = (
  env.VITE_API_URL ||
  aspNetTarget ||
  (env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
    : "http://localhost:5166")
).replace("0.0.0.0", "localhost")

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin(), tailwindcss()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    host: true,
    proxy: {
      "^/api": {
        target,
        secure: false,
      },
    },
    port: parseInt(env.DEV_SERVER_PORT || "63835"),
  },
})

import { create } from "zustand"
import { getApi, postApiRaw, resetXsrf } from "@/api"

type UserInfo = { id: string; email: string; userName: string }

type AuthState = {
  user: UserInfo | null
  roles: string[]
  loading: boolean
  error?: string

  loadMe: () => Promise<void>
  login: (
    email: string,
    password: string,
    rememberMe: boolean,
  ) => Promise<boolean>
  register: (email: string, password: string) => Promise<boolean>
  logout: () => Promise<void>

  isAdmin: () => boolean
  isUser: () => boolean
}

function extractIdentityErrors(payload: any): string | undefined {
  if (payload?.message) {
    return payload.message
  }
  // IdentityError[] => [{ code, description }]
  if (Array.isArray(payload) && payload[0]?.description) {
    return payload.map((e: any) => e.description).join(" ")
  }
  // ProblemDetails with ModelState
  if (payload?.errors && typeof payload.errors === "object") {
    const msgs: string[] = []
    for (const k of Object.keys(payload.errors)) {
      const arr = payload.errors[k]
      if (Array.isArray(arr)) msgs.push(...arr)
    }
    if (msgs.length) return msgs.join(" ")
  }
  if (payload?.detail || payload?.title) {
    return [payload.title, payload.detail].filter(Boolean).join(" - ")
  }
  return undefined
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  roles: [],
  loading: false,
  error: undefined,

  loadMe: async () => {
    try {
      set({ loading: true, error: undefined })
      const me = await getApi<{ user: UserInfo; roles: string[] }>("account/me")
      if (me && (me as any).user) set({ user: me.user, roles: me.roles })
      else set({ user: null, roles: [] })
    } finally {
      set({ loading: false })
    }
  },

  login: async (email, password, rememberMe) => {
    try {
      set({ loading: true, error: undefined })
      const res = await postApiRaw("account/login", {
        email,
        password,
        rememberMe,
      })
      if (!res.ok) {
        const friendlyLoginMessage =
          res.status === 401
            ? "You've typed in the wrong password or email."
            : undefined

        set({
          error:
            friendlyLoginMessage ||
            extractIdentityErrors(res.bodyJson) ||
            res.bodyText ||
            `Login failed (${res.status})`,
        })
        return false
      }
      await get().loadMe()
      return true
    } finally {
      set({ loading: false })
    }
  },

  register: async (email, password) => {
    try {
      set({ loading: true, error: undefined })
      const res = await postApiRaw("account/register", { email, password })
      if (!res.ok) {
        set({
          error:
            extractIdentityErrors(res.bodyJson) ||
            res.bodyText ||
            `Registration failed (${res.status})`,
        })
        return false
      }
      const ok = await get().login(email, password, false)
      return ok
    } finally {
      set({ loading: false })
    }
  },

  logout: async () => {
    try {
      set({ loading: true, error: undefined })
      await postApiRaw("account/logout", {})
      resetXsrf()
      // Force-check server state; should be 401 if cookie is gone
      await get().loadMe()
    } finally {
      set({ loading: false })
    }
  },

  isAdmin: () => get().roles.includes("Admin"),
  isUser: () => get().roles.includes("User"),
}))

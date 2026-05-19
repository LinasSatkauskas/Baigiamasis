import { useEffect, useState } from "react"
import { Link, useSearchParams } from "react-router-dom"
import { postApiRaw } from "@/api"

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const [email, setEmail] = useState(searchParams.get("email") ?? "")
  const [token, setToken] = useState(searchParams.get("token") ?? "")
  const [password, setPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setEmail(searchParams.get("email") ?? "")
    setToken(searchParams.get("token") ?? "")
  }, [searchParams])

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setMessage(null)
    setError(null)

    if (!email || !token) {
      setError("Trūksta atstatymo nuorodos duomenų.")
      return
    }

    if (password.length < 6) {
      setError("Slaptažodis turi būti bent 6 simbolių.")
      return
    }

    if (password !== confirmPassword) {
      setError("Slaptažodžiai nesutampa.")
      return
    }

    setLoading(true)
    try {
      const res = await postApiRaw("account/reset-password", {
        email,
        token,
        password,
      })

      if (!res.ok) {
        const serverMessage =
          res.bodyJson?.detail ||
          res.bodyJson?.title ||
          res.bodyText ||
          "Nepavyko atstatyti slaptažodžio."
        setError(serverMessage)
        return
      }

      setMessage(
        "Slaptažodis sėkmingai atnaujintas. Dabar galite prisijungti su nauju slaptažodžiu.",
      )
      setPassword("")
      setConfirmPassword("")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-linear-to-br from-slate-50 via-emerald-50 to-lime-100 px-4">
      <div className="w-full max-w-md rounded-3xl bg-white/90 shadow-2xl ring-1 ring-black/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.2em] text-emerald-700">
          Slaptažodžio atstatymas
        </p>
        <h1 className="mt-2 text-3xl font-bold text-slate-900">
          Nustatykite naują slaptažodį
        </h1>
        <p className="mt-3 text-sm text-slate-600">
          Įveskite naują slaptažodį paskyrai {email || "jūsų el. paštui"}.
        </p>

        <form onSubmit={onSubmit} className="mt-6 space-y-4">
          {error && (
            <div className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700">
              {error}
            </div>
          )}
          {message && (
            <div className="rounded-xl bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
              {message}
            </div>
          )}

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              El. paštas
            </label>
            <input
              className="w-full rounded-xl border border-slate-300 px-4 py-3 outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-200"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Slaptažodis
            </label>
            <input
              className="w-full rounded-xl border border-slate-300 px-4 py-3 outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-200"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Pakartokite slaptažodį
            </label>
            <input
              className="w-full rounded-xl border border-slate-300 px-4 py-3 outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-200"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>

          <button
            className="w-full rounded-xl bg-emerald-600 px-4 py-3 font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
            type="submit"
            disabled={loading}
          >
            {loading ? "Tęsiama..." : "Atstatyti slaptažodį"}
          </button>
        </form>

        <div className="mt-6 text-sm text-slate-600">
          <Link to="/" className="font-medium text-emerald-700 hover:underline">
            Grįžti į pradžią
          </Link>
        </div>
      </div>
    </div>
  )
}

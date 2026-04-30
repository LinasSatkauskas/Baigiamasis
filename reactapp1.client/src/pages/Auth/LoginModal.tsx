import { useState } from "react"
import { useAuthStore } from "@/store/authStore"
import { postApiRaw } from "@/api"
import { Modal } from "@/pages/components/Modal"
import { formStyle } from "@/styles/formStyle"

type Props = {
  visible: boolean
  onClose: () => void
}

export function LoginModal({ visible, onClose }: Props) {
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [rememberMe, setRememberMe] = useState(false)
  const [showForgotPassword, setShowForgotPassword] = useState(false)
  const [resetEmail, setResetEmail] = useState("")
  const [resetMessage, setResetMessage] = useState("")
  const [resetError, setResetError] = useState("")
  const { login, loading, error } = useAuthStore()

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const ok = await login(email, password, rememberMe)
    if (ok) onClose()
  }

  const onRequestPasswordReset = async (e: React.FormEvent) => {
    e.preventDefault()
    setResetMessage("")
    setResetError("")

    const normalizedEmail = resetEmail.trim().toLowerCase()
    if (!normalizedEmail) {
      setResetError("Įveskite el. paštą.")
      return
    }

    try {
      const res = await postApiRaw("account/forgot-password", {
        email: normalizedEmail,
      })
      if (!res.ok) {
        const serverMessage =
          res.bodyJson?.detail ||
          res.bodyJson?.title ||
          res.bodyText ||
          "Klaida. Bandykite vėliau arba susisiekite su administratoriumi."
        setResetError(serverMessage)
        return
      }

      setResetMessage(
        `Jei toks el. paštas egzistuoja, atstatymo nuoroda buvo išsiųsta į ${normalizedEmail}.`,
      )
      setTimeout(() => {
        setShowForgotPassword(false)
        setResetEmail("")
        setResetMessage("")
      }, 3000)
    } catch {
      setResetError(
        "Klaida. Bandykite vėliau arba susisiekite su administratoriumi.",
      )
    }
  }

  return (
    <>
      <Modal
        visibleModal={visible}
        setVisibleModal={() => onClose()}
        title="Prisijungti"
      >
        <form onSubmit={onSubmit} className="flex flex-col gap-3">
          {error && <div className="text-red-600 text-sm">{error}</div>}
          <div>
            <label className={formStyle.label}>El. paštas</label>
            <input
              className={formStyle.input}
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div>
            <label className={formStyle.label}>Slaptažodis</label>
            <input
              className={formStyle.input}
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <div className="flex items-center justify-between gap-4">
            <label className="inline-flex items-center gap-2">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
              />
              <span>Prisiminti</span>
            </label>
            <button
              type="button"
              onClick={() => setShowForgotPassword(true)}
              className="text-xs font-medium text-blue-600 hover:underline"
            >
              Pamiršau slaptažodį
            </button>
          </div>
          <div className="flex gap-2">
            <button
              className={formStyle.button}
              type="submit"
              disabled={loading}
            >
              Prisijungti
            </button>
            <button
              className={formStyle.button}
              type="button"
              onClick={onClose}
            >
              Uždaryti
            </button>
          </div>
        </form>
      </Modal>

      <Modal
        visibleModal={showForgotPassword}
        setVisibleModal={() => setShowForgotPassword(false)}
        title="Atstatyti slaptažodį"
      >
        <form onSubmit={onRequestPasswordReset} className="flex flex-col gap-3">
          {resetError && (
            <div className="rounded-lg bg-red-100 px-3 py-2 text-sm text-red-700">
              {resetError}
            </div>
          )}
          {resetMessage && (
            <div className="rounded-lg bg-emerald-100 px-3 py-2 text-sm text-emerald-700">
              {resetMessage}
            </div>
          )}
          <div>
            <label className={formStyle.label}>El. paštas</label>
            <input
              className={formStyle.input}
              type="email"
              value={resetEmail}
              onChange={(e) => setResetEmail(e.target.value)}
              placeholder="Įveskite savo el. paštą"
              required
            />
          </div>
          <p className="text-xs text-gray-600">
            Į šį el. paštą bus išsiųsta slaptažodžio atstatymo nuoroda.
          </p>
          <div className="flex gap-2">
            <button className={formStyle.button} type="submit">
              Siųsti
            </button>
            <button
              className={formStyle.button}
              type="button"
              onClick={() => {
                setShowForgotPassword(false)
                setResetEmail("")
                setResetMessage("")
                setResetError("")
              }}
            >
              Atgal
            </button>
          </div>
        </form>
      </Modal>
    </>
  )
}

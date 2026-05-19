import { useEffect, useState } from "react"
import { deleteApi, getApi } from "@/api"
import { useAuthStore } from "@/store/authStore"
import { IUserItem } from "@/interfaces/IUser"

export default function Users() {
  const [users, setUsers] = useState<IUserItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | undefined>()
  const [deletingId, setDeletingId] = useState<string | null>(null)

  const currentUser = useAuthStore((s) => s.user)

  const loadUsers = async () => {
    setLoading(true)
    setError(undefined)
    try {
      const data = await getApi<IUserItem[]>("account/users")
      setUsers(data ?? [])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadUsers()
  }, [])

  const deleteUser = async (user: IUserItem) => {
    if (!user.id) return
    const label = user.email ?? user.userName ?? user.id
    const ok = window.confirm(`Ar tikrai norite ištrinti vartotoją ${label}?`)
    if (!ok) return

    setDeletingId(user.id)
    setError(undefined)
    try {
      const result = await deleteApi(`account/users/${user.id}`, {})
      if (result === undefined) {
        setError("Nepavyko ištrinti vartotojo.")
        return
      }
      await loadUsers()
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <div className="text-3xl font-semibold text-emerald-950">
            Vartotojai
          </div>
          <div className="text-sm text-gray-600 mt-1">
            Administratorius gali peržiūrėti vartotojų sąrašą ir ištrinti
            paskyras.
          </div>
        </div>
        <button
          type="button"
          className="rounded-md bg-emerald-700 px-4 py-2 text-white hover:bg-emerald-800 disabled:opacity-60"
          onClick={loadUsers}
          disabled={loading}
        >
          {loading ? "Kraunama..." : "Atnaujinti"}
        </button>
      </div>

      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      )}

      <div className="overflow-hidden rounded-xl border border-emerald-100 bg-white shadow-sm">
        <table className="min-w-full divide-y divide-emerald-100">
          <thead className="bg-emerald-50/70">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-semibold text-emerald-950">
                El. paštas
              </th>
              <th className="px-4 py-3 text-left text-sm font-semibold text-emerald-950">
                Vardas
              </th>
              <th className="px-4 py-3 text-left text-sm font-semibold text-emerald-950">
                Rolės
              </th>
              <th className="px-4 py-3 text-left text-sm font-semibold text-emerald-950">
                Būsena
              </th>
              <th className="px-4 py-3 text-right text-sm font-semibold text-emerald-950"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-emerald-50">
            {users.map((user) => {
              const label = user.email ?? user.userName ?? user.id
              const isCurrent =
                currentUser?.id === user.id || user.isCurrentUser
              return (
                <tr key={user.id} className="hover:bg-emerald-50/40">
                  <td className="px-4 py-3 text-sm text-gray-900">
                    {user.email || "-"}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">
                    {user.userName || "-"}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">
                    <div className="flex flex-wrap gap-2">
                      {user.roles.length > 0 ? (
                        user.roles.map((role) => (
                          <span
                            key={role}
                            className="rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-medium text-emerald-800"
                          >
                            {role}
                          </span>
                        ))
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">
                    {isCurrent ? (
                      <span className="rounded-full bg-amber-100 px-2.5 py-1 text-xs font-medium text-amber-800">
                        Dabartinis naudotojas
                      </span>
                    ) : (
                      <span className="text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      className="rounded-md border border-red-200 px-3 py-1.5 text-sm font-medium text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
                      onClick={() => deleteUser(user)}
                      disabled={loading || deletingId === user.id || isCurrent}
                      title={
                        isCurrent
                          ? "Savo paskyros ištrinti negalite"
                          : `Ištrinti ${label}`
                      }
                    >
                      {deletingId === user.id ? "Trinama..." : "Ištrinti"}
                    </button>
                  </td>
                </tr>
              )
            })}
            {!loading && users.length === 0 && (
              <tr>
                <td className="px-4 py-6 text-sm text-gray-500" colSpan={5}>
                  Vartotojų nerasta.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}

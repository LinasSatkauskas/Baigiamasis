import { useEffect, useMemo, useState } from "react"
import { useForm } from "react-hook-form"
import { getApi, postApi, putApi, deleteApi } from "../../../api"
import { IComment } from "../../../interfaces/IComment"
import { formStyle } from "../../../styles/formStyle"
import { useAuthStore } from "@/store/authStore"
import {
  ensureCommentsHubConnection,
  stopCommentsHubConnection,
} from "../../../realtime/commentsHub"

type Props = { plantId: number; plantName: string }

export function CommentsSection({ plantId, plantName }: Props) {
  const [comments, setComments] = useState<IComment[]>([])
  const [edit, setEdit] = useState<IComment | undefined>()
  const [selected, setSelected] = useState<Set<number>>(new Set())
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | undefined>(undefined)

  const user = useAuthStore((s) => s.user)
  const isAdmin = useAuthStore((s) => s.isAdmin)
  const isUser = useAuthStore((s) => s.isUser)

  const canView = !!user && (isUser() || isAdmin())
  const canPost = canView
  const currentEmail = user?.email ?? ""
  const currentEmailLower = currentEmail.toLowerCase()
  const canEditComment = (comment: IComment) =>
    isAdmin() || comment.email.toLowerCase() === currentEmailLower

  const formatCommentTime = (value?: string | null) => {
    if (!value) return ""
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString("lt-LT", {
      dateStyle: "medium",
      timeStyle: "short",
    })
  }

  const defaults = useMemo<IComment>(
    () => ({
      id: undefined,
      plantId,
      email: "",
      text: "",
      isApproved: false,
    }),
    [plantId],
  )

  const { register, handleSubmit, reset, setValue } = useForm<IComment>({
    defaultValues: defaults,
  })

  const load = () =>
    getApi<IComment[]>(`comments?plantId=${plantId}`).then(
      (c) => c && setComments(c),
    )

  useEffect(() => {
    load()
  }, [plantId])

  useEffect(() => {
    setValue("plantId", plantId)
    reset(edit ?? defaults)
  }, [edit, reset, defaults, plantId, setValue])

  useEffect(() => {
    let active = true
    let cleanup: (() => void) | undefined

    const start = async () => {
      try {
        const connection = await ensureCommentsHubConnection()
        if (!active) return

        const handleCommentsChanged = async (changedPlantId: number) => {
          if (changedPlantId === plantId) {
            await load()
          }
        }

        const handleReconnected = async () => {
          try {
            await connection.invoke("JoinPlantGroup", plantId)
            await load()
          } catch (err) {
            console.error("Comments hub rejoin failed:", err)
          }
        }

        connection.off("CommentsChanged")
        connection.on("CommentsChanged", handleCommentsChanged)
        connection.onreconnected(handleReconnected)

        await connection.invoke("JoinPlantGroup", plantId)

        cleanup = () => {
          connection.off("CommentsChanged", handleCommentsChanged)
          connection.onreconnected(() => undefined)
          void connection
            .invoke("LeavePlantGroup", plantId)
            .catch(() => undefined)
        }
      } catch (err) {
        console.error("Comments hub connection failed:", err)
      }
    }

    void start()

    return () => {
      active = false
      cleanup?.()
      void stopCommentsHubConnection()
    }
  }, [plantId])

  const onSubmit = handleSubmit(async (data, e) => {
    if (!canPost) return

    setError(undefined)
    setSubmitting(true)
    try {
      const nativeSubmitter = (e?.nativeEvent as SubmitEvent | undefined)
        ?.submitter as HTMLButtonElement | undefined
      let action = nativeSubmitter?.value
      if (!action && e?.currentTarget) {
        const fd = new FormData(e.currentTarget as HTMLFormElement)
        action = (fd.get("action") as string) || undefined
      }
      action = action || "update"

      if (action === "delete") {
        if (data.id) await deleteApi(`comments/${data.id}`, {})
        reset(defaults)
        setEdit(undefined)
        await load()
        return
      }

      const isNew = action === "new" || !data.id
      const payload: IComment = {
        id: isNew ? undefined : data.id,
        plantId,
        text: data.text,
        email: isNew ? currentEmail : (edit?.email ?? currentEmail),
        isApproved: edit?.isApproved ?? false,
      }

      const task = isNew
        ? postApi("comments", payload)
        : putApi(`comments/${payload.id}`, payload)

      await task
      await load()
      if (isNew) reset(defaults)
    } catch (err: any) {
      setError(err?.message || "Nepavyko išsaugoti komentaro.")
    } finally {
      setSubmitting(false)
    }
  })

  const toggleSelect = (id?: number) => {
    if (!id) return
    setSelected((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  const bulkDelete = async () => {
    const ids = Array.from(selected)
    if (ids.length === 0) return
    await Promise.all(ids.map((id) => deleteApi(`comments/${id}`, {})))
    setSelected(new Set())
    await load()
  }

  if (!canView) return null

  return (
    <div className="mt-8 mb-20">
      <div className="flex items-center justify-between mb-3">
        <div className="text-xl font-semibold flex items-center gap-2">
          Komentarai apie „{plantName}“
          <span className="text-xs font-medium bg-emerald-50 text-emerald-700 px-2 py-0.5 rounded ring-1 ring-inset ring-emerald-600/20">
            {comments.length}
          </span>
        </div>
      </div>

      <div className="flex items-start gap-6 flex-wrap">
        <div className="flex-1 min-w-[20rem]">
          <div className="overflow-hidden rounded-lg shadow ring-1 ring-gray-200 bg-white">
            <table className="min-w-full table-fixed">
              <thead>
                <tr className="bg-gray-50">
                  <th className="w-56 px-4 py-3 text-left text-sm font-semibold text-gray-700">
                    Vartotojo paštas
                  </th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                    Komentaras
                  </th>
                  <th className="w-28 px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {comments.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="w-56 px-4 py-3 align-top">
                      <div className="flex items-center gap-3">
                        <div className="h-8 w-8 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-semibold ring-1 ring-emerald-200">
                          {c.email?.[0]?.toUpperCase() ?? "?"}
                        </div>
                        <div className="min-w-0 text-sm text-gray-900 break-words">
                          {c.email}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3 align-top">
                      <div className="max-w-[100ch] text-sm text-gray-700 whitespace-pre-wrap break-words">
                        {c.text}
                      </div>
                      <div className="mt-2 text-xs text-gray-500 space-y-1">
                        <div>Sukurta: {formatCommentTime(c.createdAt)}</div>
                        {c.updatedAt && c.updatedAt !== c.createdAt && (
                          <div>Redaguota: {formatCommentTime(c.updatedAt)}</div>
                        )}
                      </div>
                    </td>
                    <td className="w-28 px-4 py-3 align-top text-right">
                      {canEditComment(c) && (
                        <button
                          className="text-emerald-700 hover:text-emerald-900 font-medium text-sm"
                          onClick={() => setEdit(c)}
                        >
                          Redaguoti
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
                {comments.length === 0 && (
                  <tr>
                    <td className="px-4 py-6 text-sm text-gray-500" colSpan={3}>
                      Kol kas komentarų nėra.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {isAdmin() && (
          <div className="w-64 shrink-0">
            <div className="rounded-lg border border-emerald-200 bg-white shadow p-3">
              <div className="text-sm font-medium mb-2">Pažymėti trinimui</div>
              <div className="flex flex-col gap-2 mb-3 max-h-60 overflow-auto pr-1">
                {comments.map((c) => (
                  <label key={c.id} className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={c.id ? selected.has(c.id) : false}
                      onChange={() => toggleSelect(c.id)}
                    />
                    <span className="truncate" title={c.email}>
                      {c.email}
                    </span>
                  </label>
                ))}
                {comments.length === 0 && (
                  <div className="text-sm text-gray-500">Nėra ką pažymėti.</div>
                )}
              </div>
              <button
                type="button"
                className={`${formStyle.button} w-full`}
                onClick={bulkDelete}
                disabled={selected.size === 0}
                title={
                  selected.size === 0
                    ? "Nieko nepažymėta"
                    : "Ištrinti pažymėtus komentarus"
                }
              >
                Trinti pažymėtus
              </button>
            </div>
          </div>
        )}
      </div>

      {canPost ? (
        <form onSubmit={onSubmit} className="mt-6">
          <div className="rounded-lg border border-gray-200 bg-white shadow p-4 flex flex-col gap-3">
            <input type="hidden" {...register("id")} />
            <input type="hidden" {...register("plantId")} />

            <div className="text-xs text-gray-600">
              Rašysite kaip:{" "}
              <span className="font-medium text-gray-800">{currentEmail}</span>
            </div>

            <div className="max-w-xl">
              <label htmlFor="text" className={formStyle.label}>
                Rašyti komentarą
              </label>
              <textarea
                id="text"
                rows={3}
                placeholder={`Komentaras apie „${plantName}“...`}
                className={`${formStyle.input} max-w-xl`}
                {...register("text", { required: true, maxLength: 1000 })}
              />
            </div>

            {error && (
              <div className="text-sm text-red-700 bg-red-50 border border-red-200 rounded p-2">
                {error}
              </div>
            )}

            <div className="flex flex-col sm:flex-row gap-2">
              <button
                className={formStyle.button}
                type="submit"
                name="action"
                value="update"
                disabled={submitting}
              >
                {submitting ? "Išsaugoma..." : "Išsaugoti"}
              </button>
              <button
                className={formStyle.button}
                type="submit"
                name="action"
                value="new"
                disabled={submitting}
              >
                {submitting ? "Kuriama..." : "Naujas"}
              </button>
              <button
                className={formStyle.button}
                type="submit"
                name="action"
                value="delete"
                disabled={submitting}
              >
                {submitting ? "Trinama..." : "Ištrinti"}
              </button>
            </div>
          </div>
        </form>
      ) : null}
    </div>
  )
}

export default CommentsSection

import { useEffect, useMemo, useRef, useState } from "react"
import { IPlant } from "../../../interfaces/IPlant"
import { postApiRaw } from "../../../api"
import { formStyle } from "../../../styles/formStyle"

type Props = {
  plants: IPlant[]
  visiblePlants: IPlant[]
}

type ChatRole = "assistant" | "user"

type ChatMessage = {
  id: number
  role: ChatRole
  text: string
}

type PlantChatResponse = {
  reply: string
}

type PlantChatError = {
  message?: string
  detail?: string
}

const stripAccents = (value: string) =>
  value.normalize("NFD").replace(/[\u0300-\u036f]/g, "")

const normalize = (value: string) => stripAccents(value.toLowerCase()).trim()

const joinParts = (parts: string[]) => parts.filter(Boolean).join(", ")

const describePlant = (plant: IPlant) => {
  const parts = [
    plant.soilType ? `dirvožemis: ${plant.soilType}` : "",
    plant.pests ? `kenkėjai: ${plant.pests}` : "",
    plant.pestControlMethod ? `kontrolė: ${plant.pestControlMethod}` : "",
  ]

  return (
    joinParts(parts) || "Šiam augalui dar nėra daug papildomos informacijos."
  )
}

const findPlantMention = (question: string, plants: IPlant[]) => {
  const normalizedQuestion = normalize(question)

  return [...plants]
    .sort((a, b) => b.name.length - a.name.length)
    .find((plant) => normalizedQuestion.includes(normalize(plant.name)))
}

const buildReply = (
  question: string,
  plants: IPlant[],
  visiblePlants: IPlant[],
) => {
  const normalizedQuestion = normalize(question)
  const matchedPlant = findPlantMention(question, plants)
  const topVisiblePlants = visiblePlants.slice(0, 3)

  if (/^(labas|sveiki|hello|hey|hi)\b/.test(normalizedQuestion)) {
    return "Sveiki. Galiu papasakoti apie augalus, jų dirvožemį, kenkėjus arba pasiūlyti augalus iš šio sąrašo."
  }

  if (
    /rekomend|pasiul|kokius augalus|ka auginti|sodinti/.test(normalizedQuestion)
  ) {
    if (topVisiblePlants.length === 0) {
      return "Šiuo metu neturiu augalų sąrašo, bet galiu padėti, kai jis bus įkeltas."
    }

    const picks = topVisiblePlants
      .map(
        (plant) =>
          `${plant.name} (${plant.soilType || "be nurodyto dirvožemio"})`,
      )
      .join("; ")

    return `Iš šiuo metu matomų augalų siūlyčiau: ${picks}. Parašyk augalo pavadinimą, jei nori detalesnio paaiškinimo.`
  }

  if (/kenkej|pest/.test(normalizedQuestion)) {
    if (matchedPlant) {
      return `${matchedPlant.name}: ${matchedPlant.pests || "kenkėjai nenurodyti"}. ${matchedPlant.pestControlMethod ? `Kontrolė: ${matchedPlant.pestControlMethod}` : ""}`.trim()
    }

    return "Parašyk augalo pavadinimą, ir pasakysiu, kokie kenkėjai jam būdingi."
  }

  if (/dirvozem|zemes|soil/.test(normalizedQuestion)) {
    if (matchedPlant) {
      return `${matchedPlant.name} mėgsta ${matchedPlant.soilType || "dirvožemis nenurodytas"}. ${describePlant(matchedPlant)}`
    }

    const soils = Array.from(
      new Set(
        plants
          .map((plant) => plant.soilType?.trim())
          .filter((soil): soil is string => Boolean(soil)),
      ),
    ).slice(0, 6)

    return soils.length > 0
      ? `Šiame sąraše dažniausiai minimi dirvožemiai: ${soils.join(", ")}. Parašyk augalo pavadinimą, jei nori tikslaus atsakymo.`
      : "Dirvožemio tipų dar nerandu šiame sąraše."
  }

  if (
    /prieziur|aprasy|apie|care|auginti/.test(normalizedQuestion) &&
    matchedPlant
  ) {
    return `${matchedPlant.name}: ${matchedPlant.description || "aprašymo nėra"}. ${describePlant(matchedPlant)}`
  }

  if (matchedPlant) {
    return `${matchedPlant.name}: ${describePlant(matchedPlant)}${matchedPlant.description ? ` Aprašymas: ${matchedPlant.description}` : ""}`
  }

  const sampleNames =
    topVisiblePlants.length > 0
      ? topVisiblePlants.map((plant) => plant.name).join(", ")
      : plants
          .slice(0, 5)
          .map((plant) => plant.name)
          .join(", ")

  return sampleNames
    ? `Galiu padėti apie šiuos augalus: ${sampleNames}. Klausk apie pavadinimą, dirvožemį, kenkėjus arba priežiūrą.`
    : "Klausk manęs apie augalų priežiūrą, kenkėjus arba dirvožemį."
}

export function PlantChatbot({ plants, visiblePlants }: Props) {
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 1,
      role: "assistant",
      text: "Sveiki. Aš esu augalų asistentas. Paklausk apie augalą, dirvožemį, kenkėjus arba paprašyk pasiūlyti augalus iš sąrašo.",
    },
  ])
  const [draft, setDraft] = useState("")
  const [loading, setLoading] = useState(false)
  const [open, setOpen] = useState(false)
  const messageId = useRef(2)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" })
  }, [messages])

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setOpen(false)
      }
    }

    document.addEventListener("keydown", onKeyDown)
    return () => document.removeEventListener("keydown", onKeyDown)
  }, [])

  const quickPrompts = useMemo(() => {
    const prompts = [
      "Rekomenduok augalus",
      "Kokie kenkėjai dažniausiai puola augalus?",
    ]

    if (visiblePlants[0]) {
      prompts.unshift(`Papasakok apie ${visiblePlants[0].name}`)
    } else if (plants[0]) {
      prompts.unshift(`Papasakok apie ${plants[0].name}`)
    }

    return prompts.slice(0, 3)
  }, [plants, visiblePlants])

  const sendMessage = async (text: string) => {
    const trimmed = text.trim()
    if (!trimmed) {
      return
    }

    const userMessage: ChatMessage = {
      id: messageId.current++,
      role: "user",
      text: trimmed,
    }

    const assistantMessage: ChatMessage = {
      id: messageId.current++,
      role: "assistant",
      text: "Mąstau apie tavo klausimą...",
    }

    setMessages((current) => [...current, userMessage, assistantMessage])
    setDraft("")

    try {
      setLoading(true)
      const response = await postApiRaw("plantchat", {
        message: trimmed,
        plants: visiblePlants,
        includeInternet: true,
      })

      const okBody = response.bodyJson as PlantChatResponse | undefined
      const errBody = response.bodyJson as PlantChatError | undefined

      const fallback = buildReply(trimmed, plants, visiblePlants)
      const reply =
        response.ok && okBody?.reply?.trim()
          ? okBody.reply.trim()
          : [
              "AI atsakymas nepasiekiamas.",
              errBody?.message ? `Priežastis: ${errBody.message}` : "",
              errBody?.detail ? `Detalė: ${errBody.detail}` : "",
              "Parodau vietinį atsakymą:",
              fallback,
            ]
              .filter(Boolean)
              .join("\n")

      setMessages((current) => {
        const next = [...current]
        next[next.length - 1] = {
          ...next[next.length - 1],
          text: reply,
        }
        return next
      })
    } catch {
      const reply = [
        "AI ryšio klaida.",
        "Parodau vietinį atsakymą:",
        buildReply(trimmed, plants, visiblePlants),
      ].join("\n")
      setMessages((current) => {
        const next = [...current]
        next[next.length - 1] = {
          ...next[next.length - 1],
          text: reply,
        }
        return next
      })
    } finally {
      setLoading(false)
    }
  }

  const onSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void sendMessage(draft)
  }

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        className="fixed right-0 top-2/3 z-40 -translate-y-1/2 rounded-l-2xl border border-r-0 border-emerald-300 bg-emerald-600 px-3 py-4 text-sm font-semibold text-white shadow-lg transition hover:bg-emerald-700"
        aria-expanded={open}
        aria-controls="plants-chat-panel"
      >
        {open ? "Uždaryti pokalbį" : "Pokalbis su DI"}
      </button>

      {open && (
        <button
          type="button"
          aria-label="Uždaryti pokalbį"
          className="fixed inset-0 z-30 bg-black/25 lg:hidden"
          onClick={() => setOpen(false)}
        />
      )}

      {open && (
        <section
          id="plants-chat-panel"
          className="fixed bottom-3 right-3 z-40 flex h-[34rem] w-[22rem] max-w-[calc(100vw-1.5rem)] flex-col overflow-hidden rounded-2xl border border-emerald-200 bg-white shadow-2xl sm:bottom-4 sm:right-4 sm:w-[24rem]"
          role="dialog"
          aria-label="Augalų pokalbių asistentas"
        >
          <div className="border-b border-emerald-100 bg-linear-to-r from-emerald-50 to-lime-50 px-4 py-3">
            <div className="flex items-center justify-between gap-2">
              <div className="text-sm font-semibold text-emerald-950">
                Augalų asistentas
              </div>
              <button
                type="button"
                className="rounded-md px-2 py-1 text-xs font-medium text-emerald-800 hover:bg-emerald-100"
                onClick={() => setOpen(false)}
              >
                Uždaryti
              </button>
            </div>
            <div className="mt-1 text-xs text-emerald-900/80">
              Klausk apie priežiūrą, dirvožemį ir kenkėjus.
            </div>
          </div>

          <div className="flex-1 space-y-3 overflow-auto px-3 py-3">
            {messages.map((message) => (
              <div
                key={message.id}
                className={`flex ${message.role === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[90%] rounded-2xl px-3 py-2 text-sm leading-5 shadow-sm ${
                    message.role === "user"
                      ? "bg-emerald-600 text-white"
                      : "bg-gray-100 text-gray-800"
                  }`}
                >
                  {message.text}
                </div>
              </div>
            ))}
            <div ref={bottomRef} />
          </div>

          <div className="border-t border-gray-200 px-3 py-2">
            <div className="mb-2 flex flex-wrap gap-1">
              {quickPrompts.map((prompt) => (
                <button
                  key={prompt}
                  type="button"
                  className="rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-1 text-xs text-emerald-900 hover:bg-emerald-100"
                  onClick={() => void sendMessage(prompt)}
                  disabled={loading}
                >
                  {prompt}
                </button>
              ))}
            </div>

            <form onSubmit={onSubmit}>
              <div className="flex gap-2">
                <input
                  id="plant-chat-input"
                  value={draft}
                  onChange={(event) => setDraft(event.target.value)}
                  placeholder="Klausimas apie augalus..."
                  className={`${formStyle.input} flex-1`}
                />
                <button
                  type="submit"
                  className={`${formStyle.button} w-auto px-3`}
                  disabled={loading}
                >
                  {loading ? "..." : "Siųsti"}
                </button>
              </div>
            </form>
          </div>
        </section>
      )}
    </>
  )
}

export default PlantChatbot

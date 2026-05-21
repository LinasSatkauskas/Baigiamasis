import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"

let connection: HubConnection | undefined

function hubUrl() {
  return "/hubs/comments"
}

export async function ensureCommentsHubConnection(): Promise<HubConnection> {
  if (connection && connection.state !== HubConnectionState.Disconnected) {
    return connection
  }

  connection = new HubConnectionBuilder()
    .withUrl(hubUrl(), { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  connection.onreconnected(async () => {
    // Caller will re-join the current plant group via the component effect.
  })

  if (connection.state === HubConnectionState.Disconnected) {
    await connection.start()
  }

  return connection
}

export async function stopCommentsHubConnection() {
  if (!connection) return

  try {
    await connection.stop()
  } finally {
    connection = undefined
  }
}

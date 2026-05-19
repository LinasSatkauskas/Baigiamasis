Security notes — Gemini API key

This project previously contained a Gemini API key in `appsettings.Development.json`. That key has been removed from the file and should be rotated immediately in Google Cloud Console.

Local developer instructions

- Do NOT commit secrets. Use one of these methods to provide the key locally:
  - `dotnet user-secrets` (recommended for development):
    - `cd ReactApp1.Server`
    - `dotnet user-secrets init`
    - `dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY"`
  - Environment variable (temporary):
    - PowerShell: `$env:Gemini__ApiKey = "YOUR_API_KEY"`
    - Or set system env var via `setx Gemini__ApiKey "YOUR_API_KEY"`

Rotate the key in Google Cloud Console and restrict it to the Generative Language API and to appropriate IPs/service accounts.

If the key was pushed to remote already, rotate the key now and consider removing it from Git history with a tool like `git filter-repo` or the BFG Repo-Cleaner.

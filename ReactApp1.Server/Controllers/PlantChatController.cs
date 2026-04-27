using System.Net.Http.Headers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/plantchat")]
public class PlantChatController(IConfiguration configuration, IHttpClientFactory httpClientFactory) : ControllerBase
{
    private sealed record GeminiResult(bool Success, string? Reply, string? Error);
    private sealed record ModelListResult(List<string> Models, string? Error);

    [HttpPost]
    public async Task<ActionResult<PlantChatResponseDto>> Post([FromBody] PlantChatRequestDto request)
    {
        request ??= new PlantChatRequestDto(string.Empty, [], true);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Gemini API key is not configured." });
        }

        var configuredModel = configuration["Gemini:Model"];
        var modelResult = await GetCandidateModelsAsync(apiKey, configuredModel);
        var candidateModels = modelResult.Models;
        var internetContext = request.IncludeInternet
            ? await TryBuildInternetContextAsync(request.Message, request.Plants)
            : null;

        if (candidateModels.Count == 0)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "No supported Gemini model was found for this API key.",
                detail = modelResult.Error
            });
        }

        GeminiResult? lastFailure = null;
        foreach (var model in candidateModels)
        {
            var result = await AskGeminiAsync(apiKey, model, request, internetContext);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Reply))
            {
                return Ok(new PlantChatResponseDto(result.Reply));
            }

            lastFailure = result;
        }

        return StatusCode(StatusCodes.Status502BadGateway, new
        {
            message = "Gemini did not return a valid response.",
            detail = lastFailure?.Error
        });
    }

    private async Task<ModelListResult> GetCandidateModelsAsync(string apiKey, string? configuredModel)
    {
        var preferred = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            preferred.Add(configuredModel);
        }

        var defaults = new[]
        {
            "gemini-2.5-flash",
            "gemini-2.5-pro",
            "gemini-2.0-flash",
            "gemini-1.5-flash"
        };
        preferred.AddRange(defaults);

        var discovered = new List<string>();
        string? discoverError = null;

        try
        {
            var client = httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={Uri.EscapeDataString(apiKey)}";
            using var response = await client.GetAsync(url);
            var payload = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                discoverError = $"Model discovery failed with {(int)response.StatusCode}: {payload}";
            }
            else
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("models", out var modelsEl) && modelsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var model in modelsEl.EnumerateArray())
                    {
                        if (!model.TryGetProperty("name", out var nameEl))
                        {
                            continue;
                        }

                        var rawName = nameEl.GetString();
                        if (string.IsNullOrWhiteSpace(rawName) || !rawName.StartsWith("models/gemini-", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!model.TryGetProperty("supportedGenerationMethods", out var methodsEl) || methodsEl.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        var supportsGenerate = methodsEl.EnumerateArray().Any(m =>
                            string.Equals(m.GetString(), "generateContent", StringComparison.OrdinalIgnoreCase));

                        if (!supportsGenerate)
                        {
                            continue;
                        }

                        discovered.Add(rawName.Replace("models/", string.Empty, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            discoverError = $"Model discovery failed: {ex.Message}";
        }

        var ranked = preferred
            .Concat(discovered)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(name => name.Contains("flash", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(name => name.Contains("2.5", StringComparison.OrdinalIgnoreCase))
            .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        return new ModelListResult(ranked, discoverError);
    }

    private async Task<GeminiResult> AskGeminiAsync(string apiKey, string model, PlantChatRequestDto request, string? internetContext)
    {
        var prompt = BuildPrompt(request, internetContext);
        var primaryResult = await CallGeminiAsync(apiKey, model, prompt, maxOutputTokens: 900);
        if (!primaryResult.Success || string.IsNullOrWhiteSpace(primaryResult.Reply))
        {
            return primaryResult;
        }

        if (!IsLikelyTruncated(primaryResult.Reply, primaryResult.Error))
        {
            return primaryResult;
        }

        return await CallGeminiAsync(
            apiKey,
            model,
            $"{prompt}\n\nSvarbu: jei ankstesnis atsakymas nutrūko, užbaik mintį iki pilno sakinio.",
            maxOutputTokens: 1400);
    }

    private static string BuildPrompt(PlantChatRequestDto request, string? internetContext)
    {
        var plants = (request.Plants ?? []).Take(25).ToList();
        var plantLines = plants.Count == 0
            ? "Nėra pateiktų augalų."
            : string.Join('\n', plants.Select(plant =>
                $"- {plant.Name}: {FormatField("aprašymas", plant.Description)}; {FormatField("dirvožemis", plant.SoilType)}; {FormatField("kenkėjai", plant.Pests)}; {FormatField("kontrolė", plant.PestControlMethod)}"));

        var internetSection = string.IsNullOrWhiteSpace(internetContext)
            ? "Interneto kontekstas: nepateikta."
            : $"Interneto kontekstas:\n{internetContext}";

        return $"Vartotojo klausimas: {request.Message}\n\nPateikti augalai:\n{plantLines}\n\n{internetSection}\n\nUžduotis: atsakyk natūraliai, padėk apie augalus, jų priežiūrą, dirvožemį ir kenkėjus. Jei vartotojas mini konkretų augalą ir yra sąrašo duomenys, naudok juos kaip pirminį šaltinį. Jei klausimas bendrinis arba sąraše nėra atsakymo, gali naudoti interneto kontekstą.";
    }

    private async Task<string?> TryBuildInternetContextAsync(string userMessage, IReadOnlyList<PlantChatPlantDto>? plants)
    {
        var queries = BuildInternetQueries(userMessage, plants);
        if (queries.Count == 0)
        {
            return null;
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            if (!client.DefaultRequestHeaders.UserAgent.Any())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ReactApp1-PlantChat/1.0");
            }

            var snippets = new List<string>();
            var domains = new[] { "lt.wikipedia.org", "en.wikipedia.org" };

            foreach (var query in queries)
            {
                foreach (var domain in domains)
                {
                    var snippet = await TryFetchWikipediaSummaryAsync(client, domain, query);
                    if (!string.IsNullOrWhiteSpace(snippet))
                    {
                        snippets.Add(snippet);
                    }

                    if (snippets.Count >= 2)
                    {
                        break;
                    }
                }

                if (snippets.Count >= 2)
                {
                    break;
                }
            }

            return snippets.Count == 0 ? null : string.Join("\n\n", snippets);
        }
        catch
        {
            return null;
        }
    }

    private async Task<GeminiResult> CallGeminiAsync(string apiKey, string model, string prompt, int maxOutputTokens)
    {
        var payload = new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = "Tu esi augalų asistentas. Atsakyk lietuviškai, trumpai ir aiškiai. Nenaudok markdown simbolių, jei to neprašo vartotojas. Atsakymui prioritetas: 1) pateiktas vartotojo augalų sąrašas, 2) interneto kontekstas bendrai informacijai. Jei atsakai naudodamas interneto kontekstą, trumpai paminėk tai atsakymo pabaigoje. Jei duomenų trūksta, aiškiai pasakyk, ko trūksta, užuot spėjęs." }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens
            }
        };

        var client = httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(httpRequest);
        var responseText = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return new GeminiResult(false, null, $"Model {model} returned {(int)response.StatusCode}: {responseText}");
        }

        using var document = JsonDocument.Parse(responseText);
        if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return new GeminiResult(false, null, $"Model {model} returned no candidates.");
        }

        var candidate = candidates[0];
        var finishReason = candidate.TryGetProperty("finishReason", out var finishReasonEl)
            ? finishReasonEl.GetString()
            : null;

        if (!candidate.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
        {
            return new GeminiResult(false, null, $"Model {model} returned an empty content payload.");
        }

        var builder = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textPart))
            {
                builder.Append(textPart.GetString());
            }
        }

        var reply = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(reply)
            ? new GeminiResult(false, null, $"Model {model} returned empty text.")
            : new GeminiResult(true, reply, finishReason);
    }

    private async Task<string?> TryFetchWikipediaSummaryAsync(HttpClient client, string domain, string query)
    {
        var searchUrl = $"https://{domain}/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&srlimit=1&format=json";
        using var searchResponse = await client.GetAsync(searchUrl);
        if (!searchResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var searchPayload = await searchResponse.Content.ReadAsStringAsync();
        using var searchDoc = JsonDocument.Parse(searchPayload);

        if (!searchDoc.RootElement.TryGetProperty("query", out var queryNode)
            || !queryNode.TryGetProperty("search", out var searchNode)
            || searchNode.ValueKind != JsonValueKind.Array
            || searchNode.GetArrayLength() == 0)
        {
            return null;
        }

        var first = searchNode[0];
        var title = first.TryGetProperty("title", out var titleEl)
            ? titleEl.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var summaryUrl = $"https://{domain}/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";
        using var summaryResponse = await client.GetAsync(summaryUrl);
        if (!summaryResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var summaryPayload = await summaryResponse.Content.ReadAsStringAsync();
        using var summaryDoc = JsonDocument.Parse(summaryPayload);

        var pageTitle = summaryDoc.RootElement.TryGetProperty("title", out var pageTitleEl)
            ? pageTitleEl.GetString()
            : title;

        var extract = summaryDoc.RootElement.TryGetProperty("extract", out var extractEl)
            ? extractEl.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(extract))
        {
            return null;
        }

        string? pageUrl = null;
        if (summaryDoc.RootElement.TryGetProperty("content_urls", out var urlsEl)
            && urlsEl.TryGetProperty("desktop", out var desktopEl)
            && desktopEl.TryGetProperty("page", out var pageEl))
        {
            pageUrl = pageEl.GetString();
        }

        var trimmedExtract = extract.Trim();
        if (trimmedExtract.Length > 700)
        {
            trimmedExtract = trimmedExtract[..700].TrimEnd() + "...";
        }

        var source = string.IsNullOrWhiteSpace(pageUrl)
            ? $"Wikipedia ({domain})"
            : $"Wikipedia ({pageUrl})";

        return $"Tema: {pageTitle}\nSantrauka: {trimmedExtract}\nŠaltinis: {source}";
    }

    private static bool IsLikelyTruncated(string reply, string? finishReason)
    {
        if (string.Equals(finishReason, "MAX_TOKENS", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var text = reply.TrimEnd();
        if (text.Length == 0)
        {
            return false;
        }

        var last = text[^1];
        if (last is '.' or '!' or '?' or '"' or ')' or ']')
        {
            return false;
        }

        if (text.EndsWith(",", StringComparison.Ordinal)
            || text.EndsWith(";", StringComparison.Ordinal)
            || text.EndsWith(":", StringComparison.Ordinal)
            || text.EndsWith(" -", StringComparison.Ordinal)
            || text.EndsWith(" ir", StringComparison.OrdinalIgnoreCase)
            || text.EndsWith(" tik", StringComparison.OrdinalIgnoreCase)
            || text.EndsWith(" bet", StringComparison.OrdinalIgnoreCase)
            || text.EndsWith(" kad", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static List<string> BuildInternetQueries(string userMessage, IReadOnlyList<PlantChatPlantDto>? plants)
    {
        var queries = new List<string>();

        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            var matchFromMessage = FindPlantInText(userMessage, plants);
            if (!string.IsNullOrWhiteSpace(matchFromMessage))
            {
                queries.Add(matchFromMessage);
            }

            queries.Add(userMessage.Trim());
        }

        var normalized = NormalizeForLookup(userMessage);
        if (normalized.Contains("darzov", StringComparison.Ordinal))
        {
            queries.Add("Vegetable");
            queries.Add("Vegetable gardening");
        }

        if (normalized.Contains("kenkej", StringComparison.Ordinal) || normalized.Contains("pest", StringComparison.Ordinal))
        {
            queries.Add("Plant pest");
            queries.Add("Common garden pests");
        }

        if (normalized.Contains("dirvozem", StringComparison.Ordinal) || normalized.Contains("soil", StringComparison.Ordinal))
        {
            queries.Add("Soil for plants");
            queries.Add("Soil pH for gardening");
        }

        if (normalized.Contains("prieziur", StringComparison.Ordinal)
            || normalized.Contains("auginti", StringComparison.Ordinal)
            || normalized.Contains("care", StringComparison.Ordinal)
            || normalized.Contains("grow", StringComparison.Ordinal))
        {
            queries.Add("Plant care");
            queries.Add("How to grow plants");
        }

        if (queries.Count == 0)
        {
            queries.Add("Gardening");
        }

        return queries
            .Where(query => !string.IsNullOrWhiteSpace(query))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();
    }

    private static string? FindPlantInText(string text, IReadOnlyList<PlantChatPlantDto>? plants)
    {
        if (plants is null || plants.Count == 0 || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalizedText = NormalizeForLookup(text);

        var bestMatch = plants
            .Where(plant => !string.IsNullOrWhiteSpace(plant.Name))
            .OrderByDescending(plant => plant.Name.Length)
            .FirstOrDefault(plant => normalizedText.Contains(NormalizeForLookup(plant.Name)));

        return bestMatch?.Name;
    }

    private static string NormalizeForLookup(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string FormatField(string label, string? value)
        => string.IsNullOrWhiteSpace(value) ? $"{label}: nenurodyta" : $"{label}: {value}";
}
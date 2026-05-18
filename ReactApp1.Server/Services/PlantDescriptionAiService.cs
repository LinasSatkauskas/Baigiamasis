using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ReactApp1.Server.Services;

public class PlantDescriptionAiService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IMemoryCache memoryCache,
    ILogger<PlantDescriptionAiService> logger) : IPlantDescriptionAiService
{
    public async Task<string?> GenerateAsync(string plantName, string? soilType, string? pests)
    {
        if (string.IsNullOrWhiteSpace(plantName))
        {
            return null;
        }

        var key = $"plant-desc-ai:{plantName.Trim().ToLowerInvariant()}";
        if (memoryCache.TryGetValue<string>(key, out var cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        var apiKey = configuration["Gemini:ApiKey"];
        var configuredModel = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Gemini API key is missing. Cannot generate description for plant {PlantName}.", plantName);
            return null;
        }

        var prompt = BuildPrompt(plantName, soilType, pests);

        try
        {
            var client = httpClientFactory.CreateClient();
            var candidateModels = new[] { configuredModel, "gemini-2.5-flash", "gemini-2.0-flash" }
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            string? text = null;
            foreach (var model in candidateModels)
            {
                var result = await TryGenerateWithGeminiAsync(client, apiKey, model, prompt);
                text = result.Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    break;
                }

                logger.LogWarning(
                    "Gemini description generation failed for plant {PlantName} with model {Model}. Reason: {Reason}",
                    plantName,
                    model,
                    result.Error ?? "Unknown error");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogWarning("All Gemini models failed to generate description for plant {PlantName}. Falling back to Wikipedia.", plantName);
                text = await TryGenerateWithWikipediaAsync(client, plantName);
            }

            var clean = Cleanup(text!);
            if (string.IsNullOrWhiteSpace(clean))
            {
                logger.LogWarning("Gemini returned empty description after cleanup for plant {PlantName}.", plantName);
                return null;
            }

            if (IsPlaceholderDescription(clean))
            {
                logger.LogWarning("Gemini returned placeholder-only description for plant {PlantName}.", plantName);
                return null;
            }

            // If the AI returned an excessively short or truncated result (e.g. 2-3 words),
            // prefer the first full sentence from Wikipedia to ensure a useful description.
            if (IsTooShortOrNotSentence(clean))
            {
                logger.LogInformation("Generated description was too short; attempting Wikipedia fallback for {PlantName}.", plantName);
                var wiki = await TryGenerateWithWikipediaAsync(client, plantName);
                if (!string.IsNullOrWhiteSpace(wiki))
                {
                    var first = ExtractFirstSentences(wiki, 1);
                        if (!string.IsNullOrWhiteSpace(first))
                        {
                            clean = Cleanup(first);
                        }
                }
            }

            memoryCache.Set(key, clean, TimeSpan.FromHours(24));
            return clean;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while generating description for plant {PlantName}.", plantName);
            return null;
        }
    }

    private static string BuildPrompt(string plantName, string? soilType, string? pests)
    {
        var soil = string.IsNullOrWhiteSpace(soilType) ? "nenurodyta" : soilType.Trim();
        var pestsInfo = string.IsNullOrWhiteSpace(pests) ? "nenurodyta" : pests.Trim();

        return $"Parašyk 2-4 sakinių augalo aprašymą lietuviškai apie: {plantName}. Jei įmanoma, paminėk bendrą augalo paskirtį, augimo sąlygas ir priežiūrą. Papildoma informacija: dirvožemis={soil}; kenkėjai={pestsInfo}. Jei kažko tiksliai nežinai, nefantazuok.";
    }

    private async Task<string?> TryGenerateWithWikipediaAsync(HttpClient client, string plantName)
    {
        if (!client.DefaultRequestHeaders.UserAgent.Any())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ReactApp1-PlantDescription/1.0");
        }

        var queries = new[] { plantName, $"{plantName} augalas", $"{plantName} plant" };
        var domains = new[] { "lt.wikipedia.org", "en.wikipedia.org" };

        foreach (var query in queries)
        {
            foreach (var domain in domains)
            {
                var summary = await TryFetchWikipediaSummaryAsync(client, domain, query);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    return summary;
                }
            }
        }

        return null;
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

        if (!searchDoc.RootElement.TryGetProperty("query", out var queryEl)
            || !queryEl.TryGetProperty("search", out var searchEl)
            || searchEl.ValueKind != JsonValueKind.Array
            || searchEl.GetArrayLength() == 0)
        {
            return null;
        }

        var title = searchEl[0].TryGetProperty("title", out var titleEl)
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

        var extract = summaryDoc.RootElement.TryGetProperty("extract", out var extractEl)
            ? extractEl.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(extract))
        {
            return null;
        }

        return extract.Trim();
    }

    private static object BuildGeminiPayload(string prompt)
    {
        return new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = "Tu esi augalų ekspertas. Atsakyk lietuviškai, aiškiai ir trumpai. Nenaudok markdown formato simbolių." }
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
                maxOutputTokens = 220
            }
        };
    }

    private async Task<(string? Text, string? Error)> TryGenerateWithGeminiAsync(HttpClient client, string apiKey, string model, string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(BuildGeminiPayload(prompt)), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return (null, $"HTTP {(int)response.StatusCode}: {Truncate(content, 260)}");
        }

        using var doc = JsonDocument.Parse(content);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.ValueKind != JsonValueKind.Array
            || candidates.GetArrayLength() == 0)
        {
            return (null, "No candidates returned");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var candidateContent)
            || !candidateContent.TryGetProperty("parts", out var parts)
            || parts.ValueKind != JsonValueKind.Array)
        {
            return (null, "Candidate content parts missing");
        }

        var builder = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textNode))
            {
                builder.Append(textNode.GetString());
            }
        }

        return builder.Length == 0
            ? (null, "Candidate text is empty")
            : (builder.ToString(), null);
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = value.Trim();
        return text.Length <= max ? text : text[..max] + "...";
    }

    private static string Cleanup(string text)
    {
        var normalized = StripLeadingBullets(text.Trim());
        if (normalized.Length > 420)
        {
            normalized = normalized[..420].TrimEnd() + "...";
        }

        return normalized
            .Replace("**", string.Empty, StringComparison.Ordinal)
            .Replace("__", string.Empty, StringComparison.Ordinal)
            .Trim();
    }

    private static bool IsPlaceholderDescription(string value)
    {
        var trimmed = value.Trim();
        return trimmed is "-" or "–" or "—";
    }

    private static bool IsTooShortOrNotSentence(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 6)
            return true;

        // Check for sentence ending punctuation
        if (!text.Contains('.') && !text.Contains('!') && !text.Contains('?'))
            return true;

        return false;
    }

    private static string? ExtractFirstSentences(string text, int count)
    {
        if (string.IsNullOrWhiteSpace(text) || count <= 0) return null;
        var sentences = new List<string>();
        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            sb.Append(ch);
            if (ch == '.' || ch == '!' || ch == '?')
            {
                var s = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    sentences.Add(s);
                    sb.Clear();
                    if (sentences.Count >= count) break;
                }
            }
        }

        if (sentences.Count > 0)
        {
            return string.Join(" ", sentences).Trim();
        }

        // No sentence punctuation found — return a reasonable prefix
        var trimmed = text.Trim();
        return trimmed.Length <= 200 ? trimmed : trimmed.Substring(0, 200).TrimEnd() + "...";
    }

    private static string StripLeadingBullets(string text)
    {
        var lines = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                if (line.StartsWith("- ", StringComparison.Ordinal)
                    || line.StartsWith("• ", StringComparison.Ordinal)
                    || line.StartsWith("* ", StringComparison.Ordinal))
                {
                    return line[2..].Trim();
                }

                return line;
            })
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return lines.Count == 0 ? string.Empty : string.Join(" ", lines);
    }
}

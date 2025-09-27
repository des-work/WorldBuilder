using Serilog;
using Genisis.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Genisis.Infrastructure.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string ModelsCacheKey = "ollama_models";

    public OllamaAiService(IMemoryCache cache)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        _cache = cache;
    }

    public async Task<List<string>> GetLocalModelsAsync(CancellationToken cancellationToken = default)
    {
        // Try to get from memory cache first
        if (_cache.TryGetValue(ModelsCacheKey, out List<string>? cachedModels))
        {
            return cachedModels ?? new List<string>();
        }

        try
        {
            // Set a reasonable timeout for the HTTP request
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout

            var response = await _httpClient.GetAsync("/api/tags", cts.Token);
            if (!response.IsSuccessStatusCode) return new List<string>();

            var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cts.Token);
            var models = json?["models"]?.AsArray()
                .Select(m => m?["name"]?.GetValue<string>() ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var result = models ?? new List<string>();

            // Cache the result for 30 minutes (or until manually cleared)
            _cache.Set(ModelsCacheKey, result, TimeSpan.FromMinutes(30));

            return result;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get local Ollama models. Ollama may not be running. Continuing without AI features.");

            // Cache empty list for 5 minutes to avoid repeated failed attempts
            var emptyResult = new List<string>();
            _cache.Set(ModelsCacheKey, emptyResult, TimeSpan.FromMinutes(5));

            return emptyResult;
        }
    }

    public void ClearModelCache()
    {
        _cache.Remove(ModelsCacheKey);
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestData = new
        {
            model,
            prompt,
            stream = true // Enable streaming
        };

        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate") { Content = content };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Log.Error("Ollama API error: {Error}", error);
            yield return $"Error: Could not get a response from model '{model}'.";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line)) continue;

            var json = JsonNode.Parse(line);
            var chunk = json?["response"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(chunk))
            {
                yield return chunk;
            }
        }
    }
}

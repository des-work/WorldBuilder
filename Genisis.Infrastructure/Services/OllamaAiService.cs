using Serilog;
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
using Genisis.Core.Services;

namespace Genisis.Infrastructure.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private List<string>? _cachedModels;

    public OllamaAiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
    }

    public async Task<List<string>> GetLocalModelsAsync()
    {
        // Return cached models if available
        if (_cachedModels != null)
            return _cachedModels;

        try
        {
            var response = await _httpClient.GetAsync("/api/tags");
            if (!response.IsSuccessStatusCode) return new List<string>();

            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            var models = json?["models"]?.AsArray()
                .Select(m => m?["name"]?.GetValue<string>() ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            _cachedModels = models ?? new List<string>();
            return _cachedModels;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get local Ollama models. Is Ollama running?");
            return new List<string>(); // Return empty list on error
        }
    }

    public void ClearModelCache()
    {
        _cachedModels = null;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get completion from Ollama.");
            yield return "Error: Could not connect to the local AI service. Is Ollama running?";
        }
    }
}

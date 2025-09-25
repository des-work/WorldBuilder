using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Genisis.App.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public OllamaAiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
    }

    public async Task<List<string>> GetLocalModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags");
            if (!response.IsSuccessStatusCode) return new List<string>();

            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            var models = json?["models"]?.AsArray()
                .Select(m => m?["name"]?.GetValue<string>() ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            return models ?? new List<string>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get local Ollama models. Is Ollama running?");
            return new List<string>(); // Return empty list on error
        }
    }

    public async Task<string> GetCompletionAsync(string model, string prompt)
    {
        try
        {
            var requestData = new
            {
                model,
                prompt,
                stream = false // For now, we'll get the full response at once
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("Ollama API error: {Error}", error);
                return $"Error: Could not get a response from model '{model}'.";
            }

            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            return json?["response"]?.GetValue<string>() ?? "No response content received.";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get completion from Ollama.");
            return "Error: Could not connect to the local AI service. Is Ollama running?";
        }
    }
}
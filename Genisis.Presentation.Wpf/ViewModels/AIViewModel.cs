using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Genisis.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genisis.Presentation.Wpf.ViewModels;

public record ChatMessage(string Role, string Content);

public partial class AIViewModel : ViewModelBase
{
    private readonly IAiService _aiService;
    private string _systemPrompt = string.Empty;
    private List<ChatMessage> _chatHistory = new();

    [ObservableProperty]
    private string _interactionTitle = "Ask Your Universe";

    public ObservableCollection<string> AvailableModels { get; } = new();

    [ObservableProperty]
    private string? _selectedModel;

    [ObservableProperty]
    private string _userQuery = string.Empty;

    [ObservableProperty]
    private string _aiResponse = "Select a model and ask a question about your selected item.";

    [ObservableProperty]
    private bool _isLoading;

    public RelayCommand LoadModelsCommand { get; }
    public RelayCommand ClearHistoryCommand { get; }
    public RelayCommand SendQueryCommand { get; }

    public AIViewModel(IAiService aiService)
    {
        _aiService = aiService;
        LoadModelsCommand = new RelayCommand(async _ => await LoadModelsAsync());
        ClearHistoryCommand = new RelayCommand(_ => ClearHistory());
        SendQueryCommand = new RelayCommand(async _ => await SendQueryAsync(), _ => !IsLoading && !string.IsNullOrEmpty(SelectedModel));
    }

    private bool CanSendQuery() => !IsLoading && !string.IsNullOrEmpty(SelectedModel);

    public void UpdateInteraction(string title, string systemPrompt)
    {
        InteractionTitle = title;
        _systemPrompt = systemPrompt;
        ClearHistory(); // Clear history when context changes
    }

    private async Task LoadModelsAsync()
    {
        // Only load if we haven't loaded models before
        if (AvailableModels.Any()) return;

        AvailableModels.Clear();
        var models = await _aiService.GetLocalModelsAsync();
        foreach (var model in models)
        {
            AvailableModels.Add(model);
        }
        // Select the first model by default if any exist
        if (AvailableModels.Any() && SelectedModel is null)
        {
            SelectedModel = AvailableModels.First();
        }
    }

    private void ClearHistory()
    {
        _chatHistory.Clear();
        AiResponse = "Select a model and ask a question about your selected item.";
    }

    private async Task SendQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(UserQuery) || SelectedModel is null) return;

        IsLoading = true;

        // Add user query to history and update display
        var currentQuery = UserQuery;
        _chatHistory.Add(new ChatMessage("User", currentQuery));
        AiResponse = BuildResponseDisplay();
        UserQuery = string.Empty; // Clear the input box immediately

        var finalPrompt = BuildPrompt();
        var fullBotResponse = new StringBuilder();

        await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, finalPrompt))
        {
            fullBotResponse.Append(chunk);
            AiResponse = BuildResponseDisplay() + fullBotResponse;
        }

        _chatHistory.Add(new ChatMessage("Assistant", fullBotResponse.ToString()));
        IsLoading = false;
    }

    private string BuildPrompt()
    {
        var sb = new StringBuilder(_systemPrompt);
        foreach (var message in _chatHistory)
        {
            sb.AppendLine($"\n\n{message.Role}: {message.Content}");
        }
        sb.Append("\n\nAssistant:");
        return sb.ToString();
    }

    private string BuildResponseDisplay()
    {
        var sb = new StringBuilder();
        foreach (var message in _chatHistory)
        {
            sb.AppendLine($"{message.Role}:\n{message.Content}\n");
        }
        return sb.ToString();
    }
}

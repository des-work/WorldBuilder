using Genisis.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.Presentation.Wpf.ViewModels;

public record ChatMessage(string Role, string Content);

public class AIViewModel : ViewModelBase
{
    private readonly IAiService _aiService;
    private string _systemPrompt = string.Empty;
    private List<ChatMessage> _chatHistory = new();

    private string _interactionTitle = "Ask Your Universe";
    public string InteractionTitle
    {
        get => _interactionTitle;
        set { _interactionTitle = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> AvailableModels { get; } = new();

    private string? _selectedModel;
    public string? SelectedModel
    {
        get => _selectedModel;
        set { _selectedModel = value; OnPropertyChanged(); (SendQueryCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
    }

    private string _userQuery = string.Empty;
    public string UserQuery
    {
        get => _userQuery;
        set { _userQuery = value; OnPropertyChanged(); }
    }

    private string _aiResponse = "Select a model and ask a question about your selected item.";
    public string AiResponse
    {
        get => _aiResponse;
        set { _aiResponse = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); (SendQueryCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
    }

    public ICommand LoadModelsCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand SendQueryCommand { get; }

    public AIViewModel(IAiService aiService)
    {
        _aiService = aiService;
        LoadModelsCommand = new RelayCommand(async _ => await LoadModelsAsync());
        ClearHistoryCommand = new RelayCommand(_ => ClearHistory());
        SendQueryCommand = new RelayCommand(async _ => await SendQueryAsync(), _ => !IsLoading && !string.IsNullOrEmpty(SelectedModel));
    }

    public void UpdateInteraction(string title, string systemPrompt)
    {
        InteractionTitle = title;
        _systemPrompt = systemPrompt;
        ClearHistory(); // Clear history when context changes
    }

    private async Task LoadModelsAsync()
    {
        AvailableModels.Clear();
        var models = await _aiService.GetLocalModelsAsync();
        foreach (var model in models)
        {
            AvailableModels.Add(model);
        }
        // Select the first model by default if any exist
        if (AvailableModels.Any())
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

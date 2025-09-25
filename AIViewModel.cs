using Genisis.App.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class AIViewModel : ViewModelBase
{
    private readonly IAiService _aiService;
    private string _systemPrompt = string.Empty;

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
    public ICommand SendQueryCommand { get; }

    public AIViewModel(IAiService aiService)
    {
        _aiService = aiService;
        LoadModelsCommand = new RelayCommand(async _ => await LoadModelsAsync());
        SendQueryCommand = new RelayCommand(async _ => await SendQueryAsync(), _ => !IsLoading && !string.IsNullOrEmpty(SelectedModel));
    }

    public void UpdateInteraction(string title, string systemPrompt)
    {
        InteractionTitle = title;
        _systemPrompt = systemPrompt;
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

    private async Task SendQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(UserQuery) || SelectedModel is null) return;

        IsLoading = true;
        AiResponse = "Thinking...";

        var finalPrompt = $"{_systemPrompt}\n\nUser: {UserQuery}\n\nResponse:";
        
        // Clear the response and prepare for streaming
        AiResponse = string.Empty;

        await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, finalPrompt))
        {
            AiResponse += chunk;
        }

        IsLoading = false;
    }
}
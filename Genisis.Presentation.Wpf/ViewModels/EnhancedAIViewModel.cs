using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Genisis.Core.Services;
using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// Enhanced AI ViewModel with context awareness and proactive assistance
/// </summary>
public class EnhancedAIViewModel : ViewModelBase
{
    private readonly IAiService _aiService;
    private readonly IPromptGenerationService _promptGenerationService;

    private NonLinearStoryViewModel? _storyViewModel;
    private string _userQuery = string.Empty;
    private string _aiResponse = "Welcome! I'm your AI writing assistant. I can help you develop your story, characters, and world. What would you like to work on?";
    private bool _isLoading;
    private string _selectedModel = string.Empty;
    private ObservableCollection<string> _availableModels = new();
    private ObservableCollection<AISuggestion> _suggestions = new();
    private ObservableCollection<ChatMessage> _chatHistory = new();

    public EnhancedAIViewModel(IAiService aiService, IPromptGenerationService promptGenerationService)
    {
        _aiService = aiService;
        _promptGenerationService = promptGenerationService;

        // Initialize commands
        SendQueryCommand = new RelayCommand(async () => await SendQueryAsync(), () => !IsLoading && !string.IsNullOrEmpty(SelectedModel));
        ClearHistoryCommand = new RelayCommand(ClearHistory);
        LoadModelsCommand = new RelayCommand(async () => await LoadModelsAsync());
        GenerateSuggestionsCommand = new RelayCommand(async () => await GenerateSuggestionsAsync());
        ApplySuggestionCommand = new RelayCommand<AISuggestion>(ApplySuggestion);
        TestCharacterVoiceCommand = new RelayCommand(async () => await TestCharacterVoiceAsync());
        GenerateDialogueCommand = new RelayCommand(async () => await GenerateDialogueAsync());
        AnalyzeRelationshipsCommand = new RelayCommand(async () => await AnalyzeRelationshipsAsync());
        SuggestPlotDevelopmentCommand = new RelayCommand(async () => await SuggestPlotDevelopmentAsync());

        // Initialize suggestions
        InitializeDefaultSuggestions();
    }

    #region Properties

    /// <summary>
    /// Reference to the story view model for context awareness
    /// </summary>
    public NonLinearStoryViewModel? StoryViewModel
    {
        get => _storyViewModel;
        set
        {
            if (_storyViewModel != value)
            {
                _storyViewModel = value;
                OnPropertyChanged();
                UpdateContextualPrompts();
            }
        }
    }

    /// <summary>
    /// User's query input
    /// </summary>
    public string UserQuery
    {
        get => _userQuery;
        set
        {
            if (_userQuery != value)
            {
                _userQuery = value;
                OnPropertyChanged();
                (SendQueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// AI's response
    /// </summary>
    public string AIResponse
    {
        get => _aiResponse;
        set
        {
            if (_aiResponse != value)
            {
                _aiResponse = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Loading state
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                (SendQueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Selected AI model
    /// </summary>
    public string SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (_selectedModel != value)
            {
                _selectedModel = value;
                OnPropertyChanged();
                (SendQueryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Available AI models
    /// </summary>
    public ObservableCollection<string> AvailableModels
    {
        get => _availableModels;
        set
        {
            if (_availableModels != value)
            {
                _availableModels = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// AI suggestions for the current context
    /// </summary>
    public ObservableCollection<AISuggestion> Suggestions
    {
        get => _suggestions;
        set
        {
            if (_suggestions != value)
            {
                _suggestions = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Chat history
    /// </summary>
    public ObservableCollection<ChatMessage> ChatHistory
    {
        get => _chatHistory;
        set
        {
            if (_chatHistory != value)
            {
                _chatHistory = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Current context summary for AI
    /// </summary>
    public string CurrentContextSummary { get; private set; } = string.Empty;

    /// <summary>
    /// AI interaction title based on context
    /// </summary>
    public string InteractionTitle { get; private set; } = "AI Writing Assistant";

    #endregion

    #region Commands

    public ICommand SendQueryCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand LoadModelsCommand { get; }
    public ICommand GenerateSuggestionsCommand { get; }
    public ICommand ApplySuggestionCommand { get; }
    public ICommand TestCharacterVoiceCommand { get; }
    public ICommand GenerateDialogueCommand { get; }
    public ICommand AnalyzeRelationshipsCommand { get; }
    public ICommand SuggestPlotDevelopmentCommand { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Send query to AI
    /// </summary>
    private async Task SendQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(UserQuery) || string.IsNullOrEmpty(SelectedModel)) return;

        IsLoading = true;
        var query = UserQuery;
        UserQuery = string.Empty; // Clear input immediately

        try
        {
            // Add user message to chat history
            var userMessage = new ChatMessage("User", query);
            ChatHistory.Add(userMessage);

            // Build context-aware prompt
            var prompt = BuildContextualPrompt(query);

            // Get AI response
            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, prompt))
            {
                responseBuilder.Append(chunk);
                AIResponse = responseBuilder.ToString();
            }

            // Add AI response to chat history
            var aiMessage = new ChatMessage("Assistant", responseBuilder.ToString());
            ChatHistory.Add(aiMessage);

            // Generate new suggestions based on the conversation
            await GenerateSuggestionsAsync();
        }
        catch (Exception ex)
        {
            AIResponse = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Clear chat history
    /// </summary>
    private void ClearHistory()
    {
        ChatHistory.Clear();
        AIResponse = "Chat history cleared. How can I help you with your story?";
    }

    /// <summary>
    /// Load available AI models
    /// </summary>
    private async Task LoadModelsAsync()
    {
        try
        {
            var models = await _aiService.GetLocalModelsAsync();
            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }

            if (AvailableModels.Any() && string.IsNullOrEmpty(SelectedModel))
            {
                SelectedModel = AvailableModels.First();
            }
        }
        catch (Exception ex)
        {
            AIResponse = $"Error loading models: {ex.Message}";
        }
    }

    /// <summary>
    /// Generate AI suggestions for current context
    /// </summary>
    private async Task GenerateSuggestionsAsync()
    {
        if (StoryViewModel?.PrimaryContext == null) return;

        try
        {
            var context = StoryViewModel.PrimaryContext;
            var suggestions = new List<AISuggestion>();

            // Generate context-specific suggestions
            switch (context.ElementType)
            {
                case ElementType.Character:
                    suggestions.AddRange(await GenerateCharacterSuggestions(context));
                    break;
                case ElementType.Story:
                    suggestions.AddRange(await GenerateStorySuggestions(context));
                    break;
                case ElementType.Chapter:
                    suggestions.AddRange(await GenerateChapterSuggestions(context));
                    break;
                case ElementType.Universe:
                    suggestions.AddRange(await GenerateUniverseSuggestions(context));
                    break;
            }

            // Update suggestions
            Suggestions.Clear();
            foreach (var suggestion in suggestions.Take(5))
            {
                Suggestions.Add(suggestion);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating suggestions: {ex.Message}");
        }
    }

    /// <summary>
    /// Apply AI suggestion
    /// </summary>
    private async void ApplySuggestion(AISuggestion? suggestion)
    {
        if (suggestion == null) return;

        try
        {
            UserQuery = suggestion.Prompt;
            await SendQueryAsync();
        }
        catch (Exception ex)
        {
            AIResponse = $"Error applying suggestion: {ex.Message}";
        }
    }

    /// <summary>
    /// Test character voice
    /// </summary>
    private async Task TestCharacterVoiceAsync()
    {
        if (StoryViewModel?.PrimaryContext?.ElementType != ElementType.Character) return;

        try
        {
            var character = StoryViewModel.PrimaryContext;
            var prompt = $"You are {character.Title}. Respond to this prompt in character: 'Tell me about yourself and your role in the story.'";
            
            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, prompt))
            {
                responseBuilder.Append(chunk);
            }

            AIResponse = $"Character Voice Test for {character.Title}:\n\n{responseBuilder}";
        }
        catch (Exception ex)
        {
            AIResponse = $"Error testing character voice: {ex.Message}";
        }
    }

    /// <summary>
    /// Generate dialogue between characters
    /// </summary>
    private async Task GenerateDialogueAsync()
    {
        if (StoryViewModel?.PrimaryContext == null || StoryViewModel?.SecondaryContext == null) return;

        try
        {
            var character1 = StoryViewModel.PrimaryContext;
            var character2 = StoryViewModel.SecondaryContext;

            if (character1.ElementType != ElementType.Character || character2.ElementType != ElementType.Character)
            {
                AIResponse = "Please select two characters to generate dialogue between them.";
                return;
            }

            var prompt = $"Generate a dialogue between {character1.Title} and {character2.Title}. " +
                        $"Character 1: {character1.Description}\n" +
                        $"Character 2: {character2.Description}\n" +
                        $"Create a natural conversation that reveals their personalities and relationship.";

            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, prompt))
            {
                responseBuilder.Append(chunk);
            }

            AIResponse = $"Dialogue between {character1.Title} and {character2.Title}:\n\n{responseBuilder}";
        }
        catch (Exception ex)
        {
            AIResponse = $"Error generating dialogue: {ex.Message}";
        }
    }

    /// <summary>
    /// Analyze relationships between elements
    /// </summary>
    private async Task AnalyzeRelationshipsAsync()
    {
        if (StoryViewModel?.PrimaryContext == null) return;

        try
        {
            var context = StoryViewModel.PrimaryContext;
            var prompt = $"Analyze the relationships and connections for {context.Title} ({context.ElementType}). " +
                        $"Consider how this element relates to other elements in the story universe. " +
                        $"Provide insights about potential connections, conflicts, and story developments.";

            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, prompt))
            {
                responseBuilder.Append(chunk);
            }

            AIResponse = $"Relationship Analysis for {context.Title}:\n\n{responseBuilder}";
        }
        catch (Exception ex)
        {
            AIResponse = $"Error analyzing relationships: {ex.Message}";
        }
    }

    /// <summary>
    /// Suggest plot development
    /// </summary>
    private async Task SuggestPlotDevelopmentAsync()
    {
        if (StoryViewModel?.PrimaryContext == null) return;

        try
        {
            var context = StoryViewModel.PrimaryContext;
            var prompt = $"Suggest plot developments for {context.Title} ({context.ElementType}). " +
                        $"Consider the current story context and suggest 3-5 potential directions the plot could take. " +
                        $"Include conflicts, character development opportunities, and world-building elements.";

            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _aiService.StreamCompletionAsync(SelectedModel, prompt))
            {
                responseBuilder.Append(chunk);
            }

            AIResponse = $"Plot Development Suggestions for {context.Title}:\n\n{responseBuilder}";
        }
        catch (Exception ex)
        {
            AIResponse = $"Error suggesting plot development: {ex.Message}";
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Build context-aware prompt
    /// </summary>
    private string BuildContextualPrompt(string userQuery)
    {
        var promptBuilder = new StringBuilder();

        // Add system context
        promptBuilder.AppendLine("You are an AI writing assistant helping a creative writer develop their story universe.");
        promptBuilder.AppendLine("You have access to the current story context and can provide creative suggestions, character development, plot ideas, and world-building assistance.");
        promptBuilder.AppendLine();

        // Add current context
        if (!string.IsNullOrEmpty(CurrentContextSummary))
        {
            promptBuilder.AppendLine("Current Story Context:");
            promptBuilder.AppendLine(CurrentContextSummary);
            promptBuilder.AppendLine();
        }

        // Add chat history
        if (ChatHistory.Any())
        {
            promptBuilder.AppendLine("Previous Conversation:");
            foreach (var message in ChatHistory.TakeLast(6)) // Last 6 messages for context
            {
                promptBuilder.AppendLine($"{message.Role}: {message.Content}");
            }
            promptBuilder.AppendLine();
        }

        // Add user query
        promptBuilder.AppendLine($"User: {userQuery}");
        promptBuilder.AppendLine("Assistant:");

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Update contextual prompts based on current story context
    /// </summary>
    private void UpdateContextualPrompts()
    {
        if (StoryViewModel?.PrimaryContext == null)
        {
            CurrentContextSummary = "No specific story context selected.";
            InteractionTitle = "AI Writing Assistant";
            return;
        }

        var context = StoryViewModel.PrimaryContext;
        CurrentContextSummary = $"Working with: {context.Title} ({context.ElementType})\nDescription: {context.Description}";

        // Update interaction title based on context
        InteractionTitle = context.ElementType switch
        {
            ElementType.Character => $"Chat with {context.Title}",
            ElementType.Story => $"Develop '{context.Title}'",
            ElementType.Chapter => $"Edit '{context.Title}'",
            ElementType.Universe => $"Build {context.Title}",
            _ => "AI Writing Assistant"
        };

        OnPropertyChanged(nameof(CurrentContextSummary));
        OnPropertyChanged(nameof(InteractionTitle));

        // Generate new suggestions
        _ = GenerateSuggestionsAsync();
    }

    /// <summary>
    /// Initialize default suggestions
    /// </summary>
    private void InitializeDefaultSuggestions()
    {
        Suggestions.Clear();
        Suggestions.Add(new AISuggestion("Character Development", "How can I develop this character's backstory and personality?"));
        Suggestions.Add(new AISuggestion("Plot Ideas", "What plot developments would work well for this story?"));
        Suggestions.Add(new AISuggestion("World Building", "How can I expand the world-building elements?"));
        Suggestions.Add(new AISuggestion("Dialogue Help", "Help me write natural dialogue for these characters"));
        Suggestions.Add(new AISuggestion("Conflict Ideas", "What conflicts could arise in this situation?"));
    }

    /// <summary>
    /// Generate character-specific suggestions
    /// </summary>
    private async Task<List<AISuggestion>> GenerateCharacterSuggestions(StoryElement character)
    {
        return new List<AISuggestion>
        {
            new("Character Backstory", $"Develop the backstory for {character.Title}"),
            new("Character Motivation", $"What drives {character.Title}? What are their goals?"),
            new("Character Relationships", $"How does {character.Title} relate to other characters?"),
            new("Character Growth", $"How could {character.Title} grow and change throughout the story?"),
            new("Character Voice", $"Help me develop {character.Title}'s unique voice and speech patterns")
        };
    }

    /// <summary>
    /// Generate story-specific suggestions
    /// </summary>
    private async Task<List<AISuggestion>> GenerateStorySuggestions(StoryElement story)
    {
        return new List<AISuggestion>
        {
            new("Plot Development", $"Develop the plot for '{story.Title}'"),
            new("Character Arcs", $"Plan character development arcs for '{story.Title}'"),
            new("World Building", $"Expand the world-building for '{story.Title}'"),
            new("Conflict Ideas", $"What conflicts could drive '{story.Title}' forward?"),
            new("Theme Exploration", $"What themes could be explored in '{story.Title}'?")
        };
    }

    /// <summary>
    /// Generate chapter-specific suggestions
    /// </summary>
    private async Task<List<AISuggestion>> GenerateChapterSuggestions(StoryElement chapter)
    {
        return new List<AISuggestion>
        {
            new("Scene Development", $"Develop the scenes in '{chapter.Title}'"),
            new("Character Interactions", $"Plan character interactions for '{chapter.Title}'"),
            new("Pacing", $"Help with pacing and structure for '{chapter.Title}'"),
            new("Dialogue", $"Write dialogue for '{chapter.Title}'"),
            new("Description", $"Enhance descriptions in '{chapter.Title}'")
        };
    }

    /// <summary>
    /// Generate universe-specific suggestions
    /// </summary>
    private async Task<List<AISuggestion>> GenerateUniverseSuggestions(StoryElement universe)
    {
        return new List<AISuggestion>
        {
            new("World Building", $"Expand the world-building for {universe.Title}"),
            new("History & Lore", $"Develop the history and lore of {universe.Title}"),
            new("Magic System", $"Create or refine the magic system in {universe.Title}"),
            new("Geography", $"Map out the geography and locations in {universe.Title}"),
            new("Cultures", $"Develop the cultures and societies in {universe.Title}")
        };
    }

    #endregion
}

/// <summary>
/// AI suggestion for proactive assistance
/// </summary>
public class AISuggestion
{
    public string Title { get; }
    public string Prompt { get; }
    public string Category { get; }

    public AISuggestion(string title, string prompt, string category = "General")
    {
        Title = title;
        Prompt = prompt;
        Category = category;
    }
}

/// <summary>
/// Chat message for conversation history
/// </summary>
public class ChatMessage
{
    public string Role { get; }
    public string Content { get; }
    public DateTime Timestamp { get; }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
        Timestamp = DateTime.Now;
    }
}

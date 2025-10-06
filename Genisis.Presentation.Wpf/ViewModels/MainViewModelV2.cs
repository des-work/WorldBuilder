using CommunityToolkit.Mvvm.Input;
using Genisis.Application.Universe.Commands;
using Genisis.Application.Universe.Queries;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.ViewModels.Base;
using MediatR;
using System.Collections.ObjectModel;

namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// Main ViewModel for the application using the new architecture
/// </summary>
public class MainViewModelV2 : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private Universe? _selectedUniverse;

    public MainViewModelV2(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Title = "WorldBuilder AI";

        // Initialize commands
        CreateUniverseCommand = new AsyncRelayCommand(CreateUniverseAsync);
        UpdateUniverseCommand = new AsyncRelayCommand(UpdateUniverseAsync, () => SelectedUniverse != null);
        DeleteUniverseCommand = new AsyncRelayCommand(DeleteUniverseAsync, () => SelectedUniverse != null);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    /// <summary>
    /// Collection of universes
    /// </summary>
    public ObservableCollection<Universe> Universes { get; } = new();

    /// <summary>
    /// Currently selected universe
    /// </summary>
    public Universe? SelectedUniverse
    {
        get => _selectedUniverse;
        set
        {
            if (SetProperty(ref _selectedUniverse, value))
            {
                UpdateUniverseCommand.NotifyCanExecuteChanged();
                DeleteUniverseCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Command to create a new universe
    /// </summary>
    public IAsyncRelayCommand CreateUniverseCommand { get; }

    /// <summary>
    /// Command to update the selected universe
    /// </summary>
    public IAsyncRelayCommand UpdateUniverseCommand { get; }

    /// <summary>
    /// Command to delete the selected universe
    /// </summary>
    public IAsyncRelayCommand DeleteUniverseCommand { get; }

    /// <summary>
    /// Command to refresh the universes list
    /// </summary>
    public IAsyncRelayCommand RefreshCommand { get; }

    /// <summary>
    /// Loads initial data
    /// </summary>
    protected override async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _mediator.Send(new GetAllUniversesQuery());
            if (result.IsSuccess)
            {
                Universes.Clear();
                foreach (var universe in result.Value!)
                {
                    Universes.Add(universe);
                }
            }
            else
            {
                _dialogService.ShowMessage($"Failed to load universes: {result.Error}", "Error");
            }
        }, "Load Universes");
    }

    /// <summary>
    /// Creates a new universe
    /// </summary>
    private async Task CreateUniverseAsync()
    {
        if (_dialogService.ShowInputDialog("Enter universe name:", "New Universe", out var name) &&
            !string.IsNullOrWhiteSpace(name))
        {
            await ExecuteAsync(async () =>
            {
                var description = string.Empty;
                if (_dialogService.ShowInputDialog("Enter universe description (optional):", "Description", out var desc))
                {
                    description = desc ?? string.Empty;
                }

                var result = await _mediator.Send(new CreateUniverseCommand(
                    new EntityName(name),
                    string.IsNullOrWhiteSpace(description) ? null : new EntityDescription(description)));

                if (result.IsSuccess)
                {
                    Universes.Add(result.Value!);
                    SelectedUniverse = result.Value;
                    _dialogService.ShowMessage("Universe created successfully.", "Success");
                }
                else
                {
                    _dialogService.ShowMessage($"Failed to create universe: {result.Error}", "Error");
                }
            }, "Create Universe");
        }
    }

    /// <summary>
    /// Updates the selected universe
    /// </summary>
    private async Task UpdateUniverseAsync()
    {
        if (SelectedUniverse == null) return;

        if (_dialogService.ShowInputDialog("Enter new universe name:", "Update Universe", out var name) &&
            !string.IsNullOrWhiteSpace(name))
        {
            await ExecuteAsync(async () =>
            {
                var description = SelectedUniverse.Description?.Value ?? string.Empty;
                if (_dialogService.ShowInputDialog("Enter new universe description (optional):", "Description", out var desc))
                {
                    description = desc ?? string.Empty;
                }

                var result = await _mediator.Send(new UpdateUniverseCommand(
                    new UniverseId(SelectedUniverse.Id),
                    new EntityName(name),
                    string.IsNullOrWhiteSpace(description) ? null : new EntityDescription(description)));

                if (result.IsSuccess)
                {
                    // Update the universe in the collection
                    var index = Universes.IndexOf(SelectedUniverse);
                    if (index >= 0)
                    {
                        Universes[index] = result.Value!;
                        SelectedUniverse = result.Value;
                    }
                    _dialogService.ShowMessage("Universe updated successfully.", "Success");
                }
                else
                {
                    _dialogService.ShowMessage($"Failed to update universe: {result.Error}", "Error");
                }
            }, "Update Universe");
        }
    }

    /// <summary>
    /// Deletes the selected universe
    /// </summary>
    private async Task DeleteUniverseAsync()
    {
        if (SelectedUniverse == null) return;

        if (_dialogService.ShowConfirmation(
            $"Are you sure you want to delete the universe '{SelectedUniverse.Name.Value}'?",
            "Delete Universe"))
        {
            await ExecuteAsync(async () =>
            {
                var result = await _mediator.Send(new DeleteUniverseCommand(new UniverseId(SelectedUniverse.Id)));

                if (result.IsSuccess)
                {
                    Universes.Remove(SelectedUniverse);
                    SelectedUniverse = null;
                    _dialogService.ShowMessage("Universe deleted successfully.", "Success");
                }
                else
                {
                    _dialogService.ShowMessage($"Failed to delete universe: {result.Error}", "Error");
                }
            }, "Delete Universe");
        }
    }
}

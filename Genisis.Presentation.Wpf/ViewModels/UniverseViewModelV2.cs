using CommunityToolkit.Mvvm.ComponentModel;
using Genisis.Application.Universe.Commands;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.ViewModels.Base;
using MediatR;

namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// ViewModel for editing a universe using the new architecture
/// </summary>
public class UniverseViewModelV2 : EditorViewModelBase<Universe>
{
    private readonly IMediator _mediator;
    private string _name = string.Empty;
    private string _description = string.Empty;

    public UniverseViewModelV2(Universe universe, IMediator mediator, IDialogService dialogService)
        : base(universe, dialogService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        LoadFromModel();
    }

    /// <summary>
    /// Universe name
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                Validate();
            }
        }
    }

    /// <summary>
    /// Universe description
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                Validate();
            }
        }
    }

    /// <summary>
    /// Loads data from the model
    /// </summary>
    protected override void LoadFromModel()
    {
        Name = Model.Name.Value;
        Description = Model.Description?.Value ?? string.Empty;
        IsDirty = false;
        Validate();
    }

    /// <summary>
    /// Saves the universe
    /// </summary>
    protected override async Task OnSaveAsync()
    {
        var result = await _mediator.Send(new UpdateUniverseCommand(
            new UniverseId(Model.Id),
            new EntityName(Name),
            string.IsNullOrWhiteSpace(Description) ? null : new EntityDescription(Description)));

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error);
        }

        // Update the model with the saved data
        Model.Update(new EntityName(Name), string.IsNullOrWhiteSpace(Description) ? null : new EntityDescription(Description));
    }

    /// <summary>
    /// Validates the universe data
    /// </summary>
    protected override void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }
        else if (Name.Length > 200)
        {
            errors.Add("Name cannot exceed 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 2000)
        {
            errors.Add("Description cannot exceed 2000 characters.");
        }

        IsValid = !errors.Any();
        ValidationMessage = errors.Any() ? string.Join(" ", errors) : string.Empty;
    }
}

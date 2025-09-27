using CommunityToolkit.Mvvm.Messaging;
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Genisis.Presentation.Wpf.ViewModels;

public class UniverseViewModel : EditorViewModelBase<Universe>, INotifyDataErrorInfo
{
    private readonly IUniverseRepository _universeRepository;
    private readonly Dictionary<string, List<string>> _errors = new();

    public Universe Universe => Model;

    public UniverseViewModel(Universe universe, IUniverseRepository universeRepository) : base(universe)
    {
        _universeRepository = universeRepository;
        Universe.PropertyChanged += OnUniversePropertyChanged;
        ValidateAllProperties();
    }

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(errors => errors);

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    private void OnUniversePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ValidateProperty(e.PropertyName);
    }

    private void ValidateAllProperties()
    {
        ValidateProperty(nameof(Universe.Name));
        ValidateProperty(nameof(Universe.Description));
    }

    private void ValidateProperty(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return;

        var errors = new List<string>();

        switch (propertyName)
        {
            case nameof(Universe.Name):
                if (string.IsNullOrWhiteSpace(Universe.Name))
                    errors.Add("Name is required.");
                else if (Universe.Name.Length > 200)
                    errors.Add("Name must be 200 characters or less.");
                break;

            case nameof(Universe.Description):
                if (Universe.Description != null && Universe.Description.Length > 2000)
                    errors.Add("Description must be 2000 characters or less.");
                break;
        }

        if (errors.Any())
        {
            if (_errors.ContainsKey(propertyName))
                _errors[propertyName] = errors;
            else
                _errors.Add(propertyName, errors);
        }
        else
        {
            _errors.Remove(propertyName);
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    protected override async Task OnSaveAsync()
    {
        ValidateAllProperties();

        if (HasErrors)
            throw new InvalidOperationException("Cannot save with validation errors.");

        await _universeRepository.UpdateAsync(Universe);
    }
}

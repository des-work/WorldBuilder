﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class StoryViewModel : ViewModelBase
{
    private readonly IStoryRepository _storyRepository;
    public Story Story { get; }

    public ICommand SaveCommand { get; }

    public StoryViewModel(Story story, IStoryRepository storyRepository)
    {
        Story = story;
        _storyRepository = storyRepository;
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    private async Task SaveAsync()
    {
        await _storyRepository.UpdateAsync(Story);
    }
}
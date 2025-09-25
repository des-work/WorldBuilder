﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;

namespace Genisis.App.ViewModels;

public class StoryViewModel : EditorViewModelBase<Story>
{
    private readonly IStoryRepository _storyRepository;
    public Story Story => Model;

    public StoryViewModel(Story story, IStoryRepository storyRepository) : base(story)
    {
        _storyRepository = storyRepository;
    }

    protected override async Task OnSaveAsync()
    {
        await _storyRepository.UpdateAsync(Story);
    }
}
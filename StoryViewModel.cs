using Genisis.Core.Models;

namespace Genisis.App.ViewModels;

public class StoryViewModel : ViewModelBase
{
    public Story Story { get; }

    public StoryViewModel(Story story)
    {
        Story = story;
    }
}
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class ChapterViewModel : ViewModelBase
{
    private readonly IChapterRepository _chapterRepository;
    public Chapter Chapter { get; }

    public ICommand SaveCommand { get; }

    public ChapterViewModel(Chapter chapter, IChapterRepository chapterRepository)
    {
        Chapter = chapter;
        _chapterRepository = chapterRepository;
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    private async Task SaveAsync()
    {
        await _chapterRepository.UpdateAsync(Chapter);
        // We can add a "Saved!" notification here later.
    }
}
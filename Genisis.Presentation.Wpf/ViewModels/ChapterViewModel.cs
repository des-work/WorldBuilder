using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;

namespace Genisis.Presentation.Wpf.ViewModels;

public class ChapterViewModel : EditorViewModelBase<Chapter>
{
    private readonly IChapterRepository _chapterRepository;
    public Chapter Chapter => Model;

    public ChapterViewModel(Chapter chapter, IChapterRepository chapterRepository) : base(chapter)
    {
        _chapterRepository = chapterRepository;
    }

    protected override async Task OnSaveAsync()
    {
        await _chapterRepository.UpdateAsync(Chapter);
    }
}

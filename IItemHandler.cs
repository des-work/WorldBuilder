using Genisis.App.ViewModels;
using System.Threading.Tasks;

namespace Genisis.App.Services;

public interface IItemHandler
{
    Task AddChildAsync(object parentItem);
    Task DeleteAsync(object item, MainViewModel mainViewModel);
}

public interface IItemHandlerFactory
{
    IItemHandler? GetHandler(object? item);
}
using System.Threading.Tasks;
using Genisis.Core.Models;

namespace Genisis.Application.Handlers;

public interface IItemHandler { }

public interface IItemHandlerFactory
{
    IItemHandler? GetHandler(object? item);
}


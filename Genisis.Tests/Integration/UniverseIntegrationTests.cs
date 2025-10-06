using Genisis.Application.Universe.Commands;
using Genisis.Application.Universe.Queries;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Genisis.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Genisis.Tests.Integration;

public class UniverseIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly GenesisDbContext _dbContext;

    public UniverseIntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure in-memory database for testing
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Register repositories
        services.AddScoped<IUniverseRepository, UniverseRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();

        // Register domain services
        services.AddScoped<IUniverseDomainService, UniverseDomainService>();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUniverseCommand).Assembly));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<GenesisDbContext>();

        // Create the database and apply migrations
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateUniverse_ShouldCreateSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe for integration testing");

        // Act
        var result = await mediator.Send(new CreateUniverseCommand(name, description));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(description, result.Value.Description);
        Assert.NotEqual(0, result.Value.Id);
    }

    [Fact]
    public async Task CreateUniverse_WithDuplicateName_ShouldFail()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var name = new EntityName("Duplicate Universe");
        var description = new EntityDescription("First universe");

        // Create first universe
        var firstResult = await mediator.Send(new CreateUniverseCommand(name, description));
        Assert.True(firstResult.IsSuccess);

        // Act - Try to create second universe with same name
        var secondResult = await mediator.Send(new CreateUniverseCommand(name, description));

        // Assert
        Assert.False(secondResult.IsSuccess);
        Assert.Contains("already exists", secondResult.Error);
    }

    [Fact]
    public async Task UpdateUniverse_ShouldUpdateSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var originalName = new EntityName("Original Name");
        var originalDescription = new EntityDescription("Original Description");
        var newName = new EntityName("Updated Name");
        var newDescription = new EntityDescription("Updated Description");

        // Create universe
        var createResult = await mediator.Send(new CreateUniverseCommand(originalName, originalDescription));
        Assert.True(createResult.IsSuccess);
        var universe = createResult.Value!;

        // Act
        var updateResult = await mediator.Send(new UpdateUniverseCommand(
            new UniverseId(universe.Id), newName, newDescription));

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newName, updateResult.Value!.Name);
        Assert.Equal(newDescription, updateResult.Value.Description);
    }

    [Fact]
    public async Task DeleteUniverse_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var name = new EntityName("To Be Deleted");
        var description = new EntityDescription("This universe will be deleted");

        // Create universe
        var createResult = await mediator.Send(new CreateUniverseCommand(name, description));
        Assert.True(createResult.IsSuccess);
        var universe = createResult.Value!;

        // Act
        var deleteResult = await mediator.Send(new DeleteUniverseCommand(new UniverseId(universe.Id)));

        // Assert
        Assert.True(deleteResult.IsSuccess);

        // Verify universe is deleted
        var getResult = await mediator.Send(new GetUniverseQuery(new UniverseId(universe.Id)));
        Assert.False(getResult.IsSuccess);
        Assert.Contains("not found", getResult.Error);
    }

    [Fact]
    public async Task GetAllUniverses_ShouldReturnAllUniverses()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var universe1 = new EntityName("Universe 1");
        var universe2 = new EntityName("Universe 2");
        var universe3 = new EntityName("Universe 3");

        // Create multiple universes
        await mediator.Send(new CreateUniverseCommand(universe1, null));
        await mediator.Send(new CreateUniverseCommand(universe2, null));
        await mediator.Send(new CreateUniverseCommand(universe3, null));

        // Act
        var result = await mediator.Send(new GetAllUniversesQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Contains(result.Value, u => u.Name == universe1);
        Assert.Contains(result.Value, u => u.Name == universe2);
        Assert.Contains(result.Value, u => u.Name == universe3);
    }

    [Fact]
    public async Task SearchUniverses_ShouldReturnMatchingUniverses()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var fantasyUniverse = new EntityName("Fantasy World");
        var fantasyDescription = new EntityDescription("A magical fantasy universe");
        var sciFiUniverse = new EntityName("Sci-Fi Galaxy");
        var sciFiDescription = new EntityDescription("A futuristic sci-fi universe");

        // Create universes
        await mediator.Send(new CreateUniverseCommand(fantasyUniverse, fantasyDescription));
        await mediator.Send(new CreateUniverseCommand(sciFiUniverse, sciFiDescription));

        // Act
        var result = await mediator.Send(new SearchUniversesQuery("fantasy"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(fantasyUniverse, result.Value.First().Name);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}

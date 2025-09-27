using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Genisis.Tests;

public class UniverseRepositoryTests : IDisposable
{
    private readonly GenesisDbContext _dbContext;
    private readonly IUniverseRepository _repository;

    public UniverseRepositoryTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        services.AddLogging(builder => builder.AddConsole());

        var serviceProvider = services.BuildServiceProvider();
        _dbContext = serviceProvider.GetRequiredService<GenesisDbContext>();

        // Create the database and apply migrations
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        _repository = new UniverseRepository(_dbContext, serviceProvider.GetRequiredService<ILogger<UniverseRepository>>());
    }

    [Fact]
    public async Task AddAsync_ShouldAddUniverseToDatabase()
    {
        // Arrange
        var universe = new Universe { Name = "Test Universe", Description = "A test universe" };

        // Act
        var result = await _repository.AddAsync(universe);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Universe", result.Name);
        Assert.Equal("A test universe", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectUniverse()
    {
        // Arrange
        var universe = new Universe { Name = "Test Universe 2", Description = "Another test universe" };
        var addedUniverse = await _repository.AddAsync(universe);

        // Act
        var result = await _repository.GetByIdAsync(addedUniverse.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedUniverse.Id, result.Id);
        Assert.Equal("Test Universe 2", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUniverses()
    {
        // Arrange
        await _repository.AddAsync(new Universe { Name = "Universe 1", Description = "First universe" });
        await _repository.AddAsync(new Universe { Name = "Universe 2", Description = "Second universe" });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUniverse()
    {
        // Arrange
        var universe = new Universe { Name = "Original Name", Description = "Original description" };
        var addedUniverse = await _repository.AddAsync(universe);

        // Act
        addedUniverse.Name = "Updated Name";
        addedUniverse.Description = "Updated description";
        await _repository.UpdateAsync(addedUniverse);

        // Assert
        var updatedUniverse = await _repository.GetByIdAsync(addedUniverse.Id);
        Assert.NotNull(updatedUniverse);
        Assert.Equal("Updated Name", updatedUniverse.Name);
        Assert.Equal("Updated description", updatedUniverse.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUniverse()
    {
        // Arrange
        var universe = new Universe { Name = "To Be Deleted", Description = "Will be deleted" };
        var addedUniverse = await _repository.AddAsync(universe);

        // Act
        await _repository.DeleteAsync(addedUniverse);

        // Assert
        var result = await _repository.GetByIdAsync(addedUniverse.Id);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }
}

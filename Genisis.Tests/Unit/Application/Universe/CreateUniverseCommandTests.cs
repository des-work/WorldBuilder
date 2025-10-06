using Genisis.Application.Universe.Commands;
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.Services;
using Genisis.Core.ValueObjects;
using Moq;
using Xunit;

namespace Genisis.Tests.Unit.Application.Universe;

public class CreateUniverseCommandTests
{
    private readonly Mock<IUniverseRepository> _mockRepository;
    private readonly Mock<IUniverseDomainService> _mockDomainService;
    private readonly CreateUniverseCommandHandler _handler;

    public CreateUniverseCommandTests()
    {
        _mockRepository = new Mock<IUniverseRepository>();
        _mockDomainService = new Mock<IUniverseDomainService>();
        _handler = new CreateUniverseCommandHandler(_mockRepository.Object, _mockDomainService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUniverse()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");
        var command = new CreateUniverseCommand(name, description);

        var validationResult = ValidationResult.Success();
        var createdUniverse = new Universe(name, description);

        _mockDomainService.Setup(x => x.ValidateCreationAsync(name, description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _mockDomainService.Setup(x => x.IsNameUniqueAsync(name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Universe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUniverse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createdUniverse, result.Value);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Universe>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidValidation_ShouldReturnFailure()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");
        var command = new CreateUniverseCommand(name, description);

        var validationResult = ValidationResult.Failure("Name is required");

        _mockDomainService.Setup(x => x.ValidateCreationAsync(name, description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name is required", result.Errors);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Universe>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonUniqueName_ShouldReturnFailure()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");
        var command = new CreateUniverseCommand(name, description);

        var validationResult = ValidationResult.Success();

        _mockDomainService.Setup(x => x.ValidateCreationAsync(name, description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _mockDomainService.Setup(x => x.IsNameUniqueAsync(name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Universe>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");
        var command = new CreateUniverseCommand(name, description);

        var validationResult = ValidationResult.Success();

        _mockDomainService.Setup(x => x.ValidateCreationAsync(name, description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _mockDomainService.Setup(x => x.IsNameUniqueAsync(name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Universe>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to create universe", result.Error);
    }
}

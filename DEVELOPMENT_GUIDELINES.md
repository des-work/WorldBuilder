# WorldBuilder AI - Development Guidelines

## Overview

This document provides comprehensive guidelines for developing WorldBuilder AI using the new architecture. It covers coding standards, best practices, testing strategies, and development workflows.

## Coding Standards

### 1. Naming Conventions

#### Classes and Interfaces
- **Classes**: PascalCase (e.g., `UniverseRepository`, `CreateUniverseCommand`)
- **Interfaces**: PascalCase with "I" prefix (e.g., `IUniverseRepository`, `ICacheService`)
- **Abstract Classes**: PascalCase (e.g., `BaseRepository`, `ViewModelBase`)
- **Enums**: PascalCase (e.g., `CharacterTier`, `ValidationResult`)

#### Methods and Properties
- **Public Methods**: PascalCase (e.g., `GetByIdAsync`, `CreateUniverse`)
- **Private Methods**: PascalCase (e.g., `ValidateInput`, `HandleError`)
- **Properties**: PascalCase (e.g., `Name`, `Description`, `IsValid`)
- **Fields**: camelCase with underscore prefix (e.g., `_logger`, `_cacheService`)

#### Variables and Parameters
- **Local Variables**: camelCase (e.g., `universeName`, `validationResult`)
- **Parameters**: camelCase (e.g., `universeId`, `cancellationToken`)
- **Constants**: PascalCase (e.g., `MaxNameLength`, `DefaultCacheExpiration`)

#### Files and Directories
- **Files**: PascalCase (e.g., `UniverseRepository.cs`, `CreateUniverseCommand.cs`)
- **Directories**: PascalCase (e.g., `Commands`, `Queries`, `ValueObjects`)

### 2. Code Organization

#### File Structure
```csharp
// 1. Using statements (alphabetical order)
using System;
using System.Threading;
using System.Threading.Tasks;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

// 2. Namespace
namespace Genisis.Application.Universe.Commands;

// 3. Class documentation
/// <summary>
/// Command to create a new universe
/// </summary>
public record CreateUniverseCommand(EntityName Name, EntityDescription? Description) : IRequest<Result<Universe>>;

// 4. Class implementation
public class CreateUniverseCommandHandler : IRequestHandler<CreateUniverseCommand, Result<Universe>>
{
    // 5. Fields
    private readonly IUniverseRepository _universeRepository;
    private readonly ILogger<CreateUniverseCommandHandler> _logger;

    // 6. Constructor
    public CreateUniverseCommandHandler(IUniverseRepository universeRepository, ILogger<CreateUniverseCommandHandler> logger)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 7. Public methods
    public async Task<Result<Universe>> Handle(CreateUniverseCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }

    // 8. Private methods
    private async Task<ValidationResult> ValidateRequestAsync(CreateUniverseCommand request)
    {
        // Implementation
    }
}
```

#### Method Organization
1. **Public methods** first
2. **Private methods** after public methods
3. **Methods grouped by functionality**
4. **Related methods** together

### 3. Documentation Standards

#### XML Documentation
```csharp
/// <summary>
/// Creates a new universe with the specified name and description
/// </summary>
/// <param name="name">The name of the universe</param>
/// <param name="description">The description of the universe (optional)</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>A result containing the created universe or error information</returns>
/// <exception cref="ArgumentNullException">Thrown when name is null</exception>
/// <exception cref="DomainValidationException">Thrown when validation fails</exception>
public async Task<Result<Universe>> CreateUniverseAsync(EntityName name, EntityDescription? description, CancellationToken cancellationToken = default)
```

#### Code Comments
```csharp
// Good: Explains why, not what
if (universe.Stories.Count > 0)
{
    // Cannot delete universe with existing stories
    return Result.Failure("Universe contains stories and cannot be deleted");
}

// Bad: Explains what (obvious from code)
// Increment the counter
counter++;
```

### 4. Error Handling

#### Exception Handling
```csharp
// Good: Specific exception handling
try
{
    var result = await _repository.GetByIdAsync(id, cancellationToken);
    return result;
}
catch (EntityNotFoundException ex)
{
    _logger.LogWarning("Entity not found: {EntityId}", id);
    return Result<Universe>.Failure("Universe not found");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error getting universe: {EntityId}", id);
    return Result<Universe>.Failure("An unexpected error occurred");
}

// Bad: Generic exception handling
try
{
    var result = await _repository.GetByIdAsync(id, cancellationToken);
    return result;
}
catch (Exception ex)
{
    return Result<Universe>.Failure("Error occurred");
}
```

#### Validation
```csharp
// Good: Early validation with clear messages
if (string.IsNullOrWhiteSpace(name))
{
    return Result<Universe>.Failure("Name is required");
}

if (name.Length > 200)
{
    return Result<Universe>.Failure("Name cannot exceed 200 characters");
}

// Bad: Late validation
var universe = new Universe(name, description);
if (!universe.IsValid)
{
    return Result<Universe>.Failure("Invalid universe");
}
```

## Domain Layer Guidelines

### 1. Entities

#### Entity Design
```csharp
public class Universe
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // Private constructor for EF Core
    protected Universe() { }

    // Public constructor for domain creation
    public Universe(EntityName name, EntityDescription? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UniverseCreatedEvent(new UniverseId(Id), Name, Description));
    }

    // Properties with private setters
    public int Id { get; private set; }
    public EntityName Name { get; private set; } = null!;
    public EntityDescription? Description { get; private set; }

    // Domain methods
    public void Update(EntityName name, EntityDescription? description = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UniverseUpdatedEvent(new UniverseId(Id), Name, Description));
    }

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

#### Entity Rules
- **Immutable properties** with private setters
- **Domain methods** for business operations
- **Domain events** for state changes
- **Validation** in constructors and methods
- **No UI concerns** in entities

### 2. Value Objects

#### Value Object Design
```csharp
public class EntityName : TextValue
{
    public EntityName(string value) : base(value, 200)
    {
    }

    public static implicit operator string(EntityName name) => name.Value;
    public static implicit operator EntityName(string value) => new(value);
}
```

#### Value Object Rules
- **Immutable** after creation
- **Validation** in constructor
- **Equality** based on value
- **Implicit conversions** where appropriate
- **No identity** or lifecycle

### 3. Domain Services

#### Domain Service Design
```csharp
public interface IUniverseDomainService
{
    Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateCreationAsync(EntityName name, EntityDescription? description, CancellationToken cancellationToken = default);
}

public class UniverseDomainService : IUniverseDomainService
{
    private readonly IUniverseRepository _universeRepository;

    public UniverseDomainService(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _universeRepository.IsNameUniqueAsync(name, excludeId, cancellationToken);
    }
}
```

#### Domain Service Rules
- **Stateless** operations
- **Complex business logic** that doesn't belong to entities
- **Repository dependencies** for data access
- **Async operations** where appropriate

## Application Layer Guidelines

### 1. Commands

#### Command Design
```csharp
// Command record
public record CreateUniverseCommand(EntityName Name, EntityDescription? Description) : IRequest<Result<Universe>>;

// Command handler
public class CreateUniverseCommandHandler : IRequestHandler<CreateUniverseCommand, Result<Universe>>
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseDomainService _universeDomainService;

    public CreateUniverseCommandHandler(IUniverseRepository universeRepository, IUniverseDomainService universeDomainService)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        _universeDomainService = universeDomainService ?? throw new ArgumentNullException(nameof(universeDomainService));
    }

    public async Task<Result<Universe>> Handle(CreateUniverseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validationResult = await _universeDomainService.ValidateCreationAsync(request.Name, request.Description, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<Universe>.Failure(validationResult.Errors);
            }

            // Check business rules
            var isNameUnique = await _universeDomainService.IsNameUniqueAsync(request.Name, null, cancellationToken);
            if (!isNameUnique)
            {
                return Result<Universe>.Failure($"A universe with the name '{request.Name.Value}' already exists.");
            }

            // Create the entity
            var universe = new Universe(request.Name, request.Description);
            var createdUniverse = await _universeRepository.AddAsync(universe, cancellationToken);

            return Result<Universe>.Success(createdUniverse);
        }
        catch (Exception ex)
        {
            return Result<Universe>.Failure($"Failed to create universe: {ex.Message}");
        }
    }
}
```

#### Command Rules
- **Immutable** command records
- **Single responsibility** per command
- **Validation** before business logic
- **Error handling** with try-catch
- **Result pattern** for responses

### 2. Queries

#### Query Design
```csharp
// Query record
public record GetUniverseQuery(UniverseId Id) : IRequest<Result<Universe>>;

// Query handler
public class GetUniverseQueryHandler : IRequestHandler<GetUniverseQuery, Result<Universe>>
{
    private readonly IUniverseRepository _universeRepository;

    public GetUniverseQueryHandler(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<Result<Universe>> Handle(GetUniverseQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var universe = await _universeRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (universe == null)
            {
                return Result<Universe>.Failure($"Universe with ID {request.Id.Value} not found.");
            }

            return Result<Universe>.Success(universe);
        }
        catch (Exception ex)
        {
            return Result<Universe>.Failure($"Failed to get universe: {ex.Message}");
        }
    }
}
```

#### Query Rules
- **Immutable** query records
- **Single responsibility** per query
- **Null checks** for not found cases
- **Error handling** with try-catch
- **Result pattern** for responses

### 3. Result Pattern

#### Result Design
```csharp
// Success result
var result = Result<Universe>.Success(universe);

// Failure result with single error
var result = Result<Universe>.Failure("Name is required");

// Failure result with multiple errors
var result = Result<Universe>.Failure("Name is required", "Description is too long");

// Check result
if (result.IsSuccess)
{
    var universe = result.Value;
    // Use universe
}
else
{
    var errors = result.Errors;
    // Handle errors
}
```

#### Result Rules
- **Consistent** success/failure pattern
- **Type-safe** error handling
- **Multiple errors** support
- **Implicit conversions** where appropriate

## Infrastructure Layer Guidelines

### 1. Repositories

#### Repository Design
```csharp
public class UniverseRepository : BaseRepository<Universe>, IUniverseRepository
{
    public UniverseRepository(GenesisDbContext context, ILogger<UniverseRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Universe?> GetByIdWithContentAsync(UniverseId id, CancellationToken cancellationToken = default)
    {
        var specification = new UniverseWithContentSpecification(id);
        return await GetFirstBySpecificationAsync(specification, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (excludeId.HasValue)
        {
            var specification = new UniverseByNameExcludingIdSpecification(name, excludeId.Value);
            return !await AnyBySpecificationAsync(specification, cancellationToken);
        }
        else
        {
            var specification = new UniverseByNameSpecification(name);
            return !await AnyBySpecificationAsync(specification, cancellationToken);
        }
    }
}
```

#### Repository Rules
- **Inherit** from BaseRepository
- **Use specifications** for complex queries
- **Async operations** for all methods
- **Error handling** with try-catch
- **Logging** for debugging

### 2. Specifications

#### Specification Design
```csharp
public class UniverseByNameSpecification : Specification<Universe>
{
    public UniverseByNameSpecification(EntityName name)
    {
        Criteria = u => u.Name.Value == name.Value;
        IsTrackingEnabled = false;
    }
}

public class UniverseWithContentSpecification : Specification<Universe>
{
    public UniverseWithContentSpecification(UniverseId universeId)
    {
        Criteria = u => u.Id == universeId.Value;
        AddInclude(u => u.Stories);
        AddInclude(u => u.Characters);
        IsTrackingEnabled = false;
    }
}
```

#### Specification Rules
- **Composable** query logic
- **Reusable** across repositories
- **Type-safe** query building
- **Performance** considerations

### 3. Caching

#### Cache Design
```csharp
public class CachedUniverseRepository : IUniverseRepository
{
    private readonly IUniverseRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public async Task<Universe?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"universe:{id}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdAsync(id, cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    public async Task<Universe> AddAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        var result = await _repository.AddAsync(universe, cancellationToken);
        
        // Invalidate related caches
        await InvalidateUniverseCachesAsync();
        
        return result;
    }
}
```

#### Cache Rules
- **Cache reads** for performance
- **Invalidate** on writes
- **Expiration** strategies
- **Error handling** for cache failures

## Presentation Layer Guidelines

### 1. ViewModels

#### ViewModel Design
```csharp
public class UniverseViewModelV2 : EditorViewModelBase<Universe>
{
    private readonly IMediator _mediator;
    private string _name = string.Empty;
    private string _description = string.Empty;

    public UniverseViewModelV2(Universe universe, IMediator mediator, IDialogService dialogService)
        : base(universe, dialogService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        LoadFromModel();
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                Validate();
            }
        }
    }

    protected override void LoadFromModel()
    {
        Name = Model.Name.Value;
        Description = Model.Description?.Value ?? string.Empty;
        IsDirty = false;
        Validate();
    }

    protected override async Task OnSaveAsync()
    {
        var result = await _mediator.Send(new UpdateUniverseCommand(
            new UniverseId(Model.Id),
            new EntityName(Name),
            string.IsNullOrWhiteSpace(Description) ? null : new EntityDescription(Description)));

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error);
        }

        Model.Update(new EntityName(Name), string.IsNullOrWhiteSpace(Description) ? null : new EntityDescription(Description));
    }
}
```

#### ViewModel Rules
- **Inherit** from base ViewModel
- **Data binding** properties
- **Validation** on property changes
- **Async operations** for data access
- **Error handling** with user feedback

### 2. Services

#### Service Design
```csharp
public interface IDialogService
{
    bool ShowInputDialog(string prompt, string title, out string? result);
    bool ShowConfirmation(string message, string title);
    void ShowMessage(string message, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information);
}

public class DialogService : IDialogService
{
    public bool ShowInputDialog(string prompt, string title, out string? result)
    {
        var dialog = new InputDialog(prompt, title);
        var dialogResult = dialog.ShowDialog();
        
        if (dialogResult == true)
        {
            result = dialog.InputText;
            return true;
        }
        
        result = null;
        return false;
    }
}
```

#### Service Rules
- **Interface** for testability
- **Single responsibility** per service
- **Async operations** where appropriate
- **Error handling** with user feedback

## Testing Guidelines

### 1. Unit Tests

#### Test Structure
```csharp
public class UniverseTests
{
    [Fact]
    public void Universe_WithValidName_ShouldCreateSuccessfully()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");

        // Act
        var universe = new Universe(name, description);

        // Assert
        Assert.Equal(name, universe.Name);
        Assert.Equal(description, universe.Description);
        Assert.NotNull(universe.DomainEvents);
        Assert.Single(universe.DomainEvents);
        Assert.IsType<UniverseCreatedEvent>(universe.DomainEvents.First());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Universe_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Universe(new EntityName(invalidName), null));
    }
}
```

#### Unit Test Rules
- **Arrange-Act-Assert** pattern
- **Descriptive** test names
- **Single** assertion per test
- **Edge cases** coverage
- **Mock** external dependencies

### 2. Integration Tests

#### Test Structure
```csharp
public class UniverseIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly GenesisDbContext _dbContext;

    public UniverseIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUniverseCommand).Assembly));
        
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<GenesisDbContext>();
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateUniverse_ShouldCreateSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var name = new EntityName("Test Universe");

        // Act
        var result = await mediator.Send(new CreateUniverseCommand(name, null));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
    }
}
```

#### Integration Test Rules
- **Real dependencies** where appropriate
- **In-memory** database for testing
- **Service provider** for dependency injection
- **Cleanup** in Dispose method

### 3. Test Coverage

#### Coverage Targets
- **Domain Layer**: 95%+ coverage
- **Application Layer**: 90%+ coverage
- **Infrastructure Layer**: 85%+ coverage
- **Presentation Layer**: 80%+ coverage

#### Coverage Tools
- **Coverlet** for coverage collection
- **ReportGenerator** for coverage reports
- **CI/CD** integration for coverage tracking

## Performance Guidelines

### 1. Database Performance

#### Query Optimization
```csharp
// Good: Use specifications for complex queries
var specification = new UniverseWithContentSpecification(universeId);
var universe = await GetFirstBySpecificationAsync(specification, cancellationToken);

// Bad: N+1 queries
var universe = await _context.Universes.FindAsync(universeId);
foreach (var story in universe.Stories)
{
    story.Chapters = await _context.Chapters.Where(c => c.StoryId == story.Id).ToListAsync();
}
```

#### Caching Strategy
```csharp
// Cache frequently accessed data
var cacheKey = $"universe:{id}";
var universe = await _cacheService.GetOrSetAsync(
    cacheKey,
    async () => await _repository.GetByIdAsync(id, cancellationToken),
    TimeSpan.FromMinutes(30),
    cancellationToken);
```

### 2. Memory Management

#### Disposal Patterns
```csharp
// Good: Using statement for disposal
using var scope = _serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IService>();

// Good: IDisposable implementation
public class Service : IDisposable
{
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Dispose managed resources
        }
        _disposed = true;
    }
}
```

### 3. Async Patterns

#### Async Best Practices
```csharp
// Good: ConfigureAwait(false) for library code
public async Task<Result<Universe>> HandleAsync(CreateUniverseCommand request, CancellationToken cancellationToken)
{
    var result = await _repository.AddAsync(universe, cancellationToken).ConfigureAwait(false);
    return Result<Universe>.Success(result);
}

// Good: CancellationToken propagation
public async Task<Universe> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
}
```

## Security Guidelines

### 1. Input Validation

#### Validation Patterns
```csharp
// Good: Validate at multiple layers
public class CreateUniverseCommandValidator : AbstractValidator<CreateUniverseCommand>
{
    public CreateUniverseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(200);
    }
}

// Good: Domain validation
public EntityName(string value) : base(value, 200)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Name cannot be null or whitespace", nameof(value));
}
```

### 2. Error Handling

#### Secure Error Messages
```csharp
// Good: Generic error messages for users
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating universe");
    return Result<Universe>.Failure("An error occurred while creating the universe");
}

// Bad: Exposing internal details
catch (Exception ex)
{
    return Result<Universe>.Failure($"Database error: {ex.Message}");
}
```

## Deployment Guidelines

### 1. Build Configuration

#### Project Files
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Genisis.Core</AssemblyName>
    <RootNamespace>Genisis.Core</RootNamespace>
  </PropertyGroup>
</Project>
```

### 2. Configuration Management

#### App Settings
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Database": {
    "ConnectionString": "Data Source=worldbuilder.db"
  },
  "Cache": {
    "ExpirationMinutes": 30
  }
}
```

### 3. Environment Configuration

#### Environment Variables
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
DATABASE_CONNECTION_STRING=Data Source=worldbuilder_dev.db

# Production
ASPNETCORE_ENVIRONMENT=Production
DATABASE_CONNECTION_STRING=Data Source=worldbuilder_prod.db
```

## Conclusion

These guidelines provide a comprehensive framework for developing WorldBuilder AI. By following these standards, developers can ensure consistency, maintainability, and quality across the entire codebase. Regular reviews and updates of these guidelines will help maintain their relevance and effectiveness.

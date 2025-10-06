# WorldBuilder AI - Architecture Documentation

## Overview

WorldBuilder AI has been completely refactored using modern software architecture principles, implementing Clean Architecture, Domain-Driven Design (DDD), and CQRS patterns. This document outlines the new architecture and its benefits.

## Architecture Principles

### 1. Clean Architecture
The application follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer** (`Genisis.Core`): Contains business logic, entities, value objects, and domain services
- **Application Layer** (`Genisis.Application`): Contains use cases, commands, queries, and application services
- **Infrastructure Layer** (`Genisis.Infrastructure`): Contains data access, external services, and implementations
- **Presentation Layer** (`Genisis.Presentation.Wpf`): Contains UI, ViewModels, and user interaction logic

### 2. Domain-Driven Design (DDD)
- **Entities**: Core business objects with identity and lifecycle
- **Value Objects**: Immutable objects representing concepts without identity
- **Domain Events**: Events raised when domain state changes
- **Domain Services**: Complex business logic that doesn't belong to entities
- **Aggregates**: Consistency boundaries for related entities

### 3. CQRS (Command Query Responsibility Segregation)
- **Commands**: Operations that change state (Create, Update, Delete)
- **Queries**: Operations that read state (Get, Search, List)
- **Handlers**: Process commands and queries independently
- **Results**: Consistent result patterns for all operations

## Project Structure

```
WorldBuilder/
├── Genisis.Core/                    # Domain Layer
│   ├── Models/                      # Domain entities
│   ├── ValueObjects/               # Value objects
│   ├── DomainEvents/               # Domain events
│   ├── Services/                   # Domain services
│   ├── Repositories/               # Repository interfaces
│   └── Exceptions/                 # Domain exceptions
├── Genisis.Application/            # Application Layer
│   ├── Common/                     # Common application components
│   ├── Universe/                   # Universe use cases
│   │   ├── Commands/              # Universe commands
│   │   └── Queries/               # Universe queries
│   └── Services/                   # Application services
├── Genisis.Infrastructure/         # Infrastructure Layer
│   ├── Data/                       # Data access
│   ├── Repositories/               # Repository implementations
│   ├── Services/                   # Infrastructure services
│   ├── Caching/                    # Caching implementations
│   ├── Performance/                # Performance monitoring
│   └── Logging/                    # Logging implementations
├── Genisis.Presentation.Wpf/       # Presentation Layer
│   ├── ViewModels/                 # MVVM ViewModels
│   │   ├── Base/                   # Base ViewModel classes
│   │   └── [Feature]/              # Feature-specific ViewModels
│   ├── Views/                      # XAML Views
│   ├── Services/                   # UI services
│   └── Resources/                  # XAML resources
└── Genisis.Tests/                  # Test Layer
    ├── Unit/                       # Unit tests
    ├── Integration/                # Integration tests
    └── [Feature]/                  # Feature-specific tests
```

## Key Components

### Domain Layer Components

#### Entities
- **Universe**: Root aggregate containing stories and characters
- **Story**: Contains chapters and belongs to a universe
- **Character**: Belongs to a universe and can appear in multiple chapters
- **Chapter**: Belongs to a story and can contain multiple characters

#### Value Objects
- **EntityId**: Strongly-typed identifiers for all entities
- **EntityName**: Validated name with length constraints
- **EntityDescription**: Validated description with length constraints
- **StoryLogline**: Validated story logline
- **CharacterBio**: Validated character biography
- **CharacterNotes**: Validated character notes
- **ChapterContent**: Validated chapter content

#### Domain Events
- **UniverseCreatedEvent**: Raised when a universe is created
- **UniverseUpdatedEvent**: Raised when a universe is updated
- **UniverseDeletedEvent**: Raised when a universe is deleted
- Similar events for Story, Character, and Chapter entities

### Application Layer Components

#### Commands
- **CreateUniverseCommand**: Creates a new universe
- **UpdateUniverseCommand**: Updates an existing universe
- **DeleteUniverseCommand**: Deletes a universe
- Similar commands for other entities

#### Queries
- **GetUniverseQuery**: Gets a universe by ID
- **GetAllUniversesQuery**: Gets all universes
- **SearchUniversesQuery**: Searches universes by term
- Similar queries for other entities

#### Result Pattern
- **Result<T>**: Success/failure result with value
- **Result**: Success/failure result without value
- Consistent error handling across all operations

### Infrastructure Layer Components

#### Repository Pattern
- **BaseRepository<T>**: Common repository functionality
- **UniverseRepository**: Universe-specific data access
- **CachedUniverseRepository**: Cached wrapper for performance
- Similar repositories for other entities

#### Specification Pattern
- **ISpecification<T>**: Query specification interface
- **Specification<T>**: Base specification implementation
- **UniverseSpecifications**: Universe-specific specifications
- Enables complex query composition

#### Caching
- **ICacheService**: Caching interface
- **MemoryCacheService**: Memory-based caching implementation
- **CachedUniverseRepository**: Repository with caching

#### Performance Monitoring
- **IPerformanceMonitor**: Performance monitoring interface
- **PerformanceMonitor**: Performance monitoring implementation
- **PerformanceBehavior**: MediatR behavior for automatic monitoring

### Presentation Layer Components

#### MVVM Pattern
- **ViewModelBase**: Base ViewModel with common functionality
- **EditorViewModelBase<T>**: Base editor ViewModel
- **MainViewModelV2**: Main application ViewModel
- **UniverseViewModelV2**: Universe editor ViewModel

#### Services
- **IDialogService**: Dialog service interface
- **DialogService**: Dialog service implementation
- **IExceptionHandler**: Exception handling interface
- **ExceptionHandler**: Exception handling implementation

## Benefits of the New Architecture

### 1. Maintainability
- Clear separation of concerns
- Single responsibility principle
- Dependency inversion
- Testable components

### 2. Scalability
- CQRS enables independent scaling of read/write operations
- Caching improves performance
- Performance monitoring identifies bottlenecks
- Modular design supports feature additions

### 3. Testability
- Unit tests for domain logic
- Integration tests for data access
- Mocked dependencies
- Isolated components

### 4. Performance
- Caching reduces database load
- Performance monitoring identifies issues
- Optimized queries with specifications
- Lazy loading where appropriate

### 5. Error Handling
- Consistent error handling across layers
- User-friendly error messages
- Comprehensive logging
- Graceful degradation

## Design Patterns Used

### 1. Repository Pattern
- Abstracts data access
- Enables testing with mocks
- Provides consistent interface

### 2. Specification Pattern
- Composable queries
- Reusable query logic
- Type-safe query building

### 3. Unit of Work Pattern
- Transaction management
- Consistent data operations
- Performance optimization

### 4. CQRS Pattern
- Separates read/write operations
- Enables independent optimization
- Supports complex business logic

### 5. Mediator Pattern
- Decouples request/response handling
- Enables cross-cutting concerns
- Simplifies testing

### 6. MVVM Pattern
- Separates UI from business logic
- Enables data binding
- Supports testing

## Best Practices

### 1. Domain Layer
- Keep entities focused on business logic
- Use value objects for validation
- Raise domain events for state changes
- Implement domain services for complex logic

### 2. Application Layer
- Use commands for state changes
- Use queries for data retrieval
- Implement validation in handlers
- Return consistent results

### 3. Infrastructure Layer
- Implement repository interfaces
- Use specifications for queries
- Implement caching where appropriate
- Monitor performance

### 4. Presentation Layer
- Keep ViewModels thin
- Use data binding
- Handle errors gracefully
- Provide user feedback

## Migration Strategy

The new architecture has been implemented alongside the existing code to enable gradual migration:

1. **Phase 1**: New domain models and value objects
2. **Phase 2**: New application layer with CQRS
3. **Phase 3**: New infrastructure layer with caching
4. **Phase 4**: New presentation layer with MVVM
5. **Phase 5**: Comprehensive testing
6. **Phase 6**: Performance optimization
7. **Phase 7**: Error handling and logging

## Future Enhancements

### 1. Event Sourcing
- Store domain events
- Replay events for state reconstruction
- Enable audit trails

### 2. Microservices
- Split into domain services
- Independent deployment
- Scalable architecture

### 3. API Layer
- RESTful API
- GraphQL support
- API versioning

### 4. Advanced Caching
- Distributed caching
- Cache invalidation strategies
- Performance optimization

### 5. Monitoring and Observability
- Application insights
- Health checks
- Metrics collection

## Conclusion

The new architecture provides a solid foundation for WorldBuilder AI's future development. It addresses the original issues while providing room for growth and enhancement. The modular design, comprehensive testing, and performance optimizations ensure a maintainable and scalable application.

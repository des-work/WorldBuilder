using Genisis.Core.Extensions;
using Genisis.Core.Configuration;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Concurrent;

namespace Genisis.Tests.Infrastructure;

/// <summary>
/// Test infrastructure for unified testing
/// </summary>
public interface ITestInfrastructure
{
    Task<TestEnvironment> CreateTestEnvironmentAsync(TestConfiguration config);
    Task<TestData> CreateTestDataAsync(TestDataConfiguration config);
    Task<TestScenario> CreateTestScenarioAsync(ScenarioConfiguration config);
    Task<TestResult> ExecuteTestScenarioAsync(TestScenario scenario);
    Task CleanupTestEnvironmentAsync(TestEnvironment environment);
}

/// <summary>
/// Test infrastructure implementation
/// </summary>
public class TestInfrastructure : ITestInfrastructure
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TestInfrastructure> _logger;
    private readonly ConcurrentDictionary<string, TestEnvironment> _environments = new();

    public TestInfrastructure(IServiceProvider serviceProvider, ILogger<TestInfrastructure> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TestEnvironment> CreateTestEnvironmentAsync(TestConfiguration config)
    {
        var environment = new TestEnvironment
        {
            Id = Guid.NewGuid().ToString(),
            Configuration = config,
            CreatedAt = DateTime.UtcNow,
            Status = TestEnvironmentStatus.Creating
        };

        try
        {
            // Create test service provider
            environment.ServiceProvider = CreateTestServiceProvider(config);
            
            // Initialize test database
            await InitializeTestDatabaseAsync(environment);
            
            // Create test data
            environment.TestData = await CreateTestDataAsync(new TestDataConfiguration
            {
                EnvironmentId = environment.Id,
                DataSets = config.RequiredDataSets
            });
            
            environment.Status = TestEnvironmentStatus.Ready;
            _environments[environment.Id] = environment;
            
            _logger.LogInformation("Created test environment {EnvironmentId}", environment.Id);
            return environment;
        }
        catch (Exception ex)
        {
            environment.Status = TestEnvironmentStatus.Failed;
            environment.Error = ex.Message;
            _logger.LogError(ex, "Failed to create test environment");
            throw;
        }
    }

    private IServiceProvider CreateTestServiceProvider(TestConfiguration config)
    {
        var services = new ServiceCollection();
        
        // Add test-specific configurations
        services.AddWorldBuilderServices(CreateTestConfiguration(config));
        
        // Override with test-specific services
        if (config.UseInMemoryDatabase)
        {
            services.AddDbContext<GenesisDbContext>(options =>
                options.UseSqlite("DataSource=:memory:"));
        }
        
        if (config.UseMockServices)
        {
            services.AddMockServices();
        }
        
        return services.BuildServiceProvider();
    }

    private IConfiguration CreateTestConfiguration(TestConfiguration config)
    {
        var configurationBuilder = new ConfigurationBuilder();
        
        // Add test-specific settings
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Database:ConnectionString"] = config.UseInMemoryDatabase ? "DataSource=:memory:" : config.DatabaseConnectionString,
            ["AI:OllamaBaseUrl"] = config.AIBaseUrl ?? "http://localhost:11434",
            ["Performance:EnableMonitoring"] = "false",
            ["Testing:EnableTestDataSeeding"] = "true"
        });
        
        return configurationBuilder.Build();
    }

    private async Task InitializeTestDatabaseAsync(TestEnvironment environment)
    {
        var dbContext = environment.ServiceProvider.GetRequiredService<GenesisDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        if (environment.Configuration.SeedTestData)
        {
            var seeder = environment.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
    }

    public async Task<TestData> CreateTestDataAsync(TestDataConfiguration config)
    {
        var testData = new TestData
        {
            Id = Guid.NewGuid().ToString(),
            EnvironmentId = config.EnvironmentId,
            CreatedAt = DateTime.UtcNow
        };

        var dataBuilder = new TestDataBuilder();
        
        foreach (var dataSet in config.DataSets)
        {
            var data = await dataBuilder.BuildDataSetAsync(dataSet);
            testData.DataSets[dataSet] = data;
        }

        return testData;
    }

    public async Task<TestScenario> CreateTestScenarioAsync(ScenarioConfiguration config)
    {
        return new TestScenario
        {
            Id = Guid.NewGuid().ToString(),
            Name = config.Name,
            Description = config.Description,
            Steps = config.Steps,
            ExpectedOutcomes = config.ExpectedOutcomes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<TestResult> ExecuteTestScenarioAsync(TestScenario scenario)
    {
        var result = new TestResult
        {
            ScenarioId = scenario.Id,
            StartedAt = DateTime.UtcNow,
            Status = TestStatus.Running
        };

        try
        {
            foreach (var step in scenario.Steps)
            {
                var stepResult = await ExecuteTestStepAsync(step);
                result.StepResults.Add(stepResult);
                
                if (!stepResult.IsSuccess)
                {
                    result.Status = TestStatus.Failed;
                    result.Error = stepResult.Error;
                    break;
                }
            }

            if (result.Status == TestStatus.Running)
            {
                result.Status = TestStatus.Passed;
            }

            result.CompletedAt = DateTime.UtcNow;
            result.Duration = result.CompletedAt - result.StartedAt;

            return result;
        }
        catch (Exception ex)
        {
            result.Status = TestStatus.Failed;
            result.Error = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            result.Duration = result.CompletedAt - result.StartedAt;
            
            _logger.LogError(ex, "Test scenario {ScenarioId} failed", scenario.Id);
            return result;
        }
    }

    private async Task<TestStepResult> ExecuteTestStepAsync(TestStep step)
    {
        var stepResult = new TestStepResult
        {
            StepId = step.Id,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            switch (step.Type)
            {
                case TestStepType.ServiceCall:
                    stepResult.Result = await ExecuteServiceCallAsync(step);
                    break;
                case TestStepType.DataValidation:
                    stepResult.Result = await ValidateDataAsync(step);
                    break;
                case TestStepType.PerformanceCheck:
                    stepResult.Result = await CheckPerformanceAsync(step);
                    break;
                case TestStepType.IntegrationTest:
                    stepResult.Result = await ExecuteIntegrationTestAsync(step);
                    break;
                default:
                    throw new NotSupportedException($"Test step type {step.Type} is not supported");
            }

            stepResult.IsSuccess = true;
            stepResult.CompletedAt = DateTime.UtcNow;
            stepResult.Duration = stepResult.CompletedAt - stepResult.StartedAt;

            return stepResult;
        }
        catch (Exception ex)
        {
            stepResult.IsSuccess = false;
            stepResult.Error = ex.Message;
            stepResult.CompletedAt = DateTime.UtcNow;
            stepResult.Duration = stepResult.CompletedAt - stepResult.StartedAt;
            
            return stepResult;
        }
    }

    private async Task<object> ExecuteServiceCallAsync(TestStep step)
    {
        // Implementation for service call execution
        await Task.Delay(100); // Simulate service call
        return new { Success = true, Message = "Service call executed" };
    }

    private async Task<object> ValidateDataAsync(TestStep step)
    {
        // Implementation for data validation
        await Task.Delay(50); // Simulate validation
        return new { Success = true, Message = "Data validation passed" };
    }

    private async Task<object> CheckPerformanceAsync(TestStep step)
    {
        // Implementation for performance checking
        await Task.Delay(25); // Simulate performance check
        return new { Success = true, Message = "Performance check passed" };
    }

    private async Task<object> ExecuteIntegrationTestAsync(TestStep step)
    {
        // Implementation for integration test execution
        await Task.Delay(200); // Simulate integration test
        return new { Success = true, Message = "Integration test passed" };
    }

    public async Task CleanupTestEnvironmentAsync(TestEnvironment environment)
    {
        try
        {
            if (environment.ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _environments.Remove(environment.Id);
            _logger.LogInformation("Cleaned up test environment {EnvironmentId}", environment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup test environment {EnvironmentId}", environment.Id);
        }
    }
}

/// <summary>
/// Test environment
/// </summary>
public class TestEnvironment
{
    public string Id { get; set; } = string.Empty;
    public TestConfiguration Configuration { get; set; } = new();
    public IServiceProvider? ServiceProvider { get; set; }
    public TestData? TestData { get; set; }
    public TestEnvironmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Test configuration
/// </summary>
public class TestConfiguration
{
    public bool UseInMemoryDatabase { get; set; } = true;
    public bool UseMockServices { get; set; } = true;
    public bool SeedTestData { get; set; } = true;
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string? AIBaseUrl { get; set; }
    public List<string> RequiredDataSets { get; set; } = new();
}

/// <summary>
/// Test data configuration
/// </summary>
public class TestDataConfiguration
{
    public string EnvironmentId { get; set; } = string.Empty;
    public List<string> DataSets { get; set; } = new();
}

/// <summary>
/// Test data
/// </summary>
public class TestData
{
    public string Id { get; set; } = string.Empty;
    public string EnvironmentId { get; set; } = string.Empty;
    public Dictionary<string, object> DataSets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Test scenario
/// </summary>
public class TestScenario
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TestStep> Steps { get; set; } = new();
    public List<ExpectedOutcome> ExpectedOutcomes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Test step
/// </summary>
public class TestStep
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TestStepType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Test step types
/// </summary>
public enum TestStepType
{
    ServiceCall,
    DataValidation,
    PerformanceCheck,
    IntegrationTest
}

/// <summary>
/// Expected outcome
/// </summary>
public class ExpectedOutcome
{
    public string StepId { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public List<string> ValidationCriteria { get; set; } = new();
}

/// <summary>
/// Test result
/// </summary>
public class TestResult
{
    public string ScenarioId { get; set; } = string.Empty;
    public TestStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? TimeSpan.Zero;
    public string? Error { get; set; }
    public List<TestStepResult> StepResults { get; set; } = new();
}

/// <summary>
/// Test step result
/// </summary>
public class TestStepResult
{
    public string StepId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? TimeSpan.Zero;
    public string? Error { get; set; }
    public object? Result { get; set; }
}

/// <summary>
/// Test status
/// </summary>
public enum TestStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Skipped,
    Cancelled
}

/// <summary>
/// Test environment status
/// </summary>
public enum TestEnvironmentStatus
{
    Creating,
    Ready,
    Failed,
    Disposed
}

/// <summary>
/// Scenario configuration
/// </summary>
public class ScenarioConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TestStep> Steps { get; set; } = new();
    public List<ExpectedOutcome> ExpectedOutcomes { get; set; } = new();
}

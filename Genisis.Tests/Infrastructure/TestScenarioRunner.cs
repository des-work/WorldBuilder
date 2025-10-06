using Microsoft.Extensions.Logging;

namespace Genisis.Tests.Infrastructure;

/// <summary>
/// Interface for test scenario runner
/// </summary>
public interface ITestScenarioRunner
{
    Task<TestExecutionResult> RunScenarioAsync(string scenarioName);
    Task<TestExecutionResult> RunScenarioAsync(ScenarioConfiguration scenario);
    Task<List<TestExecutionResult>> RunAllScenariosAsync();
    Task<TestExecutionResult> RunScenarioWithDataAsync(string scenarioName, TestData testData);
}

/// <summary>
/// Test scenario runner implementation
/// </summary>
public class TestScenarioRunner : ITestScenarioRunner
{
    private readonly ITestInfrastructure _testInfrastructure;
    private readonly ITestDataBuilder _testDataBuilder;
    private readonly ILogger<TestScenarioRunner> _logger;
    private readonly Dictionary<string, ScenarioConfiguration> _scenarios = new();

    public TestScenarioRunner(
        ITestInfrastructure testInfrastructure,
        ITestDataBuilder testDataBuilder,
        ILogger<TestScenarioRunner> logger)
    {
        _testInfrastructure = testInfrastructure;
        _testDataBuilder = testDataBuilder;
        _logger = logger;
        
        InitializeScenarios();
    }

    public async Task<TestExecutionResult> RunScenarioAsync(string scenarioName)
    {
        if (!_scenarios.TryGetValue(scenarioName, out var scenario))
        {
            throw new ArgumentException($"Scenario '{scenarioName}' not found");
        }

        return await RunScenarioAsync(scenario);
    }

    public async Task<TestExecutionResult> RunScenarioAsync(ScenarioConfiguration scenario)
    {
        var executionResult = new TestExecutionResult
        {
            ScenarioName = scenario.Name,
            StartedAt = DateTime.UtcNow,
            Status = TestExecutionStatus.Running
        };

        try
        {
            _logger.LogInformation("Starting scenario: {ScenarioName}", scenario.Name);

            // Create test environment
            var testConfig = new TestConfiguration
            {
                UseInMemoryDatabase = true,
                UseMockServices = true,
                SeedTestData = true,
                RequiredDataSets = GetRequiredDataSets(scenario)
            };

            var environment = await _testInfrastructure.CreateTestEnvironmentAsync(testConfig);
            executionResult.EnvironmentId = environment.Id;

            // Execute scenario
            var testScenario = await _testInfrastructure.CreateTestScenarioAsync(scenario);
            var result = await _testInfrastructure.ExecuteTestScenarioAsync(testScenario);
            
            executionResult.TestResult = result;
            executionResult.Status = result.Status == TestStatus.Passed ? TestExecutionStatus.Passed : TestExecutionStatus.Failed;
            executionResult.CompletedAt = DateTime.UtcNow;
            executionResult.Duration = executionResult.CompletedAt - executionResult.StartedAt;

            // Cleanup
            await _testInfrastructure.CleanupTestEnvironmentAsync(environment);

            _logger.LogInformation("Completed scenario: {ScenarioName} in {Duration}ms", 
                scenario.Name, executionResult.Duration.TotalMilliseconds);

            return executionResult;
        }
        catch (Exception ex)
        {
            executionResult.Status = TestExecutionStatus.Failed;
            executionResult.Error = ex.Message;
            executionResult.CompletedAt = DateTime.UtcNow;
            executionResult.Duration = executionResult.CompletedAt - executionResult.StartedAt;

            _logger.LogError(ex, "Scenario {ScenarioName} failed", scenario.Name);
            return executionResult;
        }
    }

    public async Task<List<TestExecutionResult>> RunAllScenariosAsync()
    {
        var results = new List<TestExecutionResult>();

        foreach (var scenario in _scenarios.Values)
        {
            try
            {
                var result = await RunScenarioAsync(scenario);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run scenario: {ScenarioName}", scenario.Name);
                
                results.Add(new TestExecutionResult
                {
                    ScenarioName = scenario.Name,
                    Status = TestExecutionStatus.Failed,
                    Error = ex.Message,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }

    public async Task<TestExecutionResult> RunScenarioWithDataAsync(string scenarioName, TestData testData)
    {
        if (!_scenarios.TryGetValue(scenarioName, out var scenario))
        {
            throw new ArgumentException($"Scenario '{scenarioName}' not found");
        }

        var executionResult = new TestExecutionResult
        {
            ScenarioName = scenario.Name,
            StartedAt = DateTime.UtcNow,
            Status = TestExecutionStatus.Running
        };

        try
        {
            _logger.LogInformation("Starting scenario with custom data: {ScenarioName}", scenario.Name);

            // Create test environment with custom data
            var testConfig = new TestConfiguration
            {
                UseInMemoryDatabase = true,
                UseMockServices = true,
                SeedTestData = false,
                RequiredDataSets = new List<string>()
            };

            var environment = await _testInfrastructure.CreateTestEnvironmentAsync(testConfig);
            environment.TestData = testData;
            executionResult.EnvironmentId = environment.Id;

            // Execute scenario
            var testScenario = await _testInfrastructure.CreateTestScenarioAsync(scenario);
            var result = await _testInfrastructure.ExecuteTestScenarioAsync(testScenario);
            
            executionResult.TestResult = result;
            executionResult.Status = result.Status == TestStatus.Passed ? TestExecutionStatus.Passed : TestExecutionStatus.Failed;
            executionResult.CompletedAt = DateTime.UtcNow;
            executionResult.Duration = executionResult.CompletedAt - executionResult.StartedAt;

            // Cleanup
            await _testInfrastructure.CleanupTestEnvironmentAsync(environment);

            _logger.LogInformation("Completed scenario with custom data: {ScenarioName} in {Duration}ms", 
                scenario.Name, executionResult.Duration.TotalMilliseconds);

            return executionResult;
        }
        catch (Exception ex)
        {
            executionResult.Status = TestExecutionStatus.Failed;
            executionResult.Error = ex.Message;
            executionResult.CompletedAt = DateTime.UtcNow;
            executionResult.Duration = executionResult.CompletedAt - executionResult.StartedAt;

            _logger.LogError(ex, "Scenario with custom data {ScenarioName} failed", scenario.Name);
            return executionResult;
        }
    }

    private void InitializeScenarios()
    {
        // Full Workflow Scenario
        _scenarios["FullWorkflow"] = new ScenarioConfiguration
        {
            Name = "Full Workflow Integration",
            Description = "Tests the complete workflow from startup to story creation",
            Steps = new List<TestStep>
            {
                new TestStep
                {
                    Id = "startup",
                    Type = TestStepType.ServiceCall,
                    Name = "Application Startup",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Service"] = "IStartupService",
                        ["Method"] = "StartApplicationAsync"
                    }
                },
                new TestStep
                {
                    Id = "universe_creation",
                    Type = TestStepType.ServiceCall,
                    Name = "Universe Creation",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Service"] = "IUniverseRepository",
                        ["Method"] = "AddAsync",
                        ["Data"] = new { Name = "Test Universe", Description = "Test Description" }
                    }
                }
            },
            ExpectedOutcomes = new List<ExpectedOutcome>
            {
                new ExpectedOutcome
                {
                    StepId = "startup",
                    ExpectedResult = "Application starts successfully",
                    ValidationCriteria = new List<string> { "No errors", "Services initialized" }
                },
                new ExpectedOutcome
                {
                    StepId = "universe_creation",
                    ExpectedResult = "Universe created successfully",
                    ValidationCriteria = new List<string> { "Universe saved", "ID generated" }
                }
            }
        };

        // AI Integration Scenario
        _scenarios["AIIntegration"] = new ScenarioConfiguration
        {
            Name = "AI Integration Workflow",
            Description = "Tests AI services integration with story creation",
            Steps = new List<TestStep>
            {
                new TestStep
                {
                    Id = "model_discovery",
                    Type = TestStepType.ServiceCall,
                    Name = "Model Discovery",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Service"] = "ILocalModelManager",
                        ["Method"] = "DiscoverModelsAsync"
                    }
                },
                new TestStep
                {
                    Id = "smart_query",
                    Type = TestStepType.ServiceCall,
                    Name = "Smart Query Processing",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Service"] = "ISmartQuerySystem",
                        ["Method"] = "ProcessQueryAsync",
                        ["Data"] = new { Query = "Create a character for my fantasy story" }
                    }
                }
            },
            ExpectedOutcomes = new List<ExpectedOutcome>
            {
                new ExpectedOutcome
                {
                    StepId = "model_discovery",
                    ExpectedResult = "Models discovered successfully",
                    ValidationCriteria = new List<string> { "Models found", "Validation passed" }
                },
                new ExpectedOutcome
                {
                    StepId = "smart_query",
                    ExpectedResult = "Query processed successfully",
                    ValidationCriteria = new List<string> { "Intent detected", "Response generated" }
                }
            }
        };

        // Performance Optimization Scenario
        _scenarios["PerformanceOptimization"] = new ScenarioConfiguration
        {
            Name = "Performance Optimization Workflow",
            Description = "Tests performance optimization across all components",
            Steps = new List<TestStep>
            {
                new TestStep
                {
                    Id = "memory_check",
                    Type = TestStepType.PerformanceCheck,
                    Name = "Memory Usage Check",
                    Parameters = new Dictionary<string, object>
                    {
                        ["MaxMemoryUsage"] = 200 * 1024 * 1024, // 200MB
                        ["TestDuration"] = TimeSpan.FromMinutes(1)
                    }
                },
                new TestStep
                {
                    Id = "caching_test",
                    Type = TestStepType.ServiceCall,
                    Name = "Caching Test",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Service"] = "IResponseCacheSystem",
                        ["Method"] = "GetCacheStatisticsAsync"
                    }
                }
            },
            ExpectedOutcomes = new List<ExpectedOutcome>
            {
                new ExpectedOutcome
                {
                    StepId = "memory_check",
                    ExpectedResult = "Memory usage within limits",
                    ValidationCriteria = new List<string> { "Memory < 200MB", "No leaks detected" }
                },
                new ExpectedOutcome
                {
                    StepId = "caching_test",
                    ExpectedResult = "Caching working effectively",
                    ValidationCriteria = new List<string> { "Cache accessible", "Statistics available" }
                }
            }
        };
    }

    private List<string> GetRequiredDataSets(ScenarioConfiguration scenario)
    {
        var dataSets = new List<string>();

        // Analyze scenario steps to determine required data sets
        foreach (var step in scenario.Steps)
        {
            if (step.Parameters.TryGetValue("Data", out var data) && data is Dictionary<string, object> dataDict)
            {
                if (dataDict.ContainsKey("Name") && dataDict["Name"].ToString()!.Contains("Universe"))
                {
                    dataSets.Add("BasicUniverse");
                }
                else if (dataDict.ContainsKey("Name") && dataDict["Name"].ToString()!.Contains("Story"))
                {
                    dataSets.Add("BasicStory");
                }
                else if (dataDict.ContainsKey("Name") && dataDict["Name"].ToString()!.Contains("Character"))
                {
                    dataSets.Add("BasicCharacter");
                }
            }
        }

        // Add default data sets if none specified
        if (!dataSets.Any())
        {
            dataSets.Add("BasicUniverse");
        }

        return dataSets.Distinct().ToList();
    }
}

/// <summary>
/// Test execution result
/// </summary>
public class TestExecutionResult
{
    public string ScenarioName { get; set; } = string.Empty;
    public string? EnvironmentId { get; set; }
    public TestExecutionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? TimeSpan.Zero;
    public string? Error { get; set; }
    public TestResult? TestResult { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Test execution status
/// </summary>
public enum TestExecutionStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Skipped,
    Cancelled
}

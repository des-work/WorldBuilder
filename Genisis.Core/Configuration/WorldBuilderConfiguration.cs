namespace Genisis.Core.Configuration;

/// <summary>
/// WorldBuilder application configuration
/// </summary>
public class WorldBuilderConfiguration
{
    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseConfiguration Database { get; set; } = new();
    
    /// <summary>
    /// AI configuration
    /// </summary>
    public AIConfiguration AI { get; set; } = new();
    
    /// <summary>
    /// Performance configuration
    /// </summary>
    public PerformanceConfiguration Performance { get; set; } = new();
    
    /// <summary>
    /// UI configuration
    /// </summary>
    public UIConfiguration UI { get; set; } = new();
    
    /// <summary>
    /// Testing configuration
    /// </summary>
    public TestingConfiguration Testing { get; set; } = new();
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
    
    /// <summary>
    /// Enable detailed errors
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;
    
    /// <summary>
    /// Enable retry on failure
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;
    
    /// <summary>
    /// Maximum retry count
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
}

/// <summary>
/// AI configuration
/// </summary>
public class AIConfiguration
{
    /// <summary>
    /// Ollama base URL
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeout { get; set; } = 300;
    
    /// <summary>
    /// Enable caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// Cache expiration in minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 30;
    
    /// <summary>
    /// Enable performance profiling
    /// </summary>
    public bool EnablePerformanceProfiling { get; set; } = true;
    
    /// <summary>
    /// Enable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker recovery timeout in seconds
    /// </summary>
    public int CircuitBreakerRecoveryTimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// Performance configuration
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>
    /// Enable memory optimization
    /// </summary>
    public bool EnableMemoryOptimization { get; set; } = true;
    
    /// <summary>
    /// Maximum memory usage in MB
    /// </summary>
    public long MaxMemoryUsageMB { get; set; } = 1024;
    
    /// <summary>
    /// Enable background processing
    /// </summary>
    public bool EnableBackgroundProcessing { get; set; } = true;
    
    /// <summary>
    /// Background processing threads
    /// </summary>
    public int BackgroundProcessingThreads { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;
    
    /// <summary>
    /// Performance monitoring interval in seconds
    /// </summary>
    public int PerformanceMonitoringIntervalSeconds { get; set; } = 60;
}

/// <summary>
/// UI configuration
/// </summary>
public class UIConfiguration
{
    /// <summary>
    /// Default theme
    /// </summary>
    public string DefaultTheme { get; set; } = "Fantasy";
    
    /// <summary>
    /// Enable bootscreen
    /// </summary>
    public bool EnableBootscreen { get; set; } = true;
    
    /// <summary>
    /// Bootscreen duration in seconds
    /// </summary>
    public int BootscreenDurationSeconds { get; set; } = 5;
    
    /// <summary>
    /// Enable animations
    /// </summary>
    public bool EnableAnimations { get; set; } = true;
    
    /// <summary>
    /// Enable tooltips
    /// </summary>
    public bool EnableTooltips { get; set; } = true;
}

/// <summary>
/// Testing configuration
/// </summary>
public class TestingConfiguration
{
    /// <summary>
    /// Enable test data seeding
    /// </summary>
    public bool EnableTestDataSeeding { get; set; } = true;
    
    /// <summary>
    /// Enable performance testing
    /// </summary>
    public bool EnablePerformanceTesting { get; set; } = true;
    
    /// <summary>
    /// Enable integration testing
    /// </summary>
    public bool EnableIntegrationTesting { get; set; } = true;
    
    /// <summary>
    /// Test timeout in seconds
    /// </summary>
    public int TestTimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// Enable test logging
    /// </summary>
    public bool EnableTestLogging { get; set; } = true;
}

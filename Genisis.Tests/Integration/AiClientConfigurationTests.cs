using System.Reflection;
using FluentAssertions;
using Genisis.Core.Extensions;
using Genisis.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genisis.Tests.Integration;

public class AiClientConfigurationTests
{
    [Fact]
    public void IAiService_Uses_BaseUrl_From_Configuration()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUrl = "http://localhost:12345";

        var inMemory = new Dictionary<string, string?>
        {
            ["AI:OllamaBaseUrl"] = baseUrl,
            ["AI:RequestTimeout"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory!)
            .Build();

        services.AddWorldBuilderServices(configuration);

        var sp = services.BuildServiceProvider();

        // Act
        var ai = sp.GetRequiredService<IAiService>();

        // Assert (via reflection to inspect HttpClient in implementation)
        var httpClientField = ai.GetType().GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        httpClientField.Should().NotBeNull();
        var httpClient = httpClientField!.GetValue(ai) as System.Net.Http.HttpClient;
        httpClient.Should().NotBeNull();
        httpClient!.BaseAddress.Should().Be(new Uri(baseUrl));
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(60));
    }
}


using Microsoft.Extensions.Logging;
using SupportAssistant.Core.Models;
using SupportAssistant.AI.Services;
using SupportAssistant.KnowledgeBase.Services;

namespace SupportAssistant.Core.Tests;

public class ServicesIntegrationTests
{
    [Fact]
    public async Task OnnxAIService_ShouldInitializeAndGenerateResponse()
    {
        // Arrange
        var logger = new TestLogger<OnnxAIService>();
        var aiService = new OnnxAIService(logger);

        // Act
        await aiService.InitializeAsync();

        var query = new AIQuery { Text = "How do I fix Windows Update issues?" };
        var response = await aiService.GenerateResponseAsync(query);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Text.Should().NotBeEmpty();
        response.Confidence.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SqliteKnowledgeBaseService_ShouldInitializeAndSearch()
    {
        // Arrange
        var logger = new TestLogger<SqliteKnowledgeBaseService>();
        var kbService = new SqliteKnowledgeBaseService(logger);

        // Act
        await kbService.InitializeAsync();

        var results = await kbService.SearchAsync("Windows Update", 3);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.First().Title.Should().NotBeEmpty();
        results.First().RelevanceScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SqliteKnowledgeBaseService_ShouldAddAndRetrieveContent()
    {
        // Arrange
        var logger = new TestLogger<SqliteKnowledgeBaseService>();
        var kbService = new SqliteKnowledgeBaseService(logger);
        await kbService.InitializeAsync();

        var testContent = "This is a test technical article about network troubleshooting.";
        var metadata = new Dictionary<string, object>
        {
            ["category"] = "networking",
            ["difficulty"] = "intermediate"
        };

        // Act
        await kbService.AddContentAsync(testContent, metadata);
        var results = await kbService.SearchAsync("network troubleshooting", 5);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(r => r.Excerpt.Contains("network troubleshooting"));
    }

    private class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Test logger - output to console for debugging
            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
    }
}
using SupportAssistant.Core.Models;

namespace SupportAssistant.Core.Interfaces;

/// <summary>
/// Interface for AI inference services
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generate a response to a query
    /// </summary>
    /// <param name="query">The query to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI response</returns>
    Task<AIResponse> GenerateResponseAsync(AIQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a streaming response to a query
    /// </summary>
    /// <param name="query">The query to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of response chunks</returns>
    IAsyncEnumerable<string> GenerateStreamingResponseAsync(AIQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the AI service is ready for inference
    /// </summary>
    bool IsReady { get; }
    
    /// <summary>
    /// Initialize the AI service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
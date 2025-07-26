using SupportAssistant.Core.Models;

namespace SupportAssistant.Core.Interfaces;

/// <summary>
/// Interface for knowledge base operations
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Search the knowledge base for relevant content
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant knowledge sources</returns>
    Task<List<KnowledgeSource>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add content to the knowledge base
    /// </summary>
    /// <param name="content">Content to add</param>
    /// <param name="metadata">Content metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddContentAsync(string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the knowledge base is ready
    /// </summary>
    bool IsReady { get; }
    
    /// <summary>
    /// Initialize the knowledge base
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
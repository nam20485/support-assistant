namespace SupportAssistant.Core.Models;

/// <summary>
/// Represents a query to the AI system
/// </summary>
public record AIQuery
{
    /// <summary>
    /// The user's question or request
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// Optional context or conversation history
    /// </summary>
    public List<ChatMessage>? Context { get; init; }
    
    /// <summary>
    /// Query metadata and configuration
    /// </summary>
    public QueryConfiguration Configuration { get; init; } = new();
}

/// <summary>
/// Configuration options for AI queries
/// </summary>
public record QueryConfiguration
{
    /// <summary>
    /// Maximum number of tokens to generate in response
    /// </summary>
    public int MaxTokens { get; init; } = 512;
    
    /// <summary>
    /// Temperature for response generation (0.0 - 1.0)
    /// </summary>
    public float Temperature { get; init; } = 0.7f;
    
    /// <summary>
    /// Whether to enable RAG (Retrieval-Augmented Generation)
    /// </summary>
    public bool EnableRAG { get; init; } = true;
    
    /// <summary>
    /// Number of knowledge base chunks to retrieve for RAG
    /// </summary>
    public int MaxRetrievedChunks { get; init; } = 5;
}
namespace SupportAssistant.Core.Models;

/// <summary>
/// Represents a response from the AI system
/// </summary>
public record AIResponse
{
    /// <summary>
    /// The generated response text
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// Confidence score of the response (0.0 - 1.0)
    /// </summary>
    public float Confidence { get; init; }
    
    /// <summary>
    /// Sources used to generate the response (for RAG)
    /// </summary>
    public List<KnowledgeSource>? Sources { get; init; }
    
    /// <summary>
    /// Whether the response was generated successfully
    /// </summary>
    public bool IsSuccess { get; init; } = true;
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Response generation metadata
    /// </summary>
    public ResponseMetadata Metadata { get; init; } = new();
}

/// <summary>
/// Metadata about the AI response generation
/// </summary>
public record ResponseMetadata
{
    /// <summary>
    /// Time taken to generate the response
    /// </summary>
    public TimeSpan GenerationTime { get; init; }
    
    /// <summary>
    /// Number of tokens in the generated response
    /// </summary>
    public int TokenCount { get; init; }
    
    /// <summary>
    /// Model used for generation
    /// </summary>
    public string? ModelName { get; init; }
    
    /// <summary>
    /// Whether RAG was used for this response
    /// </summary>
    public bool UsedRAG { get; init; }
}

/// <summary>
/// Represents a knowledge source used in response generation
/// </summary>
public record KnowledgeSource
{
    /// <summary>
    /// Title or identifier of the source
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Relevance score (0.0 - 1.0)
    /// </summary>
    public float RelevanceScore { get; init; }
    
    /// <summary>
    /// URL or reference to the source
    /// </summary>
    public string? Url { get; init; }
    
    /// <summary>
    /// Excerpt from the source used in generation
    /// </summary>
    public string? Excerpt { get; init; }
}
namespace SupportAssistant.Core.Models;

/// <summary>
/// Represents a chat message in the conversation
/// </summary>
public record ChatMessage
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The content of the message
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// The sender of the message (User or Assistant)
    /// </summary>
    public required MessageSender Sender { get; init; }
    
    /// <summary>
    /// When the message was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional metadata associated with the message
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents the sender of a chat message
/// </summary>
public enum MessageSender
{
    /// <summary>
    /// Message sent by the user
    /// </summary>
    User,
    
    /// <summary>
    /// Message sent by the AI assistant
    /// </summary>
    Assistant,
    
    /// <summary>
    /// System message (notifications, status updates, etc.)
    /// </summary>
    System
}
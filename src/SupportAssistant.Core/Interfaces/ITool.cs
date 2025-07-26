namespace SupportAssistant.Core.Interfaces;

/// <summary>
/// Interface for system modification tools
/// </summary>
public interface ITool
{
    /// <summary>
    /// Name of the tool
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Description of what the tool does
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Parameters required by the tool
    /// </summary>
    IReadOnlyList<ToolParameter> Parameters { get; }
    
    /// <summary>
    /// Execute the tool with given parameters
    /// </summary>
    /// <param name="parameters">Parameters for execution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of tool execution</returns>
    Task<ToolResult> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate parameters before execution
    /// </summary>
    /// <param name="parameters">Parameters to validate</param>
    /// <returns>Validation result</returns>
    ToolValidationResult ValidateParameters(Dictionary<string, object> parameters);
}

/// <summary>
/// Represents a tool parameter
/// </summary>
public record ToolParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Parameter description
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Parameter type
    /// </summary>
    public required Type Type { get; init; }
    
    /// <summary>
    /// Whether the parameter is required
    /// </summary>
    public bool Required { get; init; } = true;
    
    /// <summary>
    /// Default value if parameter is optional
    /// </summary>
    public object? DefaultValue { get; init; }
}

/// <summary>
/// Result of tool execution
/// </summary>
public record ToolResult
{
    /// <summary>
    /// Whether the tool executed successfully
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Result message or output
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Additional result data
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// Result of parameter validation
/// </summary>
public record ToolValidationResult
{
    /// <summary>
    /// Whether validation passed
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// Validation error messages
    /// </summary>
    public List<string> Errors { get; init; } = new();
}
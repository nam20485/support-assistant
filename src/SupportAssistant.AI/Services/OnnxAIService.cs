using Microsoft.ML.OnnxRuntime;
using Microsoft.Extensions.Logging;
using SupportAssistant.Core.Models;
using SupportAssistant.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace SupportAssistant.AI.Services;

/// <summary>
/// ONNX Runtime-based AI service implementation
/// </summary>
public class OnnxAIService : IAIService, IDisposable
{
    private static readonly Random _random = new();
    private readonly ILogger<OnnxAIService> _logger;
    private InferenceSession? _session;
    private SessionOptions? _sessionOptions;
    private bool _isInitialized = false;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the OnnxAIService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public OnnxAIService(ILogger<OnnxAIService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool IsReady => _isInitialized;

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger.LogInformation("ONNX AI Service is already initialized");
            return Task.CompletedTask;
        }

        try
        {
            _logger.LogInformation("Initializing ONNX AI Service...");

            // Create session options with DirectML provider
            _sessionOptions = new SessionOptions();
            
            // Try to use DirectML provider for GPU acceleration
            try
            {
                _sessionOptions.AppendExecutionProvider_DML(0);
                _logger.LogInformation("DirectML execution provider enabled");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enable DirectML provider, falling back to CPU");
            }

            // Add CPU provider as fallback
            _sessionOptions.AppendExecutionProvider_CPU();

            // For now, we'll use a placeholder path - in a real implementation,
            // this would be the path to the Phi-3-mini ONNX model
            var modelPath = GetModelPath();

            if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
            {
                _session = new InferenceSession(modelPath, _sessionOptions);
                _logger.LogInformation("ONNX model loaded from: {ModelPath}", modelPath);
            }
            else
            {
                _logger.LogWarning("Model file not found. AI service will operate in mock mode.");
                // In a real implementation, you might download the model here
            }

            _isInitialized = true;
            _logger.LogInformation("ONNX AI Service initialized successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ONNX AI Service");
            return Task.FromException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<AIResponse> GenerateResponseAsync(AIQuery query, CancellationToken cancellationToken = default)
    {
        if (!IsReady)
        {
            return new AIResponse
            {
                Text = "AI service is not ready. Please ensure the model is loaded.",
                IsSuccess = false,
                ErrorMessage = "Service not initialized"
            };
        }

        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Processing query: {Query}", query.Text);

            // For now, return a mock response
            // In a real implementation, this would:
            // 1. Tokenize the input
            // 2. Run inference with the ONNX model
            // 3. Decode the output tokens
            var response = await GenerateMockResponseAsync(query, cancellationToken);

            var endTime = DateTime.UtcNow;
            response = response with
            {
                Metadata = response.Metadata with
                {
                    GenerationTime = endTime - startTime,
                    ModelName = "Phi-3-mini (Mock)"
                }
            };

            _logger.LogInformation("Generated response in {Duration}ms", response.Metadata.GenerationTime.TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response");
            return new AIResponse
            {
                Text = "An error occurred while generating the response.",
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Metadata = new ResponseMetadata
                {
                    GenerationTime = DateTime.UtcNow - startTime
                }
            };
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(AIQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsReady)
        {
            yield return "AI service is not ready. Please ensure the model is loaded.";
            yield break;
        }

        _logger.LogInformation("Starting streaming response for query: {Query}", query.Text);

        // Mock streaming response
        var mockResponse = await GenerateMockResponseAsync(query, cancellationToken);
        var words = mockResponse.Text.Split(' ');

        foreach (var word in words)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return word + " ";
            await Task.Delay(50, cancellationToken); // Simulate processing time
        }
    }

    private async Task<AIResponse> GenerateMockResponseAsync(AIQuery query, CancellationToken cancellationToken)
    {
        // Simulate processing time
        await Task.Delay(100, cancellationToken);

        var mockResponses = new[]
        {
            "I understand you're asking about technical support. While I'm currently operating in demo mode, I would normally use my knowledge base to provide specific guidance for your technical issue.",
            "Based on your query, I would typically search through Microsoft Learn documentation and Stack Overflow solutions to find the most relevant answer for your specific problem.",
            "In a fully operational state, I would analyze your question using natural language processing and retrieve the most accurate technical documentation to help resolve your issue.",
            "Your technical question would normally be processed through my RAG (Retrieval-Augmented Generation) system to provide factual, well-sourced answers from trusted technical resources."
        };

        var random = _random;
        var responseText = mockResponses[random.Next(mockResponses.Length)];

        return new AIResponse
        {
            Text = responseText,
            Confidence = 0.85f,
            IsSuccess = true,
            Metadata = new ResponseMetadata
            {
                TokenCount = responseText.Split(' ').Length,
                UsedRAG = false // Mock mode doesn't use RAG
            }
        };
    }

    private string GetModelPath()
    {
        // In a real implementation, this would return the path to the Phi-3-mini model
        // For now, we'll check common locations
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "models", "phi3-mini.onnx"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SupportAssistant", "models", "phi3-mini.onnx"),
            @"C:\AI\Models\phi3-mini.onnx"
        };

        return possiblePaths.FirstOrDefault(File.Exists) ?? string.Empty;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _session?.Dispose();
            _sessionOptions?.Dispose();
            _disposed = true;
            _logger.LogInformation("ONNX AI Service disposed");
        }
    }
}
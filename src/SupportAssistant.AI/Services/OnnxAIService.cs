using Microsoft.ML.OnnxRuntime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration? _configuration;
    private InferenceSession? _session;
    private SessionOptions? _sessionOptions;
    private bool _isInitialized = false;
    private bool _disposed = false;
    private bool _useGpuAcceleration = true;

    /// <summary>
    /// Initializes a new instance of the OnnxAIService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration instance (optional)</param>
    public OnnxAIService(ILogger<OnnxAIService> logger, IConfiguration? configuration = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration;

        // Check configuration for GPU acceleration preference
        if (_configuration != null)
        {
            _useGpuAcceleration = _configuration.GetSection("AI")["UseGpuAcceleration"] != "false";
        }
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

            // Create session options
            _sessionOptions = new SessionOptions();

            // Configure execution providers based on preferences and availability
            if (_useGpuAcceleration)
            {
                if (TryEnableDirectML())
                {
                    _logger.LogInformation("DirectML execution provider enabled successfully");
                }
                else
                {
                    _logger.LogInformation("DirectML not available, using CPU execution provider");
                }
            }
            else
            {
                _logger.LogInformation("GPU acceleration disabled by configuration, using CPU execution provider");
            }

            // Always add CPU provider as fallback
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
        // Check configuration first
        var configuredPath = _configuration?.GetSection("AI")["ModelPath"];
        if (!string.IsNullOrEmpty(configuredPath) && File.Exists(configuredPath))
        {
            _logger.LogInformation("Using configured model path: {ModelPath}", configuredPath);
            return configuredPath;
        }

        // Check common locations for Phi-3-mini model
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "models", "phi3-mini.onnx"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "phi3-mini.onnx"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SupportAssistant", "models", "phi3-mini.onnx"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub", "phi3-mini.onnx"),
            @"C:\AI\Models\phi3-mini.onnx",
            @"C:\Models\phi3-mini.onnx"
        };

        var foundPath = possiblePaths.FirstOrDefault(File.Exists);
        if (!string.IsNullOrEmpty(foundPath))
        {
            _logger.LogInformation("Found model at: {ModelPath}", foundPath);
        }
        else
        {
            _logger.LogWarning("Model not found in any of the expected locations. Checked paths: {Paths}",
                string.Join(", ", possiblePaths));
        }

        return foundPath ?? string.Empty;
    }

    private bool TryEnableDirectML()
    {
        try
        {
            _sessionOptions?.AppendExecutionProvider_DML(0);
            return true;
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning("DirectML entry point not found. This may indicate an incompatible ONNX Runtime version or missing DirectML support. Error: {Error}", ex.Message);
            return false;
        }
        catch (DllNotFoundException ex)
        {
            _logger.LogWarning("DirectML DLL not found. DirectML may not be installed or supported on this system. Error: {Error}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enable DirectML provider: {Error}", ex.Message);
            return false;
        }
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
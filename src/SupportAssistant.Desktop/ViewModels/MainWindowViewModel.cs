using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SupportAssistant.Core.Interfaces;
using SupportAssistant.Core.Models;
using System.Collections.ObjectModel;

namespace SupportAssistant.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAIService _aiService;
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string _greeting = "Welcome to SupportAssistant!";
    
    [ObservableProperty]
    private string _userInput = string.Empty;
    
    [ObservableProperty]
    private bool _isProcessing;
    
    [ObservableProperty]
    private string _statusMessage = "Ready";

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public MainWindowViewModel(IAIService aiService, IKnowledgeBaseService knowledgeBaseService, ILogger<MainWindowViewModel> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _knowledgeBaseService = knowledgeBaseService ?? throw new ArgumentNullException(nameof(knowledgeBaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Add welcome message
        Messages.Add(new ChatMessage
        {
            Content = "Hello! I'm your SupportAssistant. I can help you with technical issues and system troubleshooting. What can I help you with today?",
            Sender = MessageSender.Assistant
        });
    }

    /// <summary>
    /// Initialize the services asynchronously. Call this method after the ViewModel is constructed.
    /// </summary>
    public async Task InitializeAsync()
    {
        await InitializeServicesAsync();
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsProcessing)
            return;

        var userMessage = UserInput.Trim();
        UserInput = string.Empty;
        IsProcessing = true;
        StatusMessage = "Processing your question...";

        try
        {
            // Add user message to chat
            Messages.Add(new ChatMessage
            {
                Content = userMessage,
                Sender = MessageSender.User
            });

            // Create AI query
            var query = new AIQuery
            {
                Text = userMessage,
                Context = Messages.TakeLast(5).ToList() // Include recent context
            };

            // Get AI response
            var response = await _aiService.GenerateResponseAsync(query);

            // Add AI response to chat
            Messages.Add(new ChatMessage
            {
                Content = response.Text,
                Sender = MessageSender.Assistant,
                Metadata = new Dictionary<string, object>
                {
                    ["Confidence"] = response.Confidence,
                    ["GenerationTime"] = response.Metadata.GenerationTime.TotalMilliseconds,
                    ["UsedRAG"] = response.Metadata.UsedRAG
                }
            });

            _logger.LogInformation("Processed user query: {Query}", userMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user message");
            Messages.Add(new ChatMessage
            {
                Content = "I apologize, but I encountered an error while processing your request. Please try again.",
                Sender = MessageSender.System
            });
        }
        finally
        {
            IsProcessing = false;
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    private void ClearMessages()
    {
        Messages.Clear();
        Messages.Add(new ChatMessage
        {
            Content = "Chat cleared. How can I help you?",
            Sender = MessageSender.Assistant
        });
        _logger.LogInformation("Chat messages cleared");
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            StatusMessage = "Initializing AI services...";
            
            // Initialize knowledge base first
            if (!_knowledgeBaseService.IsReady)
            {
                await _knowledgeBaseService.InitializeAsync();
                _logger.LogInformation("Knowledge base service initialized");
            }

            // Initialize AI service
            if (!_aiService.IsReady)
            {
                await _aiService.InitializeAsync();
                _logger.LogInformation("AI service initialized");
            }

            StatusMessage = "Ready";
            
            // Update greeting based on service status
            if (_aiService.IsReady && _knowledgeBaseService.IsReady)
            {
                Greeting = "Welcome to SupportAssistant! All services are ready.";
            }
            else
            {
                Greeting = "Welcome to SupportAssistant! Running in limited mode.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize services");
            StatusMessage = "Service initialization failed";
            Greeting = "Welcome to SupportAssistant! Some services may be unavailable.";
        }
    }
}
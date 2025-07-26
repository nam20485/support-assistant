using CommunityToolkit.Mvvm.ComponentModel;

namespace SupportAssistant.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _greeting = "Welcome to SupportAssistant!";
    
    [ObservableProperty]
    private string _userInput = string.Empty;
    
    [ObservableProperty]
    private string _chatHistory = "SupportAssistant: Hello! How can I help you today?";
}
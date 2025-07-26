# SupportAssistant

A comprehensive open-source Windows desktop application providing intelligent technical assistance using local AI (Small Language Model) with Retrieval-Augmented Generation (RAG) architecture.

## Overview

SupportAssistant is a privacy-first desktop application that operates entirely on-device to provide intelligent technical assistance. The application uses a local AI model with a curated knowledge base to answer user questions without transmitting any data to external services.

## Key Features

- **Local AI Chat Interface**: Natural language queries with context-aware responses
- **RAG Knowledge Base**: Curated technical documentation from Microsoft Learn and Stack Overflow
- **Agentic Capabilities**: System modification tools with human-in-the-loop approval
- **Privacy-First**: All processing on-device, no data transmission to external services
- **Offline Operation**: Complete functionality without internet connection
- **Cross-Platform UI**: Avalonia for future macOS/Linux compatibility

## Technology Stack

- **Language**: C# .NET 8.0
- **UI Framework**: Avalonia UI with MVVM pattern
- **AI Model**: Microsoft Phi-3-mini ONNX (MIT license)
- **AI Runtime**: ONNX Runtime with DirectML for GPU acceleration
- **Architecture**: RAG (Retrieval-Augmented Generation) with local vector database
- **Testing**: xUnit, FluentAssertions, Moq with 80%+ coverage target
- **Logging**: Serilog with structured logging
- **Additional**: Docker support, comprehensive documentation

## Project Structure

```
SupportAssistant/
├── src/
│   ├── SupportAssistant.Core/          # Core models and interfaces
│   ├── SupportAssistant.AI/            # ONNX Runtime, RAG, inference
│   ├── SupportAssistant.KnowledgeBase/ # Vector DB, content processing
│   ├── SupportAssistant.Tools/         # Agentic system tools
│   ├── SupportAssistant.Desktop/       # Avalonia UI application
│   └── SupportAssistant.Shared/        # Shared utilities
├── tests/                              # Comprehensive test suite
├── docs/                               # Documentation and guides
├── scripts/                            # Build and setup scripts
├── docker/                             # Containerization support
├── assets/                             # AI models and knowledge base
└── global.json                         # .NET SDK version specification
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11 (primary target)
- Visual Studio 2022 or VS Code (recommended)

### Building

1. Clone the repository:
   ```bash
   git clone https://github.com/nam20485/support-assistant.git
   cd support-assistant
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

5. Run the desktop application:
   ```bash
   dotnet run --project src/SupportAssistant.Desktop
   ```

### Development

The project follows modern .NET development practices:

- **MVVM Pattern**: Used throughout the Avalonia UI application
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Structured Logging**: Serilog with console and file outputs
- **Comprehensive Testing**: xUnit with mocking and assertions
- **Documentation**: XML documentation for all public APIs

## Implementation Status

This project is currently in active development. The current phase includes:

### ✅ Phase 1: Core Setup & Knowledge Base Preparation
- [x] Create .NET solution structure with all required projects
- [x] Set up Avalonia MVVM project template
- [x] Configure project references and core dependencies
- [x] Establish proper Git repository structure
- [x] Create comprehensive README.md with project overview
- [x] Add core models and interfaces
- [x] Set up basic UI structure

### 🚧 Phase 1.2: Core Dependencies Integration (In Progress)
- [ ] Add Microsoft.ML.OnnxRuntime.DirectML NuGet package
- [ ] Add remaining core dependencies
- [ ] Configure warnings as errors and XML documentation
- [ ] Complete Serilog setup

### 📋 Upcoming Phases
- **Phase 1.3**: ONNX Runtime Foundation
- **Phase 1.4**: Knowledge Base Infrastructure
- **Phase 1.5**: Content Processing Pipeline
- **Phase 2**: AI Service & RAG Implementation
- **Phase 3**: UI/UX & Application Integration
- **Phase 4**: Tooling & Agentic Capabilities

## Contributing

This is an open-source project welcoming contributions. Please see the documentation for contribution guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Architecture

The application follows a modular architecture with clear separation of concerns:

- **Core**: Contains shared models, interfaces, and fundamental types
- **AI**: Handles ONNX Runtime integration and inference
- **KnowledgeBase**: Manages the RAG knowledge base and vector search
- **Tools**: Implements agentic capabilities for system modifications
- **Desktop**: Avalonia UI application with MVVM pattern
- **Shared**: Common utilities and helpers

## Security

- All AI inference happens locally on the user's device
- No data is transmitted to external services
- System modifications require explicit user approval
- Comprehensive audit logging for all tool executions
- Principle of least privilege enforcement

## Performance

- Hardware-optimized AI inference with DirectML GPU acceleration
- CPU fallback for systems without compatible GPUs
- Efficient vector search for knowledge base queries
- Streaming responses for better user experience
- Configurable model parameters for performance tuning
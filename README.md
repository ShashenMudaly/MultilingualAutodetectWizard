# Multilingual Autodetect Wizard API

## Architecture Overview

This .NET Core Web API provides language detection and translation services using Azure Translator API. The application follows a clean architecture pattern with separation of concerns.

### Project Structure 

MAWTranslatorService/
├── Controllers/
│ └── TranslatorController.cs # API endpoints for translation services
├── Services/
│ ├── TranslatorService.cs # Core translation service implementation
│ ├── ITranslatorService.cs # Service interface
│ └── Models/
│ └── TranslatorModels.cs # Data models for the service
├── Program.cs # Application configuration and startup
└── appsettings.json # Configuration settings
```

### Key Components

1. **API Controllers**
   - `TranslatorController`: Handles HTTP requests for language detection and translation
   - Endpoints:
     - POST `/api/translator/detect`: Detects language of provided text
     - POST `/api/translator/translate`: Translates text to target language

2. **Services Layer**
   - `ITranslatorService`: Defines the contract for translation operations
   - `TranslatorService`: Implements the translation service using Azure Translator API
   - Handles:
     - Language detection
     - Text translation
     - Error handling and logging

3. **Models**
   - `DetectedLanguage`: Language detection result
   - `TranslationResult`: Translation operation result
   - `TranslatorConfig`: Azure Translator API configuration

### Configuration

The application requires Azure Translator API credentials configured in `appsettings.json`:

```json
{
  "AzureTranslator": {
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "ApiKey": "your-api-key",
    "Region": "your-region"
  }
}
```

### Dependencies

- .NET 7.0+
- Azure Translator API subscription
- HttpClient for API communication
- System.Text.Json for JSON handling

### Security

- CORS policy configured for NextJS frontend (localhost:3000)
- API key and region validation
- Request/response logging with sensitive data protection

### Error Handling

The service implements comprehensive error handling:
- Input validation
- HTTP request/response validation
- Azure API error handling
- Detailed logging for troubleshooting

### Logging

Implements structured logging using ILogger:
- Request/response details
- Error information
- Performance metrics
- Configuration validation

## Getting Started

1. Clone the repository
2. Update `appsettings.json` with your Azure Translator API credentials
3. Run the application:
   ```bash
   dotnet run
   ```
4. API will be available at `https://localhost:5131`

## API Usage

### Detect Language
```http
POST /api/translator/detect
Content-Type: application/json

"Hello, world!"
```

### Translate Text
```http
POST /api/translator/translate?targetLanguage=es&sourceLanguage=en
Content-Type: application/json

"Hello, world!"
```

## Development

The project uses:
- Dependency Injection for service management
- Interface-based design for testability
- Structured logging for monitoring
- Configuration management for external services
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using MAWTranslatorService.Services;

namespace MAWTranslatorService.Tests;

public class TranslatorServiceTests
{
    private readonly Mock<IOptions<TranslatorConfig>> _mockOptions;
    private readonly Mock<ILogger<TranslatorService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly TranslatorService _service;

    public TranslatorServiceTests()
    {
        _mockOptions = new Mock<IOptions<TranslatorConfig>>();
        _mockOptions.Setup(x => x.Value).Returns(new TranslatorConfig
        {
            Endpoint = "https://api.test.com",
            ApiKey = "test-key",
            Region = "test-region"
        });

        _mockLogger = new Mock<ILogger<TranslatorService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        _service = new TranslatorService(_httpClient, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task DetectLanguageAsync_WithValidText_ReturnsDetectedLanguage()
    {
        // Arrange
        var text = "Hello, world!";
        var expectedResponse = new[]
        {
            new { language = "en", score = 0.95, isTranslationSupported = true, isTransliterationSupported = false }
        };

        SetupMockHttpHandler(JsonSerializer.Serialize(expectedResponse));

        // Act
        var result = await _service.DetectLanguageAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("en", result.Language);
        Assert.Equal(0.95, result.Score);
    }

    [Fact]
    public async Task DetectLanguageAsync_WithEmptyText_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DetectLanguageAsync(string.Empty));
    }

    [Fact]
    public async Task TranslateTextAsync_WithValidInput_ReturnsTranslation()
    {
        // Arrange
        var text = "Hello";
        var targetLanguage = "es";
        var expectedResponse = new[]
        {
            new
            {
                detectedLanguage = new { language = "en", score = 0.95 },
                translations = new[]
                {
                    new { text = "Hola", to = "es" }
                }
            }
        };

        SetupMockHttpHandler(JsonSerializer.Serialize(expectedResponse));

        // Act
        var result = await _service.TranslateTextAsync(text, targetLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hola", result.TranslatedText);
        Assert.Equal("en", result.SourceLanguage);
        Assert.Equal("es", result.TargetLanguage);
    }

    [Fact]
    public async Task TranslateTextAsync_WithEmptyText_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.TranslateTextAsync(string.Empty, "es"));
    }

    [Fact]
    public async Task TranslateTextAsync_WithEmptyTargetLanguage_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.TranslateTextAsync("Hello", string.Empty));
    }

    private void SetupMockHttpHandler(string responseContent)
    {
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });
    }
} 
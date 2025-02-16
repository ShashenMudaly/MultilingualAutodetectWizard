using Microsoft.AspNetCore.Mvc;
using Moq;
using MAWTranslatorService.Services;
using Xunit;

namespace MAWTranslatorService.Tests;

public class TranslatorControllerTests
{
    private readonly Mock<ITranslatorService> _mockService;
    private readonly TranslatorController _controller;

    public TranslatorControllerTests()
    {
        _mockService = new Mock<ITranslatorService>();
        _controller = new TranslatorController(_mockService.Object);
    }

    [Fact]
    public async Task DetectLanguage_WithValidText_ReturnsOkResult()
    {
        // Arrange
        var text = "Hello, world!";
        var expectedResult = new DetectedLanguage("en", 0.95);
        _mockService.Setup(x => x.DetectLanguageAsync(text))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.DetectLanguage(text);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var detectedLanguage = Assert.IsType<DetectedLanguage>(okResult.Value);
        Assert.Equal("en", detectedLanguage.Language);
        Assert.Equal(0.95, detectedLanguage.Score);
    }

    [Fact]
    public async Task Translate_WithValidInput_ReturnsOkResult()
    {
        // Arrange
        var text = "Hello";
        var targetLanguage = "es";
        var expectedResult = new TranslationResult("Hola", "en", "es");
        _mockService.Setup(x => x.TranslateTextAsync(text, targetLanguage, null))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Translate(text, targetLanguage);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var translation = Assert.IsType<TranslationResult>(okResult.Value);
        Assert.Equal("Hola", translation.TranslatedText);
        Assert.Equal("en", translation.SourceLanguage);
        Assert.Equal("es", translation.TargetLanguage);
    }

    [Fact]
    public async Task Translate_WithSourceLanguage_ReturnsOkResult()
    {
        // Arrange
        var text = "Hello";
        var targetLanguage = "es";
        var sourceLanguage = "en";
        var expectedResult = new TranslationResult("Hola", "en", "es");
        _mockService.Setup(x => x.TranslateTextAsync(text, targetLanguage, sourceLanguage))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Translate(text, targetLanguage, sourceLanguage);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var translation = Assert.IsType<TranslationResult>(okResult.Value);
        Assert.Equal("Hola", translation.TranslatedText);
        Assert.Equal("en", translation.SourceLanguage);
        Assert.Equal("es", translation.TargetLanguage);
    }
} 
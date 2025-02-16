using Microsoft.AspNetCore.Mvc;
using MAWTranslatorService.Services;

[ApiController]
[Route("api/[controller]")]
public class TranslatorController : ControllerBase
{
    private readonly ITranslatorService _translatorService;

    public TranslatorController(ITranslatorService translatorService)
    {
        _translatorService = translatorService;
    }

    [HttpPost("detect")]
    public async Task<ActionResult<DetectedLanguage>> DetectLanguage([FromBody] string text)
    {
        var result = await _translatorService.DetectLanguageAsync(text);
        return Ok(result);
    }

    [HttpPost("translate")]
    public async Task<ActionResult<TranslationResult>> Translate(
        [FromBody] string text,
        [FromQuery] string targetLanguage,
        [FromQuery] string? sourceLanguage = null)
    {
        var result = await _translatorService.TranslateTextAsync(text, targetLanguage, sourceLanguage);
        return Ok(result);
    }
} 
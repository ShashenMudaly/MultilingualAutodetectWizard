namespace MAWTranslatorService.Services 
{
    public interface ITranslatorService
    {
        Task<DetectedLanguage> DetectLanguageAsync(string text);
        Task<TranslationResult> TranslateTextAsync(string text, string targetLanguage, string? sourceLanguage = null);
    }
} 
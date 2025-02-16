namespace MAWTranslatorService.Services
{
    public class TranslatorConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public record DetectedLanguage(string Language, double Score);
    public record TranslationResult(string TranslatedText, string SourceLanguage, string TargetLanguage);
} 
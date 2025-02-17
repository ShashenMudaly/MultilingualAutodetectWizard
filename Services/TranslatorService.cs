using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace MAWTranslatorService.Services
{
    public class TranslatorService : ITranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly TranslatorConfig _config;
        private readonly ILogger<TranslatorService> _logger;

        public TranslatorService(HttpClient httpClient, IOptions<TranslatorConfig> config, ILogger<TranslatorService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            
            Console.WriteLine($"TranslatorService initialized with Key: {_config.ApiKey}, Region: {_config.Region}");
        }

        public async Task<DetectedLanguage> DetectLanguageAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be empty", nameof(text));
            }

            try
            {
                var result = await SendRequestAsync<List<DetectResponse>>("/detect?api-version=3.0", 
                    new[] { new { Text = text } });

                var detection = result?.FirstOrDefault();
                
                _logger.LogInformation("Detection response: {@Detection}", detection);
                _logger.LogInformation("Raw result: {@Result}", result);

                return new DetectedLanguage(
                    detection?.Language ?? "unknown",
                    detection?.Score ?? 0.0
                );
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Language detection failed for text: {TextPreview}", 
                    text.Length > 50 ? text[..50] + "..." : text);
                throw;
            }
        }

        public async Task<TranslationResult> TranslateTextAsync(string text, string targetLanguage, string? sourceLanguage = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty", nameof(text));
            if (string.IsNullOrWhiteSpace(targetLanguage))
                throw new ArgumentException("Target language cannot be empty", nameof(targetLanguage));

            try
            {
                var route = $"/translate?api-version=3.0&to={targetLanguage}";
                if (!string.IsNullOrEmpty(sourceLanguage))
                {
                    route += $"&from={sourceLanguage}";
                }

                var result = await SendRequestAsync<List<TranslateResponse>>(route, 
                    new[] { new { Text = text } });

                var translation = result?.FirstOrDefault()?.Translations?.FirstOrDefault();
                return new TranslationResult(
                    translation?.Text ?? text,
                    result?.FirstOrDefault()?.DetectedLanguage?.Language ?? sourceLanguage ?? "unknown",
                    targetLanguage
                );
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Translation failed for text: {TextPreview} to language: {TargetLanguage}", 
                    text.Length > 50 ? text[..50] + "..." : text, targetLanguage);
                throw;
            }
        }

        private async Task<T?> SendRequestAsync<T>(string route, object body)
        {
            int maxRetries = 3;
            int currentRetry = 0;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (currentRetry < maxRetries)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, route);
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _config.ApiKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", _config.Region);
                    
                    var jsonBody = JsonSerializer.Serialize(body);
                    _logger.LogInformation("Full Request URL: {Url}", request.RequestUri);
                    _logger.LogInformation("Request Body: {JsonBody}", jsonBody);
                    _logger.LogInformation("API Key (first 4 chars): {KeyPrefix}", _config.ApiKey?[..Math.Min(4, _config.ApiKey?.Length ?? 0)]);
                    _logger.LogInformation("Region: {Region}", _config.Region);
                    
                    request.Content = new StringContent(
                        jsonBody,
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogInformation("Response Status: {Status}", response.StatusCode);
                    _logger.LogInformation("Response Content: {Content}", content);
                    _logger.LogInformation("Response Headers: {@Headers}", response.Headers);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("API request failed with status {StatusCode}. Error: {Error}", 
                            response.StatusCode, content);
                        
                        throw new HttpRequestException(
                            $"Translation API request failed with status {response.StatusCode}: {content}");
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var result = JsonSerializer.Deserialize<T>(content, options);
                    _logger.LogInformation("Deserialized Result: {@Result}", result);

                    return result;
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry == maxRetries) throw;

                    _logger.LogWarning("Rate limit hit, waiting {Delay} seconds before retry {Retry}/{MaxRetries}", 
                        delay.TotalSeconds, currentRetry, maxRetries);
                    
                    await Task.Delay(delay);
                    delay *= 2; // Exponential backoff
                }
            }

            return default;
        }

        private class DetectResponse
        {
            [JsonPropertyName("language")]
            public string Language { get; set; } = string.Empty;

            [JsonPropertyName("score")]
            public double Score { get; set; }

            [JsonPropertyName("isTranslationSupported")]
            public bool IsTranslationSupported { get; set; }

            [JsonPropertyName("isTransliterationSupported")]
            public bool IsTransliterationSupported { get; set; }
        }

        private record TranslateResponse
        {
            public DetectedLanguage? DetectedLanguage { get; init; }
            public IList<Translation>? Translations { get; init; }
        }

        private record Translation
        {
            public string? Text { get; init; }
            public string? To { get; init; }
        }
    } 
}
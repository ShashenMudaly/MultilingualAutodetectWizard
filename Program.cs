using MAWTranslatorService.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Get the config first
var translatorConfig = builder.Configuration.GetSection("AzureTranslator").Get<TranslatorConfig>();
if (translatorConfig?.Endpoint == null)
{
    throw new InvalidOperationException("Translator endpoint not configured");
}

// Configure the HttpClient with the endpoint from config
builder.Services.AddHttpClient<ITranslatorService, TranslatorService>(client =>
{
    client.BaseAddress = new Uri(translatorConfig.Endpoint);
});

builder.Services.Configure<TranslatorConfig>(
    builder.Configuration.GetSection("AzureTranslator"));

// Debug line
Console.WriteLine($"Configuration loaded - Key: {translatorConfig?.ApiKey}, Region: {translatorConfig?.Region}, Endpoint: {translatorConfig?.Endpoint}");

builder.Services.AddControllers();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS - add this before other middleware
app.UseCors("AllowNextJS");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

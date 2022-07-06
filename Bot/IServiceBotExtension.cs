
using ccas_mgmt_core;
using ccas_mgmt_core.Bot;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using projecthelper.Bot;

public class BotBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public BotBuilder(WebApplicationBuilder app)
    {
        _services = app.Services;
        _configuration = app.Configuration;
        _services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        _services.AddScoped<IBot, Bot>();
        _services.AddScoped<IConversationService, ConversationService>();
        _services.AddScoped<FlowOrchestrator>();

        _services.AddOptions<BotOption>().Configure(options =>
        {
            options.ClientId = _configuration.GetValue<string>("MicrosoftAppId");
            options.ClientSecret = _configuration.GetValue<string>("MicrosoftAppPassword");
        });
    }
    public BotBuilder(IFunctionsHostBuilder app)
    {
        _services = app.Services;
        _configuration = app.GetContext().Configuration;
        _services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        _services.AddScoped<IBot, Bot>();
        _services.AddScoped<IConversationService, ConversationService>();
        _services.AddScoped<FlowOrchestrator>();

        _services.AddOptions<BotOption>().Configure(options =>
        {
            options.ClientId = _configuration.GetValue<string>("MicrosoftAppId");
            options.ClientSecret = _configuration.GetValue<string>("MicrosoftAppPassword");
        });
    }

    public BotBuilder AddTelemetry()
    {
        // Add Application Insights services into service collection
        _services.AddApplicationInsightsTelemetry();
        // Create the telemetry client.
        _services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
        // Add telemetry initializer that will set the correlation context for all telemetry items.
        _services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
        // Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
        _services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
        // Create the telemetry middleware to initialize telemetry gathering
        _services.AddSingleton<TelemetryInitializerMiddleware>();
        // Create the telemetry middleware (used by the telemetry initializer) to track conversation events
        _services.AddSingleton<TelemetryLoggerMiddleware>();

        return this;
    }

    /// <summary>
    /// use configuration _configuration.GetValue<string>("BlobConnectionString"), _configuration.GetValue<string>("BlobStorage")
    /// </summary>
    /// <returns></returns>
    public BotBuilder AddBlobStorage()
    {
        return AddBlobStorage(_configuration.GetValue<string>("BlobConnectionString"), _configuration.GetValue<string>("BlobStorage"));
    }

    public BotBuilder AddBlobStorage(string connectionString, string blobContainerName)
    {
        _services.AddSingleton<IStorage, BlobsStorage>(
            _ => new BlobsStorage(connectionString, blobContainerName));

        _services.AddSingleton<UserState>();
        _services.AddSingleton<ConversationState>(provider =>
            new ConversationState(provider.GetService<IStorage>()!));
        return this;
    }

    public BotBuilder AddMemoryConversationStore()
    {
        _services.AddSingleton<IConversationReferencesStore, ConversationReferencesStore>();
        return this;
    }
    public BotBuilder AddCosmosConversationStore()
    {
        _services.AddSingleton<ICryptoKeyProvider, CryptoKeyProvider>();
        _services.AddSingleton<Crypto>();
        _services.AddScoped<IBotConversationRepository, BotConversationRepository>();
        _services.AddScoped<IConversationReferencesStore, CosmosConversationReferencesStore>();
        return this;
    }
}
public static class IServiceBotExtension
{

    public static BotBuilder RegisterBot(this WebApplicationBuilder app)
    {
        return new BotBuilder(app);
    }
    public static BotBuilder RegisterBot(this IFunctionsHostBuilder app)
    {
        return new BotBuilder(app);
    }

    public static WebApplication UseBot(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost("api/messages",
            async ([FromServices] IBot bot, [FromServices] IBotFrameworkHttpAdapter adapter, HttpRequest request, HttpResponse response) =>
            {
                await adapter.ProcessAsync(request, response, bot);
            });

        return app;
    }


}
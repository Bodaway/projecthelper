using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using postalCrisisV2.Bot;

namespace projecthelper.Bot; 

public class ConversationService : IConversationService
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IConversationReferencesStore _conversationReferences;
    private readonly string _appId;
    private readonly ConversationState _conversationState;
    private readonly IServiceProvider _serviceProvider;
    private readonly FlowOrchestrator _orchestrator;

    public ConversationService(IServiceProvider serviceProvider,
        IBotFrameworkHttpAdapter adapter, ConversationState conversationState,
        IConfiguration configuration, FlowOrchestrator orchestrator,
        IConversationReferencesStore conversationReferences)
    {
        _adapter = adapter;
        _conversationState = conversationState;
        _conversationReferences = conversationReferences;
        _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        _serviceProvider = serviceProvider;
        _orchestrator = orchestrator;
    }

    public async Task StartConversation<Data>(string channelId, FlowStepResult<Data> data, CancellationToken cancellationToken = default) where Data : FlowData
    {
        ConversationReference? conversation = await _conversationReferences.GetConversation(channelId);
        if (conversation == null)
        {
            throw new NullReferenceException("no conversation found");
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversation, async (turnContext, cancellationToken) =>
            {
                await _orchestrator.MapStep(data, turnContext, cancellationToken);
            }, cancellationToken);
        }
    }
}

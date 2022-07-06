using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using projecthelper.Bot;

namespace ccas_mgmt_core.Bot;

public abstract class FlowOrchestrator
{
    private readonly BotOption _botOption;

    public FlowOrchestrator(IOptions<BotOption> botOption)
    {
        _botOption = botOption.Value;
    }

    public async Task<ResourceResponse> MapStep<T>(FlowStepResult<T> currentResult, ITurnContext context,
        CancellationToken cancellationToken) where T : FlowData
    {
        async Task<ResourceResponse> runGenericStep(IMessageActivity messageActivity)
        {
            return await context.SendActivityAsync(messageActivity, cancellationToken);
        }

        async Task<ResourceResponse> updateStep(IMessageActivity messageActivity)
        {
            messageActivity.Id = context.Activity.ReplyToId;
            return await context.UpdateActivityAsync(messageActivity, cancellationToken);
        }

        async Task<ResourceResponse> createNewConversation(string channelId, IMessageActivity messageActivity)
        {
            //var teamsChannelId = context.Activity.TeamsGetChannelId();

            var serviceUrl = context.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(_botOption.ClientId, _botOption.ClientSecret);

            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new { channel = new { id = channelId } },
                Activity = (Activity)messageActivity,
            };

            await ((CloudAdapter)context.Adapter).CreateConversationAsync(
                credentials.MicrosoftAppId,
                channelId,
                serviceUrl,
                credentials.OAuthScope,
                conversationParameters,
                (t, ct) => { return Task.CompletedTask; },
                cancellationToken);
            return null;
        }

        return currentResult switch
        {
            FlowStepResult<T>.SuccessStep<T>(T data) => await runGenericStep(RunStepCodeWithData(data)),
            FlowStepResult<T>.UpdateStep<T>(T data) => await updateStep(RunStepCodeWithData(data)),
            FlowStepResult<T>.ErrorStep(Exception code) => await runGenericStep(RunErrorStepCode(code)),
            FlowStepResult<T>.CreateConversationStep<T>(string channelId, T data) => await createNewConversation(
                channelId, RunStepCodeWithData(data)),
            _ => throw new NotImplementedException()
        };
    }

    public abstract IMessageActivity RunStepCodeWithData<T>(T data);

    public virtual IMessageActivity RunErrorStepCode(Exception error)
    {
        return MessageFactory.Text(error.Message);
    }
}
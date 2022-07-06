using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using projecthelper.Bot;

namespace ccas_mgmt_core.Bot
{
    public class Bot : TeamsActivityHandler
    {
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        protected readonly ILogger _logger;

        private readonly IConversationReferencesStore _conversationReferences;

        public Bot(ConversationState conversationState, UserState userState, ILogger<Bot> logger,
            IConversationReferencesStore conversationReferences)
        {
            _conversationState = conversationState;
            _userState = userState;
            _logger = logger;
            _conversationReferences = conversationReferences;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await _conversationReferences.AddUserConversationReference((turnContext.Activity as Activity)!);
        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(
                            "Welcome to AuthenticationBot. Type anything to get logged in. Type 'logout' to sign-out."),
                        cancellationToken);
                }
            }
        }

        protected override async Task OnInstallationUpdateAddAsync(
            ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            TeamsChannelData channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var channelId = channelData.Channel.Id;

            var conversationReference = turnContext.Activity.GetConversationReference();
            await _conversationReferences.AddOrUpdate(channelId, conversationReference);
        }

        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await base.OnTokenResponseEventAsync(turnContext, cancellationToken);
        }
    }
}
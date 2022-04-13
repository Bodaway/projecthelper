using Microsoft.Bot.Schema;

namespace projecthelper.Bot
{
    public interface IConversationReferencesStore
    {
        Task AddOrUpdate(string key, ConversationReference conversation);
        Task<ConversationReference> GetConversation(string key);
    }
}
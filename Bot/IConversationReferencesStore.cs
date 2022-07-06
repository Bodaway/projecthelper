using Microsoft.Bot.Schema;

namespace projecthelper.Bot
{
    public interface IConversationReferencesStore
    {
        Task AddOrUpdate(string key, ConversationReference conversation);
        Task AddUserConversationReference(Activity activity);
        Task<ConversationReference> GetConversation(string key);
    }
}
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace projecthelper.Bot;

public class ConversationReferencesStore : ConcurrentDictionary<string, ConversationReference>, IConversationReferencesStore
{
    public ConversationReferencesStore()
    {
    }

    private string FixKey(string key)
    {
        return key.Replace(":", "2points").Replace("_", "underline").Replace("@", "arobase").Replace(".", "dots");
    }

    public async Task AddOrUpdate(string key, ConversationReference conversation)
    {
        try
        {
            base.AddOrUpdate(key, conversation, (key, newValue) => conversation);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<ConversationReference> GetConversation(string key)
    {
        return base[key];
    }

    public async Task AddUserConversationReference(Activity activity)
    {
        var conversationReference = activity.GetConversationReference();
        AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
    }

}

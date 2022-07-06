using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using projecthelper.EfCore;
using projecthelper.Result;
using Task = System.Threading.Tasks.Task;

namespace projecthelper.Bot;

public class ConversationStore : BaseClassEntity
{
    public string Key { get; set; }
    public string HashedConversation { get; set; }
}

public interface IBotConversationRepository : IGenericRepository<ConversationStore> { }
public class BotConversationRepository : Repository<ConversationStore>, IBotConversationRepository
{
    public BotConversationRepository(DbContext context, ILogger<Repository<ConversationStore>> logger) : base(context, logger)
    {
    }
}

public class CosmosConversationReferencesStore : IConversationReferencesStore
{
    private readonly IBotConversationRepository _botConversationRepository;
    private readonly Crypto _crypto;

    public CosmosConversationReferencesStore(IBotConversationRepository botConversationRepository, Crypto crypto)
    {
        _botConversationRepository = botConversationRepository;
        _crypto = crypto;
    }

    public async Task AddOrUpdate(string key, ConversationReference conversation)
    {
        try
        {
            var hashed = Convert.ToBase64String(_crypto.EncryptStringToBytes_Aes(JsonConvert.SerializeObject(conversation)));
            var existingR = await _botConversationRepository.GetFirstByComparer(conversation => conversation.Key == key);
            if (existingR.IsOk())
            {
                var existing = existingR.ExtractOkData();
                existing.HashedConversation = hashed;
                existing.UpdatedAt = DateTime.UtcNow;
                await _botConversationRepository.Update(existing);

            }
            else
            {
                await _botConversationRepository.Insert(
                    new ConversationStore { Key = key, HashedConversation = hashed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" });
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task AddUserConversationReference(Activity activity)
    {
        var conversationReference = activity.GetConversationReference();
        await AddOrUpdate(conversationReference.User.Id, conversationReference);
    }

    public async Task<ConversationReference> GetConversation(string key)
    {
        var hashed = (await _botConversationRepository.GetFirstByComparer(conversation => conversation.Key == key)).ExtractOkData().HashedConversation;
        return JsonConvert.DeserializeObject<ConversationReference>(_crypto.DecryptStringFromBytes_Aes(Convert.FromBase64String(hashed)));
    }
}

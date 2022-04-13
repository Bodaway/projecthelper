namespace projecthelper.Bot;

public interface IConversationService
{
    Task StartConversation<Data>(string teamId, FlowStepResult<Data> data, CancellationToken cancellationToken = default) where Data : FlowData;
}

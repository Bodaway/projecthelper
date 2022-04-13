namespace projecthelper.Bot;

public record FlowData(string Actions)
{
    public DateTime CreationDate { get; init; } = DateTime.Now;
}

public abstract record FlowStepResult<T> where T : FlowData
{
    public record SuccessStep<T>(T? Data = default) : FlowStepResult<T> where T : FlowData;

    public record UpdateStep<T>(T? Data = default) : FlowStepResult<T> where T : FlowData;
    public record CreateConversationStep<T>(string channelId, T? Data = default) : FlowStepResult<T> where T : FlowData;

    public record ErrorStep(Exception ex) : FlowStepResult<T>;

    public record EndStep : FlowStepResult<T>;

    public static FlowStepResult<T> SetSuccess<T>(T data) where T : FlowData => new SuccessStep<T>(data);
    public static FlowStepResult<T> SetUpdate<T>(T? data) where T : FlowData => new UpdateStep<T>(data);
    public static FlowStepResult<T> SetError(Exception ex) => new ErrorStep(ex);
    public static FlowStepResult<T> SetEnd() => new EndStep();
    public static FlowStepResult<T> SetCreateConversation<T>(string channelId, T data) where T : FlowData => new CreateConversationStep<T>(channelId, data);
}

public static class FlowStepResultExtensions
{
    public static bool IsOk<T>(this FlowStepResult<T> result) where T : FlowData
    {
        return result switch
        {
            FlowStepResult<T>.SuccessStep<T>(T data) => true,
            FlowStepResult<T>.ErrorStep(Exception error) => false,
            _ => throw new NotImplementedException()
        };
    }
    public static T ExtractOkCodeWithData<T>(this FlowStepResult<T> result) where T : FlowData
    {
        return result switch
        {
            FlowStepResult<T>.SuccessStep<T>(T data) => data,
            _ => throw new NotImplementedException()
        };
    }
    public static Exception ExtractErrorCode<T>(this FlowStepResult<T> result) where T : FlowData
    {
        return result switch
        {
            FlowStepResult<T>.ErrorStep(Exception code) => code,
            _ => throw new NotImplementedException()
        };
    }
}

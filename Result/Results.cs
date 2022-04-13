using Microsoft.Extensions.Logging;

namespace projecthelper.Result;

/// <summary>
/// Class result for follow if operation is succes or fail
/// </summary>
/// <typeparam name="TSuccess">type of result if succes, if fail is Exception</typeparam>
public abstract record Result<TSuccess>
{
    /// <summary>
    /// Class for result is Ok
    /// </summary>
    public record Ok(TSuccess Data) : Result<TSuccess>;

    /// <summary>
    /// class for fail result
    /// </summary>
    public record Fail(Exception Error) : Result<TSuccess>;

    /// <summary>
    /// define a result OK
    /// </summary>
    public static Result<TSuccess> setOk(TSuccess data) => new Ok(data);

    /// <summary>
    /// Define a result fail
    /// </summary>
    public static Result<TSuccess> setFail(Exception error, ILogger logger)
    {
        logger.LogError(error.Message, error);
        return new Fail(error);
    }
}

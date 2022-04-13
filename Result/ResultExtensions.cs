using Microsoft.Extensions.Logging;

namespace projecthelper.Result;

/// <summary>
/// class extension for result
/// </summary>
public static class ResultExtensions
{

    /// <summary>
    /// define a result ok with data inside
    /// </summary>
    /// <param name="data">the result</param>
    /// <returns>a result Ok</returns>
    public static Result<T> isResultOk<T>(this T data)
    {
        return Result<T>.setOk(data);
    }

    /// <summary>
    /// define a result ok with data inside
    /// </summary>
    /// <param name="data">the result</param>
    /// <returns>a result Ok</returns>
    public static Result<T> isResultFail<T>(this Exception data, ILogger logger)
    {
        return Result<T>.setFail(data,logger);
    }

    /// <summary>
    /// execute functions on data inside result
    /// </summary>
    /// <param name="currentResult">the base result</param>
    /// <param name="okMap">function that execute if result is Ok</param>
    /// <param name="FailMap">function that execute if result is fail</param>
    /// <returns>a new Result that after the execution of function</returns>
    public static Result<U> Map<T, U>(this Result<T> currentResult, Func<T, Result<U>> okMap, Func<Exception, Result<U>> FailMap)
    {
        return currentResult switch
        {
            Result<T>.Ok(T data) => okMap(data),
            Result<T>.Fail(Exception ex) => FailMap(ex),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// execute functions on data if sucess only
    /// </summary>
    /// <param name="currentResult">the base result</param>
    /// <param name="okMap">function that execute if result is Ok</param>
    /// <returns>a new Result that after the execution of function</returns>
    public static Result<U> Map<T, U>(this Result<T> currentResult, Func<T, Result<U>> okMap, ILogger logger)
    {
        return currentResult.Map(okMap, (ex) => Result<U>.setFail(ex, logger));
    }

    /// <summary>
    /// define if result is ok, for procedural flow
    /// </summary>
    /// <param name="result">the result</param>
    /// <returns>true if result is Ok</returns>
    public static bool IsOk<T>(this Result<T> result)
    {
        return result switch
        {
            Result<T>.Ok(T data) => true,
            Result<T>.Fail(Exception ex) => false,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// force extraction of sucess data, throw an exception if not
    /// </summary>
    /// <param name="result">the result</param>
    /// <returns>the data inside Ok result</returns>
    public static T ExtractOkData<T>(this Result<T> result)
    {
        return result switch
        {
            Result<T>.Ok(T data) => data,
            Result<T>.Fail(Exception ex) => throw ex,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// force extraction of fail data, throw an exception if not
    /// </summary>
    /// <param name="result">the result</param>
    /// <returns>the data inside Ok result</returns>
    public static Exception ExtractFailData<T>(this Result<T> result)
    {
        return result switch
        {
            Result<T>.Fail(Exception ex) => ex,
            _ => throw new NotImplementedException()
        };
    }

    public static Result<T> ThrowIfFail<T>(this Result<T> result)
    {
        return result switch
        {
            Result<T>.Ok(T) => result,
            Result<T>.Fail(Exception ex) => throw ex,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// execute functions on data inside result
    /// </summary>
    /// <param name="currentResult">the base result</param>
    /// <param name="okMap">function that execute if result is Ok</param>
    /// <param name="FailMap">function that execute if result is fail</param>
    /// <returns>a new Result that after the execution of function</returns>
    public static async Task<Result<U>> AsyncMap<T, U>(this Result<T> currentResult, Func<T, Task<Result<U>>> okMap, Func<Exception, Task<Result<U>>> FailMap)
    {
        return currentResult switch
        {
            Result<T>.Ok(T data) => await okMap(data),
            Result<T>.Fail(Exception ex) => await FailMap(ex),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// execute functions on data inside result
    /// </summary>
    /// <param name="currentResult">the base result</param>
    /// <param name="okMap">function that execute if result is Ok</param>
    /// <returns>a new Result that after the execution of function</returns>
    public static async Task<Result<U>> AsyncMap<T, U>(this Result<T> currentResult, Func<T, Task<Result<U>>> okMap, ILogger logger)
    {
        return currentResult switch
        {
            Result<T>.Ok(T data) => await okMap(data),
            Result<T>.Fail(Exception ex) => Result<U>.setFail(ex, logger),
            _ => throw new NotImplementedException()
        };
    }
}


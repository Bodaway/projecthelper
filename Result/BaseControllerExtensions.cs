using Microsoft.AspNetCore.Mvc;

namespace projecthelper.Result;

/// <summary>
/// extension for controller
/// </summary>
public static class BaseControllerExtensions
{
    /// <summary>
    /// return a response OK or 500 depends of result state
    /// </summary>
    public static IActionResult SetHttpResponseFromResult<T>(this ControllerBase controller, Result<T> result)
    {
        return result switch
        {
            Result<T>.Ok(T data) => controller.Ok(data),
            Result<T>.Fail(Exception ex) =>
                ex switch
                {
                    HttpResultException httpException => controller.StatusCode((int)httpException.Code, httpException.Message),
                    _ => controller.StatusCode(500, ex)
                },
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// execute a function for create a response depending of result state
    /// </summary>
    public static IActionResult SetHttpResponseFromResult<T>(this ControllerBase controller, Result<T> result,
        Func<T, IActionResult> OnSuccess,
        Func<Exception, IActionResult> OnFail)
    {
        return result switch
        {
            Result<T>.Ok(T data) => OnSuccess(data),
            Result<T>.Fail(Exception ex) => OnFail(ex),
            _ => throw new NotImplementedException()
        };
    }
}

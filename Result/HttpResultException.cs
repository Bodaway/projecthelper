using System.Net;

namespace projecthelper.Result;

/// <summary>
/// used in a Result flow for follow http error
/// </summary>
public class HttpResultException : Exception
{
    /// <summary>
    /// the body data of response in error
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// the http code of response in error
    /// </summary>
    public HttpStatusCode Code { get; }

    /// <summary>
    /// base ctor
    /// </summary>
    /// <param name="code">http code</param>
    /// <param name="details">detail of the error</param>
    public HttpResultException(HttpStatusCode code, string details) : base(details)
    {
        Code = code;
        Details = details;
    }
}

/// <summary>
/// extension for HttpResultException
/// </summary>
public static class HttpResultExceptionExtension
{
    /// <summary>
    /// used a httpResponseMessage an extract an exception
    /// NOT CONTROL IF MESSAGE IS VALID
    /// </summary>
    /// <param name="response">the base response message</param>
    /// <returns>the extracted exception</returns>
    public static async Task<HttpResultException> GetException(this HttpResponseMessage response)
    {
        return new HttpResultException(response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}

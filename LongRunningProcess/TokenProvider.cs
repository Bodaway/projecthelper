using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace projecthelper.LongRunningProcess;

public interface ITokenProvider
{
    Task<TokenData> AcquireAccessTokenFromRefresh(string IdToken);
    Task<TokenData> AcquireToken(string IdToken);
}

public class TokenProvider : ITokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TokenProvider> _logger;
    private readonly TokenCacheOptions _options;

    public TokenProvider(IHttpClientFactory httpClientFactory, IOptions<TokenCacheOptions> options, ILogger<TokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<TokenData> AcquireToken(string IdToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("token");
            client.BaseAddress = new Uri("https://login.microsoftonline.com/");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.TenantId}/oauth2/v2.0/token");

            request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
            new KeyValuePair<string, string>("grant_type","urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("client_id",_options.ClientId),
            new KeyValuePair<string, string>("client_secret",_options.ClientSecret),
            new KeyValuePair<string, string>("assertion",IdToken),
            new KeyValuePair<string, string>("scope",_options.Scopes.Aggregate((a, b) => a + " " + b)),
            new KeyValuePair<string, string>("requested_token_use","on_behalf_of")
            });
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var contentstr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TokenData>(contentstr);

            if (result.RefreshToken == null)
            {
                _logger.LogError($"refresh token is null", contentstr);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<TokenData> AcquireAccessTokenFromRefresh(string IdToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("refresh_token");
            client.BaseAddress = new Uri("https://login.microsoftonline.com/");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.TenantId}/oauth2/v2.0/token");

            request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
            new KeyValuePair<string, string>("grant_type","refresh_token"),
            new KeyValuePair<string, string>("client_id",_options.ClientId),
            new KeyValuePair<string, string>("client_secret",_options.ClientSecret),
            new KeyValuePair<string, string>("refresh_token",IdToken),
            });
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var contentstr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TokenData>(contentstr);

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}

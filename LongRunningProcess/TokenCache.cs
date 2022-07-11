using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using projecthelper.Result;

namespace projecthelper.LongRunningProcess;

public class CustomTokenCache
{
    private readonly ILogger<CustomTokenCache> _logger;
    private readonly ITokenProvider _provider;
    private readonly Crypto _crypto;
    private readonly ITokenCacheRepository _tokenCacheRepository;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TokenCacheOptions _options;

    public CustomTokenCache(
        ITokenProvider tokenProvider,
        Crypto crypto,
        ITokenCacheRepository tokenCacheRepository,
        IHttpContextAccessor contextAccessor,
        IOptions<TokenCacheOptions> options,
        ILogger<CustomTokenCache> logger
        )
    {
        _provider = tokenProvider;
        _logger = logger;
        _crypto = crypto;
        _tokenCacheRepository = tokenCacheRepository;
        _contextAccessor = contextAccessor;
        _options = options.Value;
    }

    public async Task<string> GetAccessToken(ClaimsPrincipal principal)
    {
            var httpContext = _contextAccessor.HttpContext;
        try
        {
            var idToken = httpContext.Items["JwtSecurityTokenUsedToCallWebAPI"] as JwtSecurityToken;
            var tokenResult = await _provider.AcquireToken(idToken!.RawData);

            var encryptedTokenCache = new EncryptedToken
            {
                Key = FormatKey(principal.GetObjectId(), _options.TenantId),
                Token = _crypto.EncryptStringToBytes_Aes(tokenResult.RefreshToken),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = principal.GetObjectId(),
            };

            await _tokenCacheRepository.Delete(t => t.Key == FormatKey(principal.GetObjectId(), _options.TenantId));
            await _tokenCacheRepository.Insert(encryptedTokenCache);

            return tokenResult.AccessToken;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while getting access token, {0}", principal.GetObjectId(),httpContext.Items["JwtSecurityTokenUsedToCallWebAPI"] as JwtSecurityToken);
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    public async Task<string> GetAccessTokenFromRefresh(string userAadId, string tenantId)
    {
        _logger.LogInformation("Getting token data from cache");

        var tokenKey = FormatKey(userAadId, tenantId);
        var refreshTokenR = await _tokenCacheRepository.GetFirstByComparer(t => t.Key == tokenKey);
        var refreshToken = refreshTokenR.ExtractOkData();
        var tokenResult = await _provider.AcquireAccessTokenFromRefresh(_crypto.DecryptStringFromBytes_Aes(refreshToken.Token));

        var encryptedTokenCache = new EncryptedToken
        {
            Key = tokenKey,
            Token = _crypto.EncryptStringToBytes_Aes(tokenResult.RefreshToken),
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userAadId,
        };

        refreshToken = null;
        refreshTokenR = null;
        await _tokenCacheRepository.Delete(t => t.Key == FormatKey(userAadId, tenantId));
        await _tokenCacheRepository.Insert(encryptedTokenCache);

        return tokenResult.AccessToken;
    }

    private string FormatKey(string userAadId, string tenantId)
    {
        return $"{userAadId}.{tenantId}";
    }
}


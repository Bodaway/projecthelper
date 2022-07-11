using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using projecthelper.EfCore;

namespace projecthelper.LongRunningProcess;

public class TokenCacheOptions
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TenantId { get; set; }
    public IEnumerable<string> Scopes { get; set; }
}
public class TokenData
{
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }

    [JsonProperty("ext_expires_in")]
    public long ExtExpiresIn { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("id_token")]
    public string IdToken { get; set; }
}


public class EncryptedToken : BaseClassEntity
{
    public string Key { get; set; }
    public byte[] Token { get; set; }
    public int Version { get; set; }

}

public interface ITokenCacheRepository : IGenericRepository<EncryptedToken> { };

public class TokenCacheRepository : Repository<EncryptedToken>, ITokenCacheRepository
{
    public TokenCacheRepository(DatabaseDbContext context, ILogger<TokenCacheRepository> logger) : base(context, logger)
    {
    }
}








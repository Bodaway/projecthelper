using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace projecthelper.LongRunningProcess;

public static class IServiceCollectionCustomTokenCache
{
    public static WebApplication InsertEndPoint(this WebApplication app)
    {
        app.MapGet("api/v1/StoreToken", async ([FromServices] ITokenAcquisition tokenAcquisition, [FromServices] CustomTokenCache tokenCache, HttpContext context, ClaimsPrincipal principal) =>
        {
            var token = await tokenCache.GetAccessToken(principal);
        });
        return app;
    }

    public static IServiceCollection AddCustomTokenCache(this IServiceCollection services, TokenCacheOptions runningParameters)
    {
        services.AddOptions<TokenCacheOptions>().Configure(opt =>
        {
            opt.ClientId = runningParameters.ClientId;
            opt.ClientSecret = runningParameters.ClientSecret;
            opt.TenantId = runningParameters.TenantId;
            opt.Scopes = runningParameters.Scopes;
        });
        services.AddScoped<ICryptoKeyProvider,CryptoKeyProvider>();
        services.AddScoped<Crypto>();
        services.AddSingleton<ITokenProvider,TokenProvider>();
        services.AddScoped<ITokenCacheRepository,TokenCacheRepository>();
        services.AddScoped<CustomTokenCache>();
        return services;
    }

}
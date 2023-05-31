using WebAPI.Services.Authentication.Jwt;

namespace WebAPI.Extensions
{
    internal static class JwtTokenGenerator
    {
        internal static IServiceCollection AddJwtTokenGenerator(this IServiceCollection services, IConfiguration config)
        {
            string key = config["JwtSettings:Key"] ?? throw new NullReferenceException("Specify JwtSettings:Key in appsetings (optionally usersecrets)");
            string issuer = config["JwtSettings:Issuer"] ?? throw new NullReferenceException("Specify JwtSettings:Issuer in appsetings (optionally usersecrets)");
            string audience = config["JwtSettings:Audience"] ?? throw new NullReferenceException("Specify JwtSettings:Audience in appsetings (optionally usersecrets)");
            JwtSettings settings = new JwtSettings(key, issuer, audience);
            services.AddSingleton(new JwtTokenManager(settings));
            return services;
        }
    }
}

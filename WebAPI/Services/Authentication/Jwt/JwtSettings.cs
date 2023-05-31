namespace WebAPI.Services.Authentication.Jwt
{
    public class JwtSettings
    {
        public string Key { get; private set; }
        public string Issuer { get; private set; }
        public string Audience { get; private set; }
        public JwtSettings(string key, string issuer, string audience)
        {
            Key = key;
            Issuer = issuer;
            Audience = audience;
        }
    }
}

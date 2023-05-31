namespace WebAPI.Services.Authentication.Jwt
{
    public class UserClaims
    {
        public Guid Guid { get; private set; }
        public string Login { get; private set; }
        public bool IsAdmin { get; private set; }
        public UserClaims(Guid guid, string login, bool isAdmin)
        {
            Guid = guid;
            Login = login;
            IsAdmin = isAdmin;
        }
    }
}

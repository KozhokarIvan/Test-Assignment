using WebAPI.Database.Models;
using WebAPI.Exceptions;
using WebAPI.Repositories;
using WebAPI.Services.Authentication.Jwt;

namespace WebAPI.Services.Users
{
    public class LoginService
    {
        private readonly IUserRepository _userRepository;
        public LoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserClaims?> TryLoginUser(string login, string password)
        {
            User? dbUser = await _userRepository.GetUserByLoginAsync(login);
            if (dbUser?.RevokedOn is not null)
                throw new AccountDeactivatedException();
            UserClaims? user = dbUser is null ? null : new UserClaims(dbUser.Guid, dbUser.Login, dbUser.Admin);
            return user;
        }
    }
    public enum UserLoginResults
    {
        Success,
        Fail,
        NotFound
    }
}

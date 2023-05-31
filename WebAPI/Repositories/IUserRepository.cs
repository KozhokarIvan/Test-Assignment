using WebAPI.Contracts.Requests;
using WebAPI.Database.Models;

namespace WebAPI.Repositories
{
    public interface IUserRepository
    {
        public Task<User> CreateUserAsync(RegisterRequest requestUser, string createdBy);
        public Task<User?> GetUserByLoginAsync(string login);
        public Task<List<User>> GetActiveUsersAsync();
        public Task<List<User>> GetAllUsersAboveAgeAsync(int age);
        public Task<User?> UpdateUserAsync(UpdateUserRequest requestUser, string login, string modifiedBy);
        public Task<User?> ChangePasswordAsync(string login, string newPassword, string modifiedBy);
        public Task<User?> ChangeLoginAsync(string login, string newLogin, string modifiedBy);
        public Task<bool> DeactivateUserAsync(string login, string revokedBy);
        public Task<bool> DeleteUserAsync(string login);
        public Task<User?> RestoreUserAsync(string login);
    }
}

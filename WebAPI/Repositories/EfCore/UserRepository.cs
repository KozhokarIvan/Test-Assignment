using Microsoft.EntityFrameworkCore;
using WebAPI.Contracts.Requests;
using WebAPI.Database;
using WebAPI.Database.Models;
using WebAPI.Exceptions;

namespace WebAPI.Repositories.EfCore
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;
        public UserRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUserAsync(RegisterRequest requestUser, string createdBy)
        {
            bool loginAlreadyExists = await _context.Users.AnyAsync(u => u.Login == requestUser.Login);
            if (loginAlreadyExists)
                throw new LoginAlreadyExistsException();
            User user = new User()
            {
                Guid = Guid.NewGuid(),
                Login = requestUser.Login,
                Password = requestUser.Password,
                Gender = requestUser.Gender,
                Birthday = requestUser.Birthday,
                Name = requestUser.Name,
                Admin = requestUser.Admin,
                CreatedOn = DateTime.Now,
                CreatedBy = createdBy
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            List<User> users = await _context.Users
                .AsNoTracking()
                .Where(u => u.RevokedOn == null)
                .OrderBy(u => u.CreatedOn)
                .ToListAsync() ?? Enumerable.Empty<User>().ToList();
            return users;
        }

        public async Task<User?> UpdateUserAsync(UpdateUserRequest requestUser, string login, string modifiedBy)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return null;
            if (user.RevokedOn is not null && modifiedBy == login)
                throw new AccountDeactivatedException();
            user.Name = requestUser.Name;
            user.Gender = requestUser.Gender;
            user.Birthday = requestUser.Birthday;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            User? user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Login == login);
            return user;
        }

        public async Task<List<User>> GetAllUsersAboveAgeAsync(int age)
        {
            List<User> users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Birthday < DateTime.Now.AddYears(-age))
                .OrderBy(u => u.CreatedOn)
                .ToListAsync();
            return users.Count > 0 ? users : Enumerable.Empty<User>().ToList();
        }

        public async Task<User?> ChangePasswordAsync(string login, string newPassword, string modifiedBy)
        {
            User? user = await _context.Users
                .SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return user;
            if (user.RevokedOn is not null && modifiedBy == login)
                throw new AccountDeactivatedException();
            user.Password = newPassword;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> ChangeLoginAsync(string login, string newLogin, string modifiedBy)
        {
            bool loginAlreadyExists = await _context.Users.AnyAsync(u => u.Login == newLogin);
            if (loginAlreadyExists)
                throw new LoginAlreadyExistsException();
            User? user = await _context.Users
                .SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return null;
            if (user.RevokedOn is not null && modifiedBy == login)
                throw new AccountDeactivatedException();
            user.Login = newLogin;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(string login)
        {
            User? user = await _context.Users
                .SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(string login, string revokedBy)
        {
            User? user = await _context.Users
                .SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return false;
            user.RevokedOn = DateTime.Now;
            user.RevokedBy = revokedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> RestoreUserAsync(string login)
        {
            User? user = await _context.Users
                .SingleOrDefaultAsync(u => u.Login == login);
            if (user is null)
                return user;
            user.RevokedOn = null;
            user.RevokedBy = null;
            await _context.SaveChangesAsync();
            return user;
        }
    }
}

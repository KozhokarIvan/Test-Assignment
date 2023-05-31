using Microsoft.EntityFrameworkCore;
using WebAPI.Constants;
using WebAPI.Database;
using WebAPI.Database.Models;

namespace WebAPI.Extensions
{
    internal static class DbSeederExtension
    {
        private static readonly User adminUser = new User()
        {
            Login = "Admin",
            Password = "StrongPassword2023",
            Name = "Ivan",
            Gender = Genders.Male,
            Birthday = new DateTime(2003, 03, 02),
            Admin = true,
            CreatedBy = "Admin",
            CreatedOn = DateTime.Now
        };
        internal static IApplicationBuilder SeedUsersDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            context.Database.Migrate();
            if (context.Users.SingleOrDefault(u => u.Login == adminUser.Login) is null)
                context.Add(adminUser);
            context.SaveChanges();
            return app;
        }

    }
}

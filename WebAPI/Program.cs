using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Repositories;
using WebAPI.Repositories.EfCore;
using WebAPI.Services.Authentication;
using WebAPI.Services.Authorization;
using WebAPI.Services.Users;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Test Assignment"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Jwt token. Get one in /login endpoint"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Basic"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddJwtTokenGenerator(builder.Configuration);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }
                )
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey
                        (
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]
                            ??
                            throw new NullReferenceException("No security key set in appsetings (optionally usersecrets) under JwtSettings:Key")
                        )),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };
                });

            builder.Services.AddAuthorization(options =>
            options.AddPolicy(PolicyNames.IsAdmin, policy =>
            policy.RequireClaim(CustomClaimNames.IsAdmin, true.ToString()
            )));

            string? connectionString =
                Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString)
                ??
                builder.Configuration["DefaultConnection"];
            if (connectionString is null)
                throw new NullReferenceException(
                    $"Connection string is not set in {EnvironmentVariables.ConnectionString} env variable or appsettings.json (optionally usersecrets)");
            builder.Services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(connectionString));

            var app = builder.Build();


            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            //This is used for Postgres to accept DateTime values
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            app.SeedUsersDatabase();

            app.MapControllers();
            app.Run();

        }
    }
}
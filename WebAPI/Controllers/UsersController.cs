using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts.Requests;
using WebAPI.Contracts.Responses;
using WebAPI.Database.Models;
using WebAPI.Exceptions;
using WebAPI.Repositories;
using WebAPI.Services.Authentication;
using WebAPI.Services.Authentication.Jwt;
using WebAPI.Services.Authorization;
using WebAPI.Services.Users;

namespace WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly JwtTokenManager _jwtAuthManager;
        private readonly IUserRepository _userRepository;
        private readonly LoginService _loginService;
        public UsersController(IUserRepository userRepository, JwtTokenManager jwtAuthManager, LoginService passwordValidator)
        {
            _userRepository = userRepository;
            _jwtAuthManager = jwtAuthManager ?? throw new ArgumentNullException("JwtAuthManager is null");
            _loginService = passwordValidator;
        }
        /// <summary>
        /// Creates a User
        /// </summary>
        /// <returns>Created user</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users
        ///     {
        ///        "login": "SomeCoolLogin",
        ///        "password": "SomeCoolPassword",
        ///        "name": "Ivan",
        ///        "gender": 1,
        ///        "birthday": 2003-03-02T21:53:09.067Z,
        ///        "admin": false,
        ///     }
        ///
        /// </remarks>
        /// <param name="request">User</param>
        /// <response code="201">Returns the newly created User</response>
        /// <response code="400">Returns the invalid model</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="409">User with the login already exists</response>
        /// 
        [Route("/register")]
        [HttpPost]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RegisterRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RegisterRequest), StatusCodes.Status409Conflict)]
        [Authorize(Policy = PolicyNames.IsAdmin)]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            if (!IsLoginValid(request.Login) || !IsPasswordValid(request.Password) || !IsNameValid(request.Name))
                return BadRequest(request);
            string createdBy = User.FindFirst(JwtRegisteredClaimNames.Name)!.Value;
            try
            {
                User dbUser = await _userRepository.CreateUserAsync(request, createdBy);
                RegisterResponse response = new RegisterResponse
                {
                    Guid = dbUser.Guid,
                    Login = dbUser.Login,
                    Name = dbUser.Name,
                    Gender = dbUser.Gender,
                    Birthday = dbUser.Birthday,
                    Admin = dbUser.Admin,
                    CreatedOn = dbUser.CreatedOn,
                    CreatedBy = dbUser.CreatedBy,
                };
                return Created("/", response);

            }
            catch (LoginAlreadyExistsException)
            {
                return Conflict(request);
            }
        }


        /// <summary>
        /// Changes the User's information
        /// </summary>
        /// <returns>Updated user</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /users
        ///     {
        ///        "name": "Ivan",
        ///        "gender": 1,
        ///        "birthday": 2003-03-02T21:53:09.067Z
        ///     }
        ///
        /// </remarks>
        /// <param name="login">Login</param>
        /// <param name="request">User</param>
        /// <response code="200">Returns the updated User</response>
        /// <response code="400">Model is invalid</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// <response code="404">User was not found</response>
        /// 
        [Route("/users/{login}")]
        [HttpPut]
        [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateUserRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UpdateUserRequest), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserAsync(string login, UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            if (!IsNameValid(request.Name))
                return BadRequest(request);
            string modifiedBy = User.FindFirst(JwtRegisteredClaimNames.Name)!.Value;
            bool isAdmin = bool.Parse(User.FindFirst(CustomClaimNames.IsAdmin)!.Value);
            if (!isAdmin && modifiedBy != login)
                return Forbid();
            try
            {
                User? dbUser = await _userRepository.UpdateUserAsync(request, login, modifiedBy);
                if (dbUser is null)
                    return NotFound($"User {login} was not found");
                UpdateUserResponse response = new UpdateUserResponse
                {
                    Guid = dbUser.Guid,
                    Login = dbUser.Login,
                    Name = dbUser.Name,
                    Gender = dbUser.Gender,
                    Birthday = dbUser.Birthday,
                    Admin = dbUser.Admin,
                    CreatedOn = dbUser.CreatedOn,
                    CreatedBy = dbUser.CreatedBy,
                    ModifiedOn = dbUser.ModifiedOn,
                    ModifiedBy = dbUser.ModifiedBy
                };
                return Ok(response);
            }
            catch (AccountDeactivatedException)
            {
                return NotFound(request);
            }
        }

        /// <summary>
        /// Changes the User's password
        /// </summary>
        /// <returns>Message</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users/{login}/changepassword
        ///     {
        ///        "newPassword": "NewCoolPassword"
        ///     }
        ///
        /// </remarks>
        /// <param name="login">Login</param>
        /// <param name="request">New password info</param>
        /// <response code="200">Message</response>
        /// <response code="400">Model is invalid</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// <response code="404">User was not found</response>
        /// 
        [HttpPost]
        [Route("/users/{login}/changepassword")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdatePasswordRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UpdatePasswordRequest), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePasswordAsync(string login, UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            if (!IsPasswordValid(request.NewPassword))
                return BadRequest(request);
            string modifiedBy = User.FindFirst(JwtRegisteredClaimNames.Name)!.Value;
            bool isAdmin = bool.Parse(User.FindFirst(CustomClaimNames.IsAdmin)!.Value);
            if (!isAdmin && modifiedBy != login)
                return Forbid();
            try
            {
                User? dbUser = await _userRepository.ChangePasswordAsync(login, request.NewPassword, modifiedBy);
                if (dbUser is null)
                    return NotFound($"User {login} was not found");
                return Ok("Password was successfully changed");
            }
            catch (AccountDeactivatedException)
            {
                return NotFound(Request);
            }
        }

        /// <summary>
        /// Changes the User's login
        /// </summary>
        /// <returns>Message</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users/{login}/changelogin
        ///     {
        ///        "newLogin": "NewCoolLogin"
        ///     }
        ///
        /// </remarks>
        /// <param name="login">Login</param>
        /// <param name="request">New login info</param>
        /// <response code="200">Message</response>
        /// <response code="400">Model is invalid</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// <response code="404">User was not found</response>
        /// 
        [Route("/users/{login}/changelogin")]
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateLoginRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UpdateLoginRequest), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLoginAsync(string login, UpdateLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            if (!IsLoginValid(request.NewLogin))
                return BadRequest(request);
            string modifiedBy = User.FindFirst(JwtRegisteredClaimNames.Name)!.Value;
            bool isAdmin = bool.Parse(User.FindFirst(CustomClaimNames.IsAdmin)!.Value);
            if (!isAdmin && modifiedBy != login)
                return Forbid();
            try
            {
                User? dbUser = await _userRepository.ChangeLoginAsync(login, request.NewLogin, modifiedBy);
                if (dbUser is null)
                    return NotFound($"User {login} was not found");
                return Ok("Login was successfully changed");
            }
            catch (LoginAlreadyExistsException)
            {
                return Conflict(request);
            }
            catch (AccountDeactivatedException)
            {
                return NotFound(request);
            }
        }

        /// <summary>
        /// Gets users
        /// </summary>
        /// <returns>Active users</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /users
        ///
        /// </remarks>
        /// <response code="200">Array of active users</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// 
        [Route("/users")]
        [HttpGet]
        [Authorize(Policy = PolicyNames.IsAdmin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersAsync()
        {
            List<User> users = await _userRepository.GetActiveUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Signs up the user
        /// </summary>
        /// <returns>User model</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /login
        ///     {
        ///        "login": "YourCoolLogin",
        ///        "password": "YourCoolPassword"
        ///     }
        ///
        /// </remarks>
        /// <param name="request">Credentials</param>
        /// <response code="200">Logged in user</response>
        /// <response code="400">Model is invalid</response>
        /// <response code="404">User was not found</response>
        /// 
        [Route("/login")]
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LoginRequest), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            User? dbUser = await _userRepository.GetUserByLoginAsync(request.Login);
            if (dbUser is null)
                return NotFound($"Wrong username");
            try
            {
                UserClaims? userClaims = await _loginService.TryLoginUser(request.Login, request.Password);
                if (userClaims is null)
                    return NotFound($"Wrong password");
                string token = _jwtAuthManager.CreateToken(userClaims);
                Response.Headers.Authorization = token;
                UserResponse response = new UserResponse
                {
                    Name = dbUser.Name,
                    Gender = dbUser.Gender,
                    Birthday = dbUser.Birthday,
                    Admin = dbUser.Admin,
                    IsActive = dbUser.RevokedOn is null ? true : false
                };
                return Ok(response);
            }
            catch (AccountDeactivatedException)
            {
                return NotFound(request);
            }

        }

        /// <summary>
        /// Gets users above set age
        /// </summary>
        /// <returns>Array of users</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /users/above/{age}
        ///
        /// </remarks>
        /// <param name="age">Users older than this value will be get</param>
        /// <response code="200">Array of users above set age</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// 
        [Route("/users/above/{age}")]
        [HttpGet]
        [Authorize(Policy = PolicyNames.IsAdmin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersAboveAgeAsync(int age)
        {
            List<User> dbUsers = await _userRepository.GetAllUsersAboveAgeAsync(age);
            List<UserResponse> response = dbUsers.Select(u => new UserResponse
            {
                Name = u.Name,
                Gender = u.Gender,
                Birthday = u.Birthday,
                Admin = u.Admin,
                IsActive = u.RevokedOn is null ? true : false
            })
                .ToList();
            return Ok(response);
        }

        /// <summary>
        /// Deletes user
        /// </summary>
        /// <returns>Message</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users/{login}
        ///     {
        ///        "deleteMode": "0"
        ///     }
        ///
        /// </remarks>
        /// <param name="login">Login</param>
        /// <param name="request">Delete info</param>
        /// <response code="200">Message</response>
        /// <response code="400">Model is invalid</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// <response code="404">User was not found</response>
        /// 
        [HttpDelete]
        [Route("/users/{login}")]
        [Authorize(Policy = PolicyNames.IsAdmin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUserAsync(string login, DeleteUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(request);
            string revokedBy = User.FindFirst(JwtRegisteredClaimNames.Name)!.Value;
            switch (request.DeleteMode)
            {
                case 0:
                    if (!await _userRepository.DeactivateUserAsync(login, revokedBy))
                        return NotFound($"User {login} was not found");
                    return Ok("User was deactivated");
                case 1:
                    if (!await _userRepository.DeleteUserAsync(login))
                        return NotFound($"User {login} was not found");
                    return Ok("User was completely vanished from databased");
                default:
                    return BadRequest("Unexisting delete mode");
            }
        }

        /// <summary>
        /// Deletes user
        /// </summary>
        /// <returns>Message</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /users/{login}/restore
        ///
        /// </remarks>
        /// <param name="login">Login</param>
        /// <response code="200">Message</response>
        /// <response code="401">Authorize to make requests</response>
        /// <response code="403">You dont have access</response>
        /// <response code="404">User was not found</response>
        /// 
        [HttpPut]
        [Route("/users/{login}/restore")]
        [Authorize(Policy = PolicyNames.IsAdmin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreUserAsync(string login)
        {
            User? dbUser = await _userRepository.RestoreUserAsync(login);
            if (dbUser is null)
                return NotFound($"User {login} was not found");
            return Ok("User was sucessfully restored");
        }
        private bool IsLoginValid(string login) => Regex.IsMatch(login, @"^[a-zA-Z0-9]+$");
        private bool IsPasswordValid(string password) => Regex.IsMatch(password, @"^[a-zA-Z0-9]+$");
        private bool IsNameValid(string name) => Regex.IsMatch(name, @"^[a-zA-Z]+$|^[а-яА-Я]+$");

    }
}

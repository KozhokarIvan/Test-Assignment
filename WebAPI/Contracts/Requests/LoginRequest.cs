using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to sign in and get a token</para> 
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// <para>Your Login. Should be unique. Only English letters and numbers are allowed</para> 
        /// </summary>
        /// <example>User4234234</example>
        [Required]
        public string Login { get; set; } = null!;
        /// <summary>
        /// <para>Only English letters and numbers are allowed</para> 
        /// </summary>
        /// <example>Ivan</example>
        [Required]
        public string Password { get; set; } = null!;
    }
}

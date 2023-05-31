using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to change your password</para> 
    /// </summary>
    public class UpdatePasswordRequest
    {
        /// <summary>
        /// <para>Only English letters and numbers are allowed</para> 
        /// </summary>
        /// <example>Password234234</example>
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}

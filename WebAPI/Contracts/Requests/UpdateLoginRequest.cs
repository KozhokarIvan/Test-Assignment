using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to change your login</para> 
    /// </summary>
    public class UpdateLoginRequest
    {
        /// <summary>
        /// <para>Only English letters and numbers are allowed</para> 
        /// </summary>
        /// <example>Ivan</example>
        [Required]
        public string NewLogin { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to register a new user</para> 
    /// </summary>
    public class RegisterRequest
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
        /// <example>Password234234</example>
        [Required]
        public string Password { get; set; } = null!;
        /// <summary>
        /// <para>First name. Only English and Russian letters are allowed</para> 
        /// </summary>
        /// <example>Ivan</example>
        [Required]
        public string Name { get; set; } = null!;
        /// <summary>
        /// <para>All genders available:</para> 
        /// <para>0 - Female,</para> 
        /// <para>1 - Male,</para> 
        /// <para>2 - Unknown</para>
        /// </summary>
        /// <example>1</example>
        [Required]
        public int Gender { get; set; }
        /// <summary>
        /// <para>Date of birth</para>
        /// </summary>
        /// <example>2003-03-02T21:53:09.067Z</example>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// <para>Boolean value that indicates if the user is admin</para>
        /// </summary>
        /// <example>false</example>
        [Required]
        public bool Admin { get; set; }
    }
}

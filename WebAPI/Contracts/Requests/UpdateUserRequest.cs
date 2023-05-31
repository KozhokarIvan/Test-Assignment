using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to change user data.</para> 
    /// <para>Fill every field with data or you risk to change fields you didnt mean to.</para> 
    /// <para>All fields will replace the existing ones</para> 
    /// </summary>
    public class UpdateUserRequest
    {
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
    }
}

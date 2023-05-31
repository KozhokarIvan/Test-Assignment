using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts.Requests
{
    /// <summary>
    /// <para>Information required to delete a user</para> 
    /// </summary>
    public class DeleteUserRequest
    {
        /// <summary>
        /// <para>0 - deactivate account,</para>  <para>1 - vanish account from database</para> 
        /// </summary>
        /// <example>0</example>
        [Required]
        public int DeleteMode { get; set; }
    }
}

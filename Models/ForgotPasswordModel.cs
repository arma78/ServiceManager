
using System.ComponentModel.DataAnnotations;

namespace ServiceManager.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
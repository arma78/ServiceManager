using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace ServiceManager.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [PersonalData, Required, StringLength(20)]
        public string FirstName { get; set; }

        [PersonalData, Required, StringLength(20)]
        public string LastName { get; set; }

        [PersonalData, Required, StringLength(40)]
        public string Professional_Skill { get; set; }

        [PersonalData, StringLength(6)]
        public string PhoneCodeValidator { get; set; }

        public string FullName { get { return $"{FirstName} {LastName}"; } }
    }
}
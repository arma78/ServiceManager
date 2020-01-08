using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace ServiceManager.Models
{
    public class ApplicationUser : IdentityUser<string>
    {
        [PersonalData, Required, StringLength(20)]
        public string FirstName { get; set; }

        [PersonalData, Required, StringLength(20)]
        public string LastName { get; set; }

        public string FullName { get { return $"{FirstName} {LastName}"; } }
    }
}
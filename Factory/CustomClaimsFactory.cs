using ServiceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServiceManager.Factory
{
    public class CustomClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, AppRole>
    {
        public CustomClaimsFactory(UserManager<ApplicationUser> userManager , RoleManager<AppRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("firstname", user.FirstName));
            identity.AddClaim(new Claim("lastname", user.LastName));

            return identity;
        }
    }
}
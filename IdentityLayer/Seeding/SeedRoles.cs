using IdentityLayer.Constants;
using Microsoft.AspNetCore.Identity;

namespace IdentityLayer.Seeding
{
    public class SeedRoles
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(RoutingServiceRoles.Manager.ToString()));
                await roleManager.CreateAsync(new IdentityRole(RoutingServiceRoles.Admin.ToString()));
                await roleManager.CreateAsync(new IdentityRole(RoutingServiceRoles.User.ToString()));
            }
        }
    }
}

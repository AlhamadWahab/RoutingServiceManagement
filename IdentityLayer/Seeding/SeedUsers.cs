using IdentityLayer.Constants;
using IdentityLayer.IdnetityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace IdentityLayer.Seeding
{
    public static class SeedUsers
    {

        public static async Task SeedManagerUserAsync(UserManager<RoutingServiceAppUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            var managerSection = configuration.GetSection("MANAGER");
            var managerSetting = managerSection.Get<ManagerSetting>() ??
                throw new InvalidOperationException("Mangaer settings are not properly configured.");
            var managerUser = new RoutingServiceAppUser
            {
                FirstName = managerSetting.FirstName,
                LastName = managerSetting.LastName,
                UserName = managerSetting.UserName,
                Email = managerSetting.Email,
                EmailConfirmed = true,
            };
            var user = await userManager.FindByEmailAsync(managerUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(managerUser, managerSetting.Password);
                await userManager.AddToRolesAsync(managerUser, new List<string>()
                {
                    RoutingServiceRoles.Manager.ToString(),
                    RoutingServiceRoles.Admin.ToString(),
                    RoutingServiceRoles.User.ToString()
                });
            }
        }
    }
}
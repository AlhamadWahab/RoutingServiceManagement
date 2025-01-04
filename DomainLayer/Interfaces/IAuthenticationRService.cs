using IdentityLayer.IdnetityModels;

namespace DomainLayer.Interfaces
{
    public interface IAuthenticationRService
    {
        Task<AuthenticationModel> RegisterAsync(RegisterModel register);
        Task<AuthenticationModel> LoginAsync(TokenRequestModel tokenRequestModel);
        Task<string> AddRoleAsync(AddRoleModel roleModel);
    }
}

using DomainLayer.Interfaces;
using IdentityLayer;
using IdentityLayer.Constants;
using IdentityLayer.IdnetityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class AuthenticationRService : IAuthenticationRService
    {
        private readonly UserManager<RoutingServiceAppUser> _userManager;
        private readonly JwtSetting _jwtSetting;
        public readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticationRService(UserManager<RoutingServiceAppUser> userManager, IOptions<JwtSetting> jwtSetting, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _jwtSetting = jwtSetting.Value ?? throw new ArgumentNullException(nameof(jwtSetting));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        /// <summary>
        /// Adds a role to a specified user.
        /// Checks if the user and role exist before assigning the role.
        /// </summary>
        /// <param name="roleModel">The model containing user ID and role name.</param>
        /// <returns>A task representing the asynchronous operation, with a message indicating the result.</returns>
        public async Task<string> AddRoleAsync(AddRoleModel roleModel)
        {
            var user = await _userManager.FindByIdAsync(roleModel.UserId);
            /// checking if the user and role exists in database 
            if (user == null || !await _roleManager.RoleExistsAsync(roleModel.RoleName))
            {
                return "Invalid user ID or user Role";
            }
            if (await _userManager.IsInRoleAsync(user, roleModel.RoleName))
            {
                return "user already assignd to this role.";
            }
            var result = await _userManager.AddToRoleAsync(user, roleModel.RoleName);

            return result.Succeeded ? string.Empty : "One Error accourding.";
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login.
        /// Validates the user's email and password, and returns an authentication model with the token and user details.
        /// </summary>
        /// <param name="tokenRequestModel">The model containing the user's email and password.</param>
        /// <returns>A task representing the asynchronous operation, with an authentication model containing the token and user details.</returns>
        public async Task<AuthenticationModel> LoginAsync(TokenRequestModel tokenRequestModel)
        {
            var authenticateModel = new AuthenticationModel();
            var user = await _userManager.FindByEmailAsync(tokenRequestModel.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, tokenRequestModel.Password))
            {
                authenticateModel.Message = "one error accourd, Email or Password invaled!";
                return authenticateModel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            authenticateModel.IsAuthenticated = true;
            authenticateModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authenticateModel.Email = tokenRequestModel.Email;
            authenticateModel.UserName = user.UserName ?? "";
            authenticateModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authenticateModel.Roles = roles.ToList();
            return authenticateModel;
        }

        /// <summary>
        /// Registers a new user and assigns them a default role.
        /// Validates the uniqueness of the email and username before creating the user.
        /// </summary>
        /// <param name="register">The model containing registration details.</param>
        /// <returns>A task representing the asynchronous operation, with an authentication model indicating the result of the registration.</returns>
        public async Task<AuthenticationModel> RegisterAsync(RegisterModel register)
        {
            if (await _userManager.FindByEmailAsync(register.Email) != null)
            {
                return new AuthenticationModel { Message = "Email is already registered!" };
            }

            if (await _userManager.FindByNameAsync(register.Username) != null)
            {
                return new AuthenticationModel { Message = "Username is already registered!" };
            }

            /// var user = _mapper.Map<RoutingServiceAppUser>(register); or:
            var user = new RoutingServiceAppUser
            {
                Email = register.Email,
                UserName = register.Username,
                FirstName = register.FirstName,
                LastName = register.LastName
            };

            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return new AuthenticationModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, RoutingServiceRoles.User.ToString());

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthenticationModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { RoutingServiceRoles.User.ToString() },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName,
                Message = "Registration successful!"
            };
        }

        /// <summary>
        /// Creates a JWT token for a specified user.
        /// Generates claims based on the user's information and roles, and signs the token using the configured signing key.
        /// </summary>
        /// <param name="user">The user for whom the JWT token is being created.</param>
        /// <returns>A task representing the asynchronous operation, with the generated JWT token.</returns>
        private async Task<JwtSecurityToken> CreateJwtToken(RoutingServiceAppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();
            var appClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("uid", user.Id)
        }.Union(userClaims).Union(roleClaims).ToList();

            if (string.IsNullOrEmpty(_jwtSetting.SIGNINGKEY))
            {
                throw new InvalidOperationException("JWT Signing Key is not configured.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SIGNINGKEY));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwtSetting.ISSUER,
                audience: _jwtSetting.AUDIENCE,
                expires: DateTime.UtcNow.AddMinutes(_jwtSetting.LIFTIME),
                claims: appClaims,
                signingCredentials: signingCredentials);
        }
    }
}

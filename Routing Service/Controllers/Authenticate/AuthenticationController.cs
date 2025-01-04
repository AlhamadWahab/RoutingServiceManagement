using DomainLayer.Interfaces;
using IdentityLayer.IdnetityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Routing_Service.Controllers.Authenticate
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationRService rService) : ControllerBase
    {
        private readonly IAuthenticationRService _rService = rService;

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _rService.RegisterAsync(register);
            if (!result.IsAuthenticated) { return BadRequest(result.Message); }
            return Ok(result);
            /// or:
            /*
             return Ok(new { token = result.Token, expiresOn = result.ExpiresOn});
             */
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] TokenRequestModel tokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _rService.LoginAsync(tokenRequest);
            if (!result.IsAuthenticated) { return BadRequest(result.Message); }
            return Ok(result);
        }

        [HttpPost("Assign Role")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignRole([FromBody] AddRoleModel roleModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _rService.AddRoleAsync(roleModel);
            if (!string.IsNullOrEmpty(result)) { return BadRequest(result); }
            return Ok(roleModel); /// if we want the obj in frontend or mobile app using or :  return Ok("user assign ssuccessfully..")
        }
    }
}

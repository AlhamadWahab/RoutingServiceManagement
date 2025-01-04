using System.ComponentModel.DataAnnotations;
namespace IdentityLayer.IdnetityModels
{
    public class TokenRequestModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

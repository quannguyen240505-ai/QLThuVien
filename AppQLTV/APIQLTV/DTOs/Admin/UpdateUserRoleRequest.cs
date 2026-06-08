using System.ComponentModel.DataAnnotations;

namespace APIQLTV.DTOs.Admin
{
    public class UpdateUserRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}

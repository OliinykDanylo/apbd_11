using System.ComponentModel.DataAnnotations;

namespace DeviceHubUpd.DTOs;

public class LoginRequest
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
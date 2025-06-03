using System.ComponentModel.DataAnnotations;

namespace DeviceHubUpd.DTOs;

public class RegisterAccountRequest
{
    [Required]
    [RegularExpression(@"^[^\d][\w]{2,}$", ErrorMessage = "Username must not start with a digit.")]
    public string UserName { get; set; } = null!;

    [Required]
    [MinLength(12)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must include upper, lower, digit and symbol.")]
    public string Password { get; set; } = null!;

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int RoleId { get; set; }
}
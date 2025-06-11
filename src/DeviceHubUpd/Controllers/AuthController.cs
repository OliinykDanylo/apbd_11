using DeviceHubUpd.DTOs;
using DeviceHubUpd.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAuthService _authService;

    public AuthController(IAccountService accountService, IAuthService authService)
    {
        _accountService = accountService;
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
    {
        var account = await _accountService.Authenticate(request.UserName, request.Password);
        if (account == null)
            return Unauthorized("Invalid credentials.");

        var token = _authService.GenerateJwtToken(account);
        return Ok(new { token });
    }
}
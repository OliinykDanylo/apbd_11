using DeviceHubUpd.DTOs;
using DeviceHubUpd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Roles = "Admin")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request)
    {
        if (await _accountService.UsernameExists(request.UserName))
            return BadRequest("Username already taken.");

        var account = await _accountService.RegisterAccount(request);
        return Ok(new { message = "Account created.", accountId = account.Id });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] RegisterAccountRequest request)
    {
        var existingAccount = await _accountService.GetAccountById(id);
        if (existingAccount == null)
            return NotFound("Account not found.");

        existingAccount.UserName = request.UserName;
        existingAccount.Password = request.Password;

        await _accountService.UpdateAccount(existingAccount);
        return Ok(new { message = "Account updated." });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var existingAccount = await _accountService.GetAccountById(id);
        if (existingAccount == null)
            return NotFound("Account not found.");

        await _accountService.DeleteAccount(id);
        return Ok(new { message = "Account deleted." });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _accountService.GetAllAccounts();
        var result = accounts.Select(a => new
        {
            a.Id,
            a.UserName,
            a.Password
        });

        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await _accountService.GetAccountById(id);
        if (account == null)
            return NotFound();

        return Ok(new
        {
            account.UserName,
            account.Password
        });
    }
}
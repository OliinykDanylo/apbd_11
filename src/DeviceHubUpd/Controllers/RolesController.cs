using DeviceHubUpd.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;
    private readonly ILogger<RolesController> _logger;
    
    public RolesController(DeviceHubUpdContext context, ILogger<RolesController> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return StatusCode(500, ex.Message);
        }
    }
}
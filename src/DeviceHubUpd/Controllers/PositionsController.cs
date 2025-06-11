using DeviceHubUpd.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;
    private readonly ILogger<RolesController> _logger;
    
    public PositionsController(DeviceHubUpdContext context, ILogger<RolesController> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPositions()
    {
        try
        {
            var positions = await _context.Positions.Select(p => new { p.Id, p.Name }).ToListAsync();
            return Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching positions");
            return StatusCode(500, ex.Message);
        }
        
    }
}
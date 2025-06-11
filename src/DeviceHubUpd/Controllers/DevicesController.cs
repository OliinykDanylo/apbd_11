using System.Security.Claims;
using System.Text.Json;
using DeviceHubUpd.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPI.DTO;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(DeviceHubUpdContext context, ILogger<DevicesController> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        try
        {
            var result = await _context.Devices
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Ok(result);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching devices");
            return StatusCode(500, ex.Message);
        }
        
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDevice(int id)
    {
        try
        {
            var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees)
                .ThenInclude(de => de.Employee)
                    .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();

            var currentEmployeeRelation = device.DeviceEmployees
                .OrderByDescending(de => de.IssueDate)
                .FirstOrDefault();
            
            if (!User.IsInRole("Admin"))
            {
                var userEmail = User.Identity?.Name;
                if (userEmail == null)
                    return Forbid();
                
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.UserName == userEmail);

                if (account == null || account.EmployeeId == null)
                    return Forbid();

                var employeeId = account.EmployeeId;
                
                var isOwner = await _context.DeviceEmployees
                    .AnyAsync(de => de.DeviceId == id && de.EmployeeId == employeeId);

                if (!isOwner)
                    return Forbid();
            }

            var dto = new DeviceDetailDto
            {
                Name = device.Name,
                IsEnabled = device.IsEnabled,
                AdditionalProperties = JsonSerializer.Deserialize<object>(device.AdditionalProperties),
                DeviceTypeName = device.DeviceType?.Name
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching device details");
            return StatusCode(500, ex.Message);
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] DeviceCreateDto dto)
    {
        try
        {
            var deviceType = await _context.DeviceTypes.FirstOrDefaultAsync(dt => dt.Id == dto.TypeId);
            if (deviceType == null) return BadRequest("Invalid device type id.");

            var device = new Device
            {
                Name = dto.Name,
                DeviceTypeId = deviceType.Id,
                IsEnabled = dto.IsEnabled,
                AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties)
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, new { device.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating device");
            return StatusCode(500, ex.Message);
        }
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] DeviceCreateDto dto)
    {
        try
        {
            var device = await _context.Devices
                .Include(d => d.DeviceEmployees)
                .ThenInclude(de => de.Employee)
                .ThenInclude(e => e.Person)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();

            // if user is not admin, ensure they own the device
            if (!User.IsInRole("Admin"))
            {
                var userEmail = User.Identity?.Name;
                if (userEmail == null)
                    return Forbid();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.UserName == userEmail);

                if (account == null || account.EmployeeId == null)
                    return Forbid();

                var employeeId = account.EmployeeId;

                var isOwner = await _context.DeviceEmployees
                    .AnyAsync(de => de.DeviceId == id && de.EmployeeId == employeeId);

                if (!isOwner)
                    return Forbid();
            }

            var deviceType = await _context.DeviceTypes.FirstOrDefaultAsync(dt => dt.Id == dto.TypeId);
            if (deviceType == null) return BadRequest("Invalid device type id.");

            device.Name = dto.Name;
            device.DeviceTypeId = deviceType.Id;
            device.IsEnabled = dto.IsEnabled;
            device.AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties);

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device");
            return StatusCode(500, ex.Message);
        }
    }

    // admin only: Delete device
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        try
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting device");
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet("types")]
    public async Task<IActionResult> GetDeviceTypes()
    {
        try
        {
            var types = await _context.DeviceTypes.Select(dt => new { dt.Id, dt.Name }).ToListAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching device types");
            return StatusCode(500, ex.Message);
        }
    }
}
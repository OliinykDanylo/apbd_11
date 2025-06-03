using System.Security.Claims;
using System.Text.Json;
using DeviceHubUpd.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPI.DTO;

namespace DeviceHubUpd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;

    public DevicesController(DeviceHubUpdContext context)
    {
        _context = context;
    }

    // admins only: Get all devices
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        var result = await _context.Devices
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();

        return Ok(result);
    }

    // admin or assigned User: Get specific device
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDevice(int id)
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
        
        // If user is not an admin, ensure they are assigned to this device
        if (!User.IsInRole("Admin"))
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null)
                return Forbid();

            // Get the user's account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserName == userEmail);

            if (account == null || account.EmployeeId == null)
                return Forbid();

            var employeeId = account.EmployeeId;

            // Check if this employee is assigned to the device
            var isOwner = await _context.DeviceEmployees
                .AnyAsync(de => de.DeviceId == id && de.EmployeeId == employeeId);

            if (!isOwner)
                return Forbid();
        }

        var dto = new DeviceDetailDto
        {
            Name = device.Name,
            DeviceTypeName = device.DeviceType?.Name,
            IsEnabled = device.IsEnabled,
            AdditionalProperties = JsonSerializer.Deserialize<object>(device.AdditionalProperties),
            CurrentEmployee = currentEmployeeRelation?.Employee != null
                ? new DeviceEmployeeDto
                {
                    Id = currentEmployeeRelation.Employee.Id,
                    FullName = $"{currentEmployeeRelation.Employee.Person.FirstName} {currentEmployeeRelation.Employee.Person.MiddleName} {currentEmployeeRelation.Employee.Person.LastName}"
                }
                : null
        };

        return Ok(dto);
    }

    // admin only: Create new device
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] DeviceCreateDto dto)
    {
        var deviceType = await _context.DeviceTypes.FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName);
        if (deviceType == null) return BadRequest("Invalid device type name.");

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

    // admin or assigned User: Update device
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] DeviceCreateDto dto)
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

            // Get the user's account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserName == userEmail);

            if (account == null || account.EmployeeId == null)
                return Forbid();

            var employeeId = account.EmployeeId;

            // Check if this employee is assigned to the device
            var isOwner = await _context.DeviceEmployees
                .AnyAsync(de => de.DeviceId == id && de.EmployeeId == employeeId);

            if (!isOwner)
                return Forbid();
        }

        var deviceType = await _context.DeviceTypes.FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName);
        if (deviceType == null) return BadRequest("Invalid device type name.");

        device.Name = dto.Name;
        device.DeviceTypeId = deviceType.Id;
        device.IsEnabled = dto.IsEnabled;
        device.AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // admin only: Delete device
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound();

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
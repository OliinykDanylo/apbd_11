using DeviceHubUpd.DAL;
using Microsoft.AspNetCore.Authorization;

namespace DeviceHubUpd.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;

    public EmployeesController(DeviceHubUpdContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        try
        {
            var employees = await _context.Employees
                .Include(e => e.Person)
                .Select(e => new
                {
                    e.Id,
                    FullName = $"{e.Person.FirstName} {e.Person.MiddleName} {e.Person.LastName}"
                })
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Person)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return Ok(new
            {
                Person = new
                {
                    employee.Person.Id,
                    employee.Person.PassportNumber,
                    employee.Person.FirstName,
                    employee.Person.MiddleName,
                    employee.Person.LastName,
                    employee.Person.PhoneNumber,
                    employee.Person.Email
                },
                employee.Salary,
                employee.HireDate,
                Position = new
                {
                    employee.Position.Id,
                    employee.Position.Name
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
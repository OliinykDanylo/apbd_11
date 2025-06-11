using DeviceHubUpd.DAL;
using DeviceHubUpd.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DeviceHubUpd.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly DeviceHubUpdContext _context;
    private readonly ILogger<EmployeesController> _logger;
    public DbSet<Person> People { get; set; }

    public EmployeesController(DeviceHubUpdContext context, ILogger<EmployeesController> logger)
    {
        _logger = logger;
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
            _logger.LogError(ex, "Error fetching employees");
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
            
            String PositionName = employee.Position?.Name ?? "Unknown";

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
                PositionName,
                employee.HireDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employee with ID {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDTO dto)
    {
        try
        {
            // Create Person entity
            var person = new Person
            {
                PassportNumber = dto.Person.PassportNumber,
                FirstName = dto.Person.FirstName,
                MiddleName = dto.Person.MiddleName,
                LastName = dto.Person.LastName,
                PhoneNumber = dto.Person.PhoneNumber,
                Email = dto.Person.Email
            };
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            // Create Employee entity
            var employee = new Employee
            {
                PersonId = person.Id,
                Salary = dto.Salary,
                PositionId = dto.PositionId,
                HireDate = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new { employee.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, ex.Message);
        }
    }
}
namespace DeviceHubUpd.DTOs;

public class EmployeeCreateDTO
{
    public PersonCreateDto Person { get; set; } = null!;
    public decimal Salary { get; set; }
    public int PositionId { get; set; }
}
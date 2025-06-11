namespace DeviceHubUpd.Middleware;

public class ValidationRule
{
    public String Type { get; set; }
    public String PreRequestName { get; set; }
    public String PreRequestValue { get; set; }
    public List<Rule> Rules { get; set; } = new List<Rule>();
}
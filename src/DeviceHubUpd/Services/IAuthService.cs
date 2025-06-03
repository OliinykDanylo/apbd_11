namespace DeviceHubUpd.Services;

public interface IAuthService
{
    string GenerateJwtToken(Account account);
}
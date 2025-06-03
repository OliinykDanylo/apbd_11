using DeviceHubUpd.DTOs;

namespace DeviceHubUpd.Services;

public interface IAccountService
{
    Task<bool> UsernameExists(string username);
    Task<Account?> Authenticate(string username, string password);
    Task<Account> RegisterAccount(RegisterAccountRequest request);
    Task<Account?> GetAccountById(int id);
    Task<List<Account>> GetAllAccounts();
    Task UpdateAccount(Account account);
    Task DeleteAccount(int id);
}
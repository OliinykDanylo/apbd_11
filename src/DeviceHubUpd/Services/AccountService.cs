using DeviceHubUpd.DAL;
using DeviceHubUpd.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeviceHubUpd.Services;

public class AccountService : IAccountService
{
    private readonly DeviceHubUpdContext _context;

    public AccountService(DeviceHubUpdContext context)
    {
        _context = context;
    }

    public async Task<bool> UsernameExists(string username) =>
        await _context.Accounts.AnyAsync(a => a.UserName == username);

    public async Task<Account?> Authenticate(string username, string password)
    {
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.UserName == username);

        if (account == null || !BCrypt.Net.BCrypt.Verify(password, account.Password))
            return null;

        return account;
    }

    public async Task<Account> RegisterAccount(RegisterAccountRequest request)
    {
        var account = new Account
        {
            UserName = request.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmployeeId = request.EmployeeId,
            RoleId = request.RoleId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> GetAccountById(int id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<List<Account>> GetAllAccounts()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task UpdateAccount(Account account)
    {
        // Password should be hashed if manually updated
        var existing = await _context.Accounts.FindAsync(account.Id);
        if (existing == null) return;

        existing.UserName = account.UserName;

        // If password is not already hashed, hash it
        if (!BCrypt.Net.BCrypt.Verify(account.Password, existing.Password))
        {
            existing.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}
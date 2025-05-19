using Microsoft.EntityFrameworkCore;
using DataContext;
using Managers;
using TableData;

namespace Entity.Repository
{
    public class AccountRepository
    {
        private readonly AppDbContext _context;
        private DbSet<AccountDb> _accounts;
        private readonly GameDataManager _gameDataManager;
        
        public AccountRepository(AppDbContext context, GameDataManager gameDataManager)
        {
            _context = context;
            _accounts = context.Set<AccountDb>();
            _gameDataManager = gameDataManager;
        }

        public Task<AccountDb?> GetAccountInfo(long accountId)
        {
            return _accounts
            .AsNoTracking()
            .Where(a => a.account_id == accountId)
            .FirstOrDefaultAsync();
        }

        public async Task<AccountDb?> GetAccountInfoAll(string accountId)
        {
            var account = await _accounts
                .Where(a => a.account_name == accountId)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return null;
            }

            var accountEntry = _context.Entry(account);

            await accountEntry.Collection(a => a.items!).LoadAsync();

            return account;
        }
        
        public async Task<AccountDb?> GetAccountInfoAll(long accountId)
        {
            var account = await _accounts
                .Where(a => a.account_id == accountId)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return null;
            }

            var accountEntry = _context.Entry(account);

            await accountEntry.Collection(a => a.items!).LoadAsync();

            return account;
        }

        public AccountDb  AccountRegister(string username, string password, int levelCode)
        {
            AccountDb account = new AccountDb()
            {
                account_name = username,
                password = password,
                level_code = levelCode
            };
            
            _accounts.Add(account);

            return account;
        }
        
        public List<long> GetAllAccountIds()
        {
            return _accounts.Select(a => a.account_id).ToList();
        }
    }
}
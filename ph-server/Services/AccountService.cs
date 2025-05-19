using Microsoft.IdentityModel.Tokens;
using DataContext;
using Entity;
using Services.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Constants;
using Entity.DbSet;
using Managers;
using TableData;
using Protocols;

namespace Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly RedisCacheService _redis;
        private readonly IConfiguration _configuration;
        private readonly GameDataManager _gameDataManager;

        private TimeSpan _expireTimeSpan;
        
        public AccountService(AppDbContext context, RedisCacheService redis, IConfiguration configuration, GameDataManager gameDataManager)
        {
            _context = context;
            _redis = redis;
            _configuration = configuration;
            _expireTimeSpan = TimeSpan.FromMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? throw new Exception("Expire Minutes")));
            _gameDataManager = gameDataManager;
        }

        public async Task<LoginRes> LoginAccount(LoginReq req)
        {
            LoginRes res = new LoginRes();

            AccountDb? account = await _context.AccountRepo.GetAccountInfoAll(req.AccountName);
            if (account == null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        account = await CreateAccount(req.AccountName);
                        await transaction.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"[LoginAccount] DataBase Error: {e}");
                    }
                }
            } 

            string jwtToken = GenerateJwtToken(account);
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken deserializeToken = jwtHandler.ReadJwtToken(jwtToken);
            string tokenJti = deserializeToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            UserCache userCache = new UserCache(account.account_id.ToString(), _redis);

            await userCache.Save(account);
            await userCache.UpdateToken(tokenJti, _expireTimeSpan);
            
            res.Token = jwtToken;
            res.UpdateData.Account = account.ToDto;
            res.UpdateData.Items = account.items?.Select(x => x.ToDto).ToList();
            return res;
        }

        private async Task<AccountDb> CreateAccount(string accountName, string password = "test_password")
        {
            List<LevelTable> accountTable = _gameDataManager.GetDatas<LevelTable>();

            LevelTable? nextLevelAccount = accountTable
                .OrderBy(a => a.Level)
                .FirstOrDefault();

            if (nextLevelAccount == null)
            {
                throw new Exception($"[CreateAccount] Default Account Level Code Not Found: {accountName}");
            }

            
            AccountDb account = _context.AccountRepo.AccountRegister(accountName, password, nextLevelAccount.Code);
            await _context.SaveChangesAsync();
            return account;
        }

        private string GenerateJwtToken(AccountDb account)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("GenerateJwtToken Error Jwt:Key")));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims =
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.account_id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expireTimeSpan.Minutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> RefreshJwt(string accountId)
        {
            // redis에서 계정 정보 조회
            UserCache user = new UserCache(accountId, _redis);
            AccountDb? account = await user.LoadAccount();
            if (account == null)
            {
                throw new Exception($"[RefreshJwt] Account not found: {accountId}");
            }

            string jwtToken = GenerateJwtToken(account);
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken deserializeToken = jwtHandler.ReadJwtToken(jwtToken);
            string tokenJti = deserializeToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            await user.UpdateToken(tokenJti);
            await user.UpdateTTL(_expireTimeSpan);

            return jwtToken;
        }

        public async Task<AccountInfoRes?> GetAccountInfo(string accountId)
        {
            AccountInfoRes res = new AccountInfoRes();
            
            // redis에서 계정 정보 조회
            UserCache user = new UserCache(accountId, _redis);
            AccountDb? account = await user.LoadAccount();
            if (account == null)
            {
                throw new Exception($"[RefreshJwt] Account not found: {accountId}");
            }

            res.Account = account.ToDto;

            return res;
        }

        public async Task<AccountCurrencyInfoRes> GetAccountCurrencyInfo(string accountId)
        {
            AccountCurrencyInfoRes res = new AccountCurrencyInfoRes();
            
            UserCache user = new UserCache(accountId, _redis);
            AccountDb? account = await user.LoadAccount();
            if (account == null)
            {
                throw new Exception($"[GetAccountCurrencyInfo] Account not found: {accountId}");
            }

            res.Currency = account.ToCurrencyDto;
            
            return res;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Constants;
using DataContext;
using Entity;
using Entity.DbSet;

namespace Services.Redis
{
    public class UserCache
    {
        private readonly RedisCacheService _redis;
        private string _accountId;
        private AccountDb? _accountDb;

        public UserCache(string accountId, RedisCacheService redis)
        {
            _redis = redis;
            _accountId = accountId;
        }

        public async Task<AccountDb?> LoadAccount(bool includeItem = false)
        {
            if (_accountDb != null)
                return _accountDb;

            _accountDb = await _redis.GetHashCacheAsync<AccountDb>(_accountId, "Account", RedisDefine.DB_ACCOUNT);

            if (_accountDb != null && includeItem == true)
            {
                await LoadItems();
            }

            return _accountDb;
        }

        public async Task<ICollection<ItemDb>> LoadItems()
        {
            AccountDb? account = await LoadAccount();
            if (account == null)
                throw new InvalidOperationException("Account is null");

            account.items = await _redis.GetHashCacheAsync<ICollection<ItemDb>>(_accountId, "Items", RedisDefine.DB_ACCOUNT);

            if (account.items == null)
                account.items = new List<ItemDb>();

            return account.items;
        }

        public async Task<int> LoadStageStart()
        {
            AccountDb? account = await LoadAccount();
            if (account == null)
                throw new InvalidOperationException("Account is null");

            int startCode = await _redis.GetHashCacheAsync<int>(_accountId, "StagesStart", RedisDefine.DB_ACCOUNT);

            return startCode;
        }

        public async Task Save(AccountDb accountDb)
        {
            _accountDb = accountDb;

            await SaveAll();
        }

        public async Task SaveAll()
        {
            await SaveAccount();
            await SaveItems();
        }

        public async Task UpdateTTL(TimeSpan ttl)
        {
            if (_accountDb == null)
                return;

            await _redis.SetExpiryAsync(_accountId, ttl, RedisDefine.DB_ACCOUNT);
        }

        public async Task UpdateToken(string token, TimeSpan? ttl = null)
        {
            if (_accountDb == null)
                return;

            await _redis.SetHashCacheAsync(_accountId, "Token", token, RedisDefine.DB_ACCOUNT, ttl);
        }

        public async Task SaveAccount()
        {
            if (_accountDb == null)
                return;

            // 릴레이션 정보는 저장하지 않음
            AccountDb? account = new AccountDb
            {
                account_id = _accountDb.account_id,
                account_name = _accountDb.account_name,
                password = _accountDb.password,
                level_code = _accountDb.level_code,
                current_exp = _accountDb.current_exp,
                gold = _accountDb.gold,
                cash = _accountDb.cash,
                CreatedAt = _accountDb.CreatedAt,
                IsActive = _accountDb.IsActive
            };

            await _redis.SetHashCacheAsync(_accountId, "Account", account, RedisDefine.DB_ACCOUNT);
        }

        public async Task SaveItems()
        {
            if (_accountDb == null)
                return;

            await _redis.SetHashCacheAsync(_accountId, "Items", _accountDb.items, RedisDefine.DB_ACCOUNT);
        }

        public async Task SaveStageStart(int stageCode)
        {
            if (_accountDb == null)
            {
                return;
            }
            
            await _redis.SetHashCacheAsync(_accountId, "StagesStart", stageCode, RedisDefine.DB_ACCOUNT);
        }
        
        public async Task RemoveStageStart()
        {
            if (_accountDb == null)
            {
                return;
            }
            
            await _redis.RemoveHashCacheAsync(_accountId, "StagesStart", RedisDefine.DB_ACCOUNT);
        }

        public async Task<string?> GetToken()
        {
            return await _redis.GetHashCacheAsync(_accountId, "Token", RedisDefine.DB_ACCOUNT);
        }
        
        private string GetRedisKey(string tableName)
        {
            return tableName switch
            {
                "AccountDb" => "Account",
                "CharacterDb" => "Character",
                "StageDb" or "StageRankRewardDb" => "Stages",
                "StageEntrySlotDb" => "StageEntry",
                "ItemDb" => "Items",
                "HuntingDb" or "HuntingRankRewardDb" => "Huntings",
                "BossBattleDb" or "BossBattleRankRewardDb" => "BossBattles",
                _ => string.Empty
            };
        }
    }
}

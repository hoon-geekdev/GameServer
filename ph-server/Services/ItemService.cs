using Constants;
using DataContext;
using Managers;
using Entity;
using Entity.DbSet;
using Protocols;
using Services.Redis;
using Serilog;
using Protocols.DTOs;
using TableData;

namespace Services
{
    public class ItemService
    {
        private readonly AppDbContext _context;
        private readonly GameDataManager _gameDataManager;
        private readonly RedisCacheService _redis;

        public ItemService(AppDbContext context, RedisCacheService redis, GameDataManager gameDataManager)
        {
            _context = context;
            _gameDataManager = gameDataManager;
            _redis = redis;
        }

        public async Task<ItemAcqRes?> ItemAcq(string accountId, ItemAcqReq req)
        {
            if (req.Items == null || req.Items.Count == 0)
                throw new Exception("[ItemAcq] Invalid item list");

            // insert item to db
            ItemAcqRes res = new ItemAcqRes();
            UserCache userCache = new UserCache(accountId, _redis);
            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    AccountDb? account = await userCache.LoadAccount(true);
                    if (account == null)
                        throw new Exception($"[ItemAcq] Account not found: {accountId}");

                    List<ItemDb>? resultItems = _context.ItemRepo.CreateOrSum(account, req.Items);

                    _context.SaveChanges();
                    await transaction.CommitAsync();
                    await userCache.SaveItems();

                    res.UpdateData.Items = resultItems.Select(x => x.ToDto).ToList();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"[ItemAcq] Error occurred while acquiring item for user: {accountId}\r\nex");
                }
            }

            return res;
        }

        public async Task<ItemListRes?> ItemList(string accountId)
        {
            UserCache userCache = new UserCache(accountId, _redis);
            AccountDb? account = await userCache.LoadAccount(true);
            if (account == null)
                throw new Exception($"[ItemList] Account not found: {accountId}");

            ICollection<ItemDb>? items = account.items;
            ItemListRes res = new ItemListRes();
            res.UpdateData.Items = items?.Select(x => x.ToDto).ToList();

            return res;
        }

        private List<ItemDb> GetItemsToUnequip(AccountDb account, long characterId, int partsCode, long itemId)
        {
            return account.items?
                .Where(x => x.equipped_character_id == characterId && x.item_id != itemId &&
                            _gameDataManager.GetData<ItemTable>(x.item_code)?.Parts_code == partsCode)
                .ToList() ?? new List<ItemDb>();
        }

        public async Task<ItemUnequipRes?> ItemUnequip(string accountId, ItemUnequipReq req)
        {
            UserCache userCache = new UserCache(accountId, _redis);
            AccountDb? account = await userCache.LoadAccount(true);

            if (account == null)
                throw new Exception($"[ItemUnequip] Account not found: {accountId}");

            ItemDb? unequipItem = account.items?.FirstOrDefault(x => x.item_id == req.ItemId);
            if (unequipItem == null)
                throw new Exception($"[ItemUnequip] Item not found - accountId: {accountId}, unequipItem: {req.ItemId}");

            ItemTable? itemTable = _gameDataManager.GetData<ItemTable>(unequipItem.item_code);
            if (itemTable == null)
                throw new Exception($"[ItemUnequip] Invalid item code: {unequipItem.item_code}");

            if (itemTable.Parts_code == 0)
                throw new Exception($"[ItemUnequip] cannot unequip: {unequipItem.item_code}, parts_code: {itemTable.Parts_code}");

            ItemUnequipRes res = new ItemUnequipRes();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ItemDb? item = _context.ItemRepo.Unequip(account, req.ItemId);
                    if (item == null)
                        throw new Exception($"[ItemUnequip] Item not found - accountId: {accountId}, itemId: {req.ItemId}");

                    _context.SaveChanges();
                    await transaction.CommitAsync();
                    await userCache.SaveItems();

                    res.UpdateData.Items = new List<ItemDto> { item.ToDto };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"[ItemUnequip] Error occurred while unequipping item for user: {accountId}\r\n{ex}");
                }
            }

            return res;
        }
    }
}

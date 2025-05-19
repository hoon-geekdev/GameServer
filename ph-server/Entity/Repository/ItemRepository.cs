using Microsoft.EntityFrameworkCore;
using DataContext;
using Entity.DbSet;
using Managers;
using Protocols.DTOs;
using TableData;

namespace Entity.Repository
{
    public class ItemRepository
    {
        private readonly DbSet<ItemDb> _items;
        private readonly GameDataManager _gameDataManager;
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context, GameDataManager gameDataManager)
        {
            _context = context;
            _items = _context.Set<ItemDb>();
            _gameDataManager = gameDataManager;
        }

        public List<ItemDb> CreateOrSum(AccountDb account, List<ItemAcqDto> items)
        {
            List<ItemDb> result = new List<ItemDb>();

            // account db를 tracking 상태로 변경
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            foreach (ItemAcqDto item in items)
            {
                if (item.Count <= 0)
                    throw new Exception($"[CreateOrSum] Invalid item count: {item.Count}");

                ItemTable? itemTable = _gameDataManager.GetData<ItemTable>(item.ItemCode);
                if (itemTable == null)
                    throw new Exception($"[CreateOrSum] Invalid item code: {item.ItemCode}");

                ItemDb? resultItem;
                if (itemTable.Single_stack == 1)
                {
                    resultItem = Create(account.account_id, item.ItemCode, 1);
                    account.items?.Add(resultItem);
                }
                else
                {
                    // account.items에item.ItemCode가 있는지 확인후 없으면 추가, 있으면 수량만 증가
                    ItemDb? existingItem = account.items?.FirstOrDefault(x => x.item_code == item.ItemCode);
                    if (existingItem == null)
                    {
                        resultItem = Create(account.account_id, item.ItemCode, item.Count);
                        account.items?.Add(resultItem);
                    }
                    else
                    {
                        existingItem.item_count += item.Count;
                        resultItem = existingItem;
                    }
                }

                if (result.Contains(resultItem) == false)
                    result.Add(resultItem);
            }

            return result;
        }

        public List<ItemDb> Equip(AccountDb account, long characterId, long itemId, List<ItemDb> itemsToUnequip)
        {
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            List<ItemDb> updatedItems = new List<ItemDb>();

            // 해제할 아이템 업데이트
            foreach (ItemDb item in itemsToUnequip)
            {
                item.equipped_character_id = null;
                updatedItems.Add(item);
            }

            // 장착할 아이템 업데이트
            var equipItem = account.items?.FirstOrDefault(x => x.item_id == itemId);
            if (equipItem != null)
            {
                equipItem.equipped_character_id = characterId;
                updatedItems.Add(equipItem);
            }

            return updatedItems;
        }

        public ItemDb? Unequip(AccountDb account, long itemId)
        {
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            ItemDb? item = account.items?.FirstOrDefault(x => x.item_id == itemId);
            if (item == null)
                return null;

            item.equipped_character_id = null;
            return item;
        }

        public List<ItemDb> ItemLevelup(AccountDb account, long itemId, int nextLevelCode,  List<Tuple<ItemDb, int>> materials)
        {
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            List<ItemDb> result = new List<ItemDb>();

            ItemDb? targetItem = account.items?.FirstOrDefault(x => x.item_id == itemId);
            if (targetItem == null)
                throw new Exception($"[ItemLevelup] Invalid item id: {itemId}");

            result.AddRange(ItemUse(account, materials));

            targetItem.level_code = nextLevelCode;
            result.Add(targetItem);

            return result;
        }

        public List<ItemDb> ItemUse(AccountDb account, List<Tuple<ItemDb, int>> items)
        {
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            List<ItemDb> result = new List<ItemDb>();
            foreach (var material in items)
            {
                ItemDb? find = account.items?.FirstOrDefault(x => x.item_id == material.Item1.item_id);
                if (find == null || find.item_count < material.Item2)
                {
                    throw new Exception($"[ItemLevelup] Not enough material: {material.Item1.item_id}, Need count: {material.Item2}");
                }

                find.item_count -= material.Item2;

                if (find.item_count == 0)
                    account.items?.Remove(find);

                result.Add(find);
            }

            return result;
        }

        public List<ItemDb> ItemEvolve(AccountDb account, long itemId, int nextEvolveCode, List<Tuple<ItemDb, int>> materials)
        {
            if (_context.Entry(account).State == EntityState.Detached)
                _context.Attach(account);

            List<ItemDb> result = new List<ItemDb>();

            ItemDb? targetItem = account.items?.FirstOrDefault(x => x.item_id == itemId);
            if (targetItem == null)
                throw new Exception($"[ItemEvolve] Invalid item id: {itemId}");

            result.AddRange(ItemUse(account, materials));

            targetItem.evolve_code = nextEvolveCode;
            result.Add(targetItem);

            return result;
        }

        private ItemDb Create(long accountId, int itemCode, int itemCount)
        {
            ItemTable? itemTable = _gameDataManager.GetData<ItemTable>(itemCode);
            if (itemTable == null)
                throw new Exception($"[Create] Invalid item code: {itemCode}");

            ItemDb newItem = new ItemDb
            {
                account_id = accountId,
                item_code = itemCode,
                item_count = itemCount,
                equipped_character_id = null,
                level_code = itemTable.Level_code,
                evolve_code = itemTable.Evolution_code
            };

            return newItem;
        }
    }
}

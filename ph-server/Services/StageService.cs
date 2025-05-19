using DataContext;
using Entity;
using Entity.DbSet;
using Managers;
using Protocols;
using Protocols.DTOs;
using Services.Redis;
using TableData;

namespace Services
{
    public class StageService
    {
        private readonly AppDbContext _context;
        private readonly GameDataManager _gameDataManager;
        private readonly RedisCacheService _redis;
        public StageService(AppDbContext context, RedisCacheService redis, GameDataManager gameDataManager)
        {
            _context = context;
            _gameDataManager = gameDataManager;
            _redis = redis;
        }

        // stage enter시 redis에 stage code 저장
        public async Task<StageEnterRes> StageEnter(string accountId, StageEnterReq req)
        {
            if (req.StageCode <= 0)
                throw new Exception("[StageEnter] Invalid stage id");

            StageEnterRes res = new StageEnterRes();
            UserCache userCache = new UserCache(accountId, _redis);
            AccountDb? account = await userCache.LoadAccount(true);
            if (account == null)
                throw new Exception($"[StageEnter] Account not found: {accountId}");

            await userCache.SaveStageStart(req.StageCode);
            res.CanEnter = true;
            return res; 
        }   

        public async Task<StageClearRes> StageClear(string accountId, StageClearReq req)
        {
            if (req.StageCode <= 0)
                throw new Exception("[StageClear] Invalid stage id");

            StageTable? stageTable = _gameDataManager.GetData<StageTable>(req.StageCode);
            if (stageTable == null)
                throw new Exception($"[StageClear] Invalid stage id: {req.StageCode}");

            // insert item to db
            StageClearRes res = new StageClearRes();
            UserCache userCache = new UserCache(accountId, _redis);
            // redis에 있는 stage start code 조회
            int stageStartCode = await userCache.LoadStageStart();
            if (stageStartCode != req.StageCode)
                throw new Exception($"[StageClear] Invalid stage id: {req.StageCode}");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    AccountDb? account = await userCache.LoadAccount();
                    if (account == null)
                        throw new Exception($"[StageClear] Account not found: {accountId}");

                    await userCache.LoadItems();

                    // stageTable.Reward_items to List<itemAcqDto>
                    List<ItemDb> rewardItems = _context.ItemRepo.CreateOrSum(account, stageTable.Reward_items.Select(itemCode => new ItemAcqDto
                    {
                        ItemCode = itemCode,
                        Count = 1
                    }).ToList());

                    _context.SaveChanges();
                    await transaction.CommitAsync();
                    await userCache.SaveItems();
                    await userCache.RemoveStageStart();
                    
                    res.UpdateData.Items = rewardItems.Select(x => x.ToDto).ToList();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"[StageClear] Error occurred while acquiring item for user: {accountId}\r\nex");
                }
            }
            return res;
        }
    }
}

using Constants;
using Services.Redis;
using Serilog;
using TableData;
using StackExchange.Redis;

namespace Managers
{
    public class GameDataManager
    {
        private readonly object _lock = new object();
        private RedisCacheService _redis;
        private Dictionary<string, Dictionary<int, BaseTable>> _tables = new Dictionary<string, Dictionary<int, BaseTable>>();

        public GameDataManager(RedisCacheService redis)
        {
            _redis = redis;
        }

        public async Task Init()
        {
            var subscriber = _redis.GetConnection().GetSubscriber();
            await subscriber.SubscribeAsync(new RedisChannel($"__keyspace@{RedisDefine.DB_TABLE_DATA}__:*", RedisChannel.PatternMode.Pattern), async (channel, message) =>
            {
                var parts = channel.ToString().Split(':');
                int dbNumber = int.Parse(parts[0].Replace("__keyspace@", "").Replace("__", ""));
                string key = parts[1];
                await HandleRedisMessageAsync(dbNumber, key);
            });

            // 서버 시작 시 모든 테이블 데이터를 가져옴
            string[] keys = _redis.GetKeysAsync(RedisDefine.DB_TABLE_DATA);
            List<Task> tasks = new List<Task>();
            foreach (var key in keys)
            {
                tasks.Add(HandleRedisMessageAsync(RedisDefine.DB_TABLE_DATA, key));
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleRedisMessageAsync(int numOfDb, string tableName)
        {
            // 변경된 데이터 가져오기
            string? jsonData = await _redis.GetCacheAsync(tableName, numOfDb);
            if (jsonData != null && jsonData != string.Empty)
            {
                // JSON 데이터를 객체로 변환
                var table = DataTableGenerate.JsonToObject(tableName, jsonData);
                if (table == null)
                {
                    Log.Error($"Failed to deserialize JSON data: {tableName}");
                    return;
                }

                Log.Information($"Load table key: {tableName}");

                lock (_lock)
                {
                    // 테이블을 딕셔너리에 추가
                    if (_tables.ContainsKey(tableName) == false)
                        _tables.Add(tableName, new Dictionary<int, BaseTable>());

                    _tables[tableName].Clear();

                    foreach (var data in table)
                    {
                        _tables[tableName].Add(data.Code, data);
                    }
                }
            }
        }

        public void WriteTable()
        {
            lock (_lock)
            {
                _tables.Clear();
            }
        }
        
        public List<BaseTable> GetTablesData(string tableName)
        {
            if (_tables.TryGetValue(tableName, out Dictionary<int, BaseTable> table))
            {
                return table.Values.ToList();
            }
            // Log.Warn($"Table with name {tableName} does not exist in _tables.");
            return new List<BaseTable>();
        }

        public List<T> GetDatas<T>()
        {
            string name = typeof(T).Name;
            if (_tables.TryGetValue(name, out Dictionary<int, BaseTable>? table))
            {
                return table.Values.Cast<T>().ToList();
            }

            return new List<T>();
        }

        public T? GetData<T>(int code) where T : BaseTable
        {
            string name = typeof(T).Name;
            if (_tables.TryGetValue(name, out Dictionary<int, BaseTable>? table))
            {
                if (table.TryGetValue(code, out BaseTable? data))
                {
                    return (T)data;
                }
            }

            return default;
        }
    }
}
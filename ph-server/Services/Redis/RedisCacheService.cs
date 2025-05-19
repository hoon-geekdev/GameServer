using StackExchange.Redis;
using Newtonsoft.Json;

namespace Services.Redis
{
    public class RedisCacheService
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisCacheService(string redisConnectionString)
        {
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        public ConnectionMultiplexer GetConnection()
        {
            return _redis;
        }

        private IDatabase GetDatabase(int dbIndex)
        {
            return _redis.GetDatabase(dbIndex);
        }

        public string[] GetKeysAsync(int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            IEnumerable<RedisKey> keys = _redis.GetServer(_redis.GetEndPoints().First()).Keys(db.Database);

            return keys.Select(k => k.ToString()).ToArray();
        }

        // 기본 키-값 저장 (옵션: TTL 설정)
        public async Task SetCacheAsync(string key, string value, int dbIndex, TimeSpan? expiry = null)
        {
            IDatabase db = GetDatabase(dbIndex);
            await db.StringSetAsync(key, value, expiry);
        }

        // 기본 키-값 가져오기
        public async Task<string?> GetCacheAsync(string key, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.StringGetAsync(key);
        }

        // 키 존재 여부 확인
        public async Task<bool> KeyExistsAsync(string key, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.KeyExistsAsync(key);
        }

        // 키 삭제
        public async Task<bool> RemoveCacheAsync(string key, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.KeyDeleteAsync(key);
        }
        
        // Redis Hash에서 삭제
        public async Task RemoveHashCacheAsync(string key, string? field, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);

            if (!string.IsNullOrEmpty(field))
            {
                // 특정 필드를 제거합니다.
                bool isRemoved = await db.HashDeleteAsync(key, field);
                if (!isRemoved)
                {
                    Console.WriteLine($"Failed to remove field '{field}' from key '{key}' in database index '{dbIndex}'");
                }
            }
            else
            {
                // 전체 키를 제거합니다.
                bool isRemoved = await db.KeyDeleteAsync(key);
                if (!isRemoved)
                {
                    Console.WriteLine($"Failed to remove key '{key}' in database index '{dbIndex}'");
                }
            }
        }

        // JSON 형태로 데이터 저장
        public async Task SetCacheAsJsonAsync<T>(string key, T value, int dbIndex, TimeSpan? expiry = null)
        {
            IDatabase db = GetDatabase(dbIndex);
            string jsonData = JsonConvert.SerializeObject(value);
            await db.StringSetAsync(key, jsonData, expiry);
        }

        // JSON 형태로 데이터 가져오기
        public async Task<T?> GetCacheAsJsonAsync<T>(string key, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            RedisValue jsonData = await db.StringGetAsync(key);
            if (jsonData.IsNullOrEmpty)
                return default(T);

            return JsonConvert.DeserializeObject<T>(jsonData.ToString());
        }

        public async Task SetHashCacheAsync<T>(string key, string field, T? value, int dbIndex, TimeSpan? expiry = null)
        {
            IDatabase db = GetDatabase(dbIndex);
            string jsonData = JsonConvert.SerializeObject(value);
            await db.HashSetAsync(key, field, jsonData);

            if (expiry != null)
                await db.KeyExpireAsync(key, expiry);
        }

        public async Task SetHashCacheAsync(string key, string field, string value, int dbIndex, TimeSpan? expiry = null)
        {
            IDatabase db = GetDatabase(dbIndex);
            await db.HashSetAsync(key, field, value);

            if (expiry != null)
                await db.KeyExpireAsync(key, expiry);
        }

        // Redis Hash에서 데이터 조회
        public async Task<T?> GetHashCacheAsync<T>(string key, string field, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            RedisValue jsonData = await db.HashGetAsync(key, field);
            if (jsonData.IsNullOrEmpty)
                return default(T);

            return JsonConvert.DeserializeObject<T>(jsonData.ToString());
        }

        public async Task<string?> GetHashCacheAsync(string key, string field, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.HashGetAsync(key, field);
        }

        // TTL (Time to Live) 설정
        public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.KeyExpireAsync(key, expiry);
        }

        // 남은 TTL 가져오기
        public async Task<TimeSpan?> GetTimeToLiveAsync(string key, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.KeyTimeToLiveAsync(key);
        }

        // 키 값 증가
        public async Task<long> IncrementAsync(string key, long value = 1, int dbIndex = 0)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.StringIncrementAsync(key, value);
        }

        // 키 값 감소
        public async Task<long> DecrementAsync(string key, long value = 1, int dbIndex = 0)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.StringDecrementAsync(key, value);
        }

        // Hash 값 설정
        public async Task SetHashValueAsync(string hashKey, string field, string value, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            await db.HashSetAsync(hashKey, field, value);
        }

        // Hash 값 가져오기
        public async Task<string?> GetHashValueAsync(string hashKey, string field, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            return await db.HashGetAsync(hashKey, field);
        }

        // List 데이터 추가 (왼쪽 삽입)
        public async Task PushToListLeftAsync(string listKey, string value, int dbIndex)
        {
            IDatabase db = GetDatabase(dbIndex);
            await db.ListLeftPushAsync(listKey, value);
        }

        // List 데이터 가져오기
        public async Task<string?[]> GetListAsync(string listKey, long start = 0, long stop = -1, int dbIndex = 0)
        {
            IDatabase db = GetDatabase(dbIndex);
            RedisValue[] values = await db.ListRangeAsync(listKey, start, stop);
            return Array.ConvertAll(values, item => (string?)item);
        }

        // 캐시 전체 삭제
        public async Task FlushAllAsync(int dbIndex)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await server.FlushDatabaseAsync(dbIndex);
        }
    }
}

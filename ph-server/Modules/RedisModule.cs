using Services.Redis;

namespace Modules
{
    public class RedisModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            string? redisConnectionString = configuration.GetConnectionString("Redis");

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            services.AddSingleton<RedisCacheService>(sp => new RedisCacheService(redisConnectionString));
        }
    }
}

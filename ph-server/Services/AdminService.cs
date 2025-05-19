using Constants;
using DataContext;
using Entity;
using Protocols;
using Services.Redis;
using Serilog;

namespace Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;
        private readonly RedisCacheService _redis;

        public AdminService(RedisCacheService redis, AppDbContext context)
        {
            _redis = redis;
            _context = context;
        }

        public async Task TableUpload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return;

            foreach (var file in files)
            {
                try
                {
                    if (file.Length > 0)
                    {
                        using (var streamReader = new StreamReader(file.OpenReadStream()))
                        {
                            var jsonData = await streamReader.ReadToEndAsync();
                            await _redis.SetCacheAsync(file.FileName.Replace(".json", ""), jsonData, RedisDefine.DB_TABLE_DATA);
                        }
                    }
                    else
                    {
                        Log.Warning($@"Failed to upload table: {file.FileName}
    File length: {file.Length}");
                    }
                } 
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to upload table: {file.FileName}");
                }
            }
        }
    }
}

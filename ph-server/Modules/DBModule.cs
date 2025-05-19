using Microsoft.EntityFrameworkCore;
using DataContext;

namespace Modules
{
    public class DBModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            string? mysqlConnectionString = configuration.GetConnectionString("MySql");

            services.AddDbContext<AppDbContext>(options =>
                options
                .UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString)));
        }
    }
}
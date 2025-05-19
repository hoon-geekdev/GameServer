using Managers;

namespace Modules
{
    public class ManagersModule
    {
        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<GameDataManager>();
        }

        public static async Task Initialize(IServiceProvider services)
        {
            var gameDataManager = services.GetRequiredService<GameDataManager>();
            await gameDataManager.Init();
        }
    }
}

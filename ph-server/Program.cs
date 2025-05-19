using Managers;
using Modules;
using Utils;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));

// Serilog 설정
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Async(a => a.Console())
    .WriteTo.Async(a => a.File("logs/log-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

// 모듈별 DI 등록
DBModule.Register(builder.Services, builder.Configuration);
RedisModule.Register(builder.Services, builder.Configuration);
ManagersModule.Register(builder.Services);
ServicesModule.Register(builder.Services, builder.Configuration);

builder.Host.UseSerilog();

// Swagger 및 기타 설정
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 미들웨어 설정
app.UseMiddleware<LoggerMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthGuardMiddleware>();

ServicesModule.ConfigureMiddleware(app);
await ManagersModule.Initialize(app.Services);

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

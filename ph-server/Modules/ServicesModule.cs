using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.Text;

namespace Modules
{
    public class ServicesModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<AccountService>();
            services.AddScoped<AdminService>();
            services.AddScoped<ItemService>();
            services.AddScoped<StageService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            services.AddHttpContextAccessor();
        }

        public static void ConfigureMiddleware(IApplicationBuilder app)
        {
            app.UseCors(builder => builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod());
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/admin/table/upload"))
                {
                    var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                    if (maxRequestBodySizeFeature != null)
                    {
                        maxRequestBodySizeFeature.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
                    }
                }

                await next(context);
            });
        }
    }
}

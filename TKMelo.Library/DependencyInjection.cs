using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TKMelo.Library.Interfaces;
using TKMelo.Library.Options;
using TKMelo.Library.Services;

namespace TKMelo.Library
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLibrary(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<TKMelo.Library.Services.SmtpOptions>(cfg.GetSection("Smtp"));
            services.AddScoped<IEmailSender, TKMelo.Library.Services.SmtpEmailSender>();

            services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
            services.AddScoped<IUserService, UserService>();

            services.Configure<OpenAIOptions>(cfg.GetSection(OpenAIOptions.SectionName));
            services.AddHttpClient<IOpenersService, OpenersService>();

            return services;
        }
    }
}

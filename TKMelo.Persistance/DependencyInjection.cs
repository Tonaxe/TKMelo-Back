using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TKMelo.Persistance.Data;

namespace TKMelo.Persistance
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
        {
            var cs = cfg.GetConnectionString("DefaultConnection")!;

            services.AddDbContext<TKMeloDbContext>(opt =>
                opt.UseNpgsql(cs)
                   .UseSnakeCaseNamingConvention()
            );

            return services;
        }
    }
}

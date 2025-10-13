using TKMelo.Library.Interfaces;
using TKMelo.Library.Services;

namespace TKMelo.Library;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}

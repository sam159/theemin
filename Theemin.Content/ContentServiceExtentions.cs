using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Theemin.Content;

public static class ContentServiceExtentions
{
    public static IServiceCollection AddContentOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<ContentOptions>(configuration.GetSection("Content"));
    }

    public static IServiceCollection AddContentServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ContentReader>()
            .AddSingleton<ContentService>();
    }
}
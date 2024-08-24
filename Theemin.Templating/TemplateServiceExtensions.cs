using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Theemin.Templating;

public static class TemplateServiceExtensions
{
    public static IServiceCollection AddTemplateOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<TemplateOptions>(configuration.GetSection("Template"));
    }

    public static IServiceCollection AddTemplateServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<TemplateParser>(x =>
            {
                var config = x.GetService<IOptions<TemplateOptions>>();
                var parser = new TemplateParser(config != null ? config.Value.Variables : new Dictionary<string, string>());
                return parser;
            })
            .AddSingleton<TemplateCache>()
            .AddSingleton<TemplateService>();
    }
}
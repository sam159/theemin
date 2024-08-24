using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Theemin.Templating;

public class TemplateService
{
    private readonly TemplateParser _templateParser;
    private readonly TemplateOptions _options;
    private readonly TemplateCache _cache;
    private readonly IWebHostEnvironment _environment;
    private readonly bool _canCache;

    public TemplateService(TemplateParser templateParser, IOptions<TemplateOptions> options, TemplateCache cache, IWebHostEnvironment environment)
    {
        _templateParser = templateParser;
        _options = options.Value;
        _cache = cache;
        _environment = environment;
        _canCache = !environment.IsDevelopment();
    }
    
    public Template? Get(params string[] path)
    {
        // do some sanity checks
        path = path.Select(x => x.Trim()).Where(x => x != "." && x != "..").ToArray();
        
        // use default page if no path
        if (path.Length == 0)
        {
            path = [.._options.Default.Split(Path.PathSeparator)];
        }
        
        var dirCheck =  _environment.ContentRootFileProvider.GetFileInfo(Path.Join([_options.Root, ..path]));
        if (dirCheck is { Exists: true, IsDirectory: true })
        {
            path = [..path, "index"];
        }
        
        // append default extension if missing
        if (!path[^1].Contains('.'))
        {
            path[^1] += $".{_options.DefaultExtension}";
        }
        
        var name = Path.Join([_options.Root, ..path]);
        
        if (_cache.Has(name))
        {
            return _cache.Get(name);
        }

        var file = _environment.ContentRootFileProvider.GetFileInfo(name);

        if (!file.Exists)
        {
            return null;
        }
        
        using var reader = file.CreateReadStream();
        
        var nodes = _templateParser.Parse(reader);
        var template = new Template(name, nodes);

        if (_canCache)
        {
            _cache.Add(template);
        }

        return template;
    }
}
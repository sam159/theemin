using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Theemin.Templating;

namespace Theemin.Content;

public class ContentService
{
    private readonly ContentReader _reader;
    private readonly ContentOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly TemplateService _templateService;

    public ContentService(ContentReader reader, IOptions<ContentOptions> options, IWebHostEnvironment environment, TemplateService templateService)
    {
        _reader = reader;
        _options = options.Value;
        _environment = environment;
        _templateService = templateService;
    }

    public ContentPage? Get(params string[] path)
    {
        // do some sanity checks
        path = path.Select(x => x.Trim()).Where(x => x != "." && x != "..").ToArray();

        // use default page if no path
        if (path.Length == 0)
        {
            path = [.._options.DefaultPage.Split(Path.PathSeparator)];
        }

        // return null if no default
        if (path.Length == 0)
        {
            return null;
        }

        var dirCheck =  _environment.ContentRootFileProvider.GetFileInfo(Path.Join([_options.Root, ..path]));
        if (dirCheck is { Exists: true, IsDirectory: true })
        {
            path = [..path, "index"];
        }
        
        var name = Path.Join(path);

        IFileInfo? file = null;
        var tryExtensions = new [] { ".html", ".md" };

        foreach (var extension in tryExtensions)
        {
            var fileName = $"{path[^1]}{extension}";
            var tryPath = path.Length > 1 ? path[..^2] : [];
            file = _environment.ContentRootFileProvider.GetFileInfo(Path.Join([_options.Root, ..tryPath, fileName]));
            if (file is { Exists: true, IsDirectory: false })
            {
                name += $"{extension}";
                break;
            }

            file = null;
        }

        if (file == null)
        {
            return null;
        }
        
        using var fileStream = file.CreateReadStream();
        
        var page = _reader.ReadPage(name, fileStream);
        page.LastModified = file.LastModified;
        if (string.IsNullOrEmpty(page.Data.Title))
        {
            page.Data.Title = Path.GetFileNameWithoutExtension(Path.Join(path));
        }
        if (string.IsNullOrEmpty(page.Data.Layout))
        {
            page.Data.Layout = _options.DefaultLayout;
        }

        return page;
    }

    public string? Render(ContentPage page)
    {
        if (page.Data.Layout == null)
        {
            return Renderer.RenderTemplate(page.Content, _templateService, page.Data.Variables);
        }
        
        var template = _templateService.Get(page.Data.Layout);
        if (template == null)
        {
            return Renderer.RenderTemplate(page.Content, _templateService, page.Data.Variables);
        }

        var renderer = new PageRenderer(template, _templateService);
        return renderer.Render(page);
    }
    
    public ContentRenderResult Render(bool use404, params string[] path)
    {
        var page = Get(path);
        return page != null 
            ? new ContentRenderResult(true, page.LastModified, Render(page)) 
            : new ContentRenderResult(false, null, use404 ? Render404() : null);
    }

    public string? Render404()
    {
        var page = Get(_options.NotFoundErrorPage);
        return page != null ? Render(page) : null;
    }

    public string? Render500()
    {
        var page = Get(_options.ServerErrorPage);
        return page != null ? Render(page) : null;
    }
}
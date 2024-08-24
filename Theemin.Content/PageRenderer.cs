using Theemin.Templating;

namespace Theemin.Content;

public class PageRenderer
{
    private readonly Renderer _renderer;
    private readonly Template _layout;

    public PageRenderer(Template layout, TemplateService templateService)
    {
        _renderer = new Renderer(templateService);
        _layout = layout;
    }

    public string Render(ContentPage page)
    {
        var variables = new Dictionary<string, string>(page.Data.Variables)
        {
            { "PageContent", _renderer.Render(page.Content, page.Data.Variables) }
        };
        if (page.Data.Title != null)
        {
            variables.Add("PageTitle", page.Data.Title);
        }

        return _renderer.Render(_layout, variables);
    }
}
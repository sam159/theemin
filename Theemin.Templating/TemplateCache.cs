namespace Theemin.Templating;

public class TemplateCache
{
    private readonly Dictionary<string, Template> _templates = new();

    public bool Has(string name)
    {
        return _templates.ContainsKey(name);
    }

    public void Add(Template template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _templates[template.Name] = template;
    }

    public Template Get(string name)
    {
        if (Has(name))
        {
            return _templates[name];
        }

        throw new InvalidOperationException("template is not in cache");
    }
}
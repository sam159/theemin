namespace Theemin.Templating;

public record TemplateOptions
{
    public string Root { get; set; } = string.Empty;
    public string Default { get; set; } = string.Empty;
    
    public string DefaultExtension { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

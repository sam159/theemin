using System.Collections;
using System.Text;

namespace Theemin.Templating;

public class Renderer
{
    public static string RenderTemplate(Template template, TemplateService templateService, IDictionary<string, string> variables)
    {
        return new Renderer(templateService).Render(template, variables);
    }
    
    private readonly TemplateService _templateService;

    public Renderer(TemplateService templateService)
    {
        _templateService = templateService;
    }

    public string Render(Template template, IDictionary<string, string> variables)
    {
        var result = new StringBuilder();

        foreach (var node in template.Nodes)
        {
            switch (node.Type)
            {
                case NodeType.None:
                    break;
                case NodeType.Content:
                    result.Append(node.Content);
                    break;
                case NodeType.Variable:
                    if (variables.ContainsKey(node.Content))
                    {
                        result.Append(variables[node.Content]);
                    }
                    break;
                case NodeType.IncludeTemplate:
                    var includedTemplate = _templateService.Get(node.Content);
                    if (includedTemplate == null)
                    {
                        throw new TemplateException($"Could not find template '{node.Content}' to include in '{template.Name}'");
                    }
                    
                    result.Append(Render(includedTemplate, variables));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node.Type), $"Unknown node type: {Enum.GetName(node.Type)}");
            }
        }
        
        return result.ToString();
    }
}
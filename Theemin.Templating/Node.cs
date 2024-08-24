namespace Theemin.Templating;

public record Node
{
    public NodeType Type { get; init; }
    public string Content { get; init; }
    
    public Node(NodeType type, string content)
    {
        Type = type;
        Content = content;
    }
}

namespace Theemin.Content;

public record PageData
{
    public PageData()
    { }
    public PageData(string? layout, Dictionary<string, string> variables)
    {
        Layout = layout;
        Variables = variables;
    }

    public string? Layout { get; set; } = string.Empty;
    public string? Title { get; set; } = null;
    public Dictionary<string, string> Variables { get; init; } = new();
}
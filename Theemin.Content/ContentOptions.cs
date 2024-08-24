namespace Theemin.Content;

public record ContentOptions
{
    public required string Root { get; init; }
    
    public required string DefaultPage { get; init; }
    
    public string? DefaultLayout { get; init; }

    public string NotFoundErrorPage { get; init; } = "error/404.md";
    
    public string ServerErrorPage { get; init; } = "error/500.md";
}
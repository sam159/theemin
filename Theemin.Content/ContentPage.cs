using Theemin.Templating;

namespace Theemin.Content;

public record ContentPage
{
    public required string Name { get; init; }
    public DateTimeOffset? LastModified { get; set; }
    public required PageData Data { get; init; }
    public required Template Content { get; init; }
}

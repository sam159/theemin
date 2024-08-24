namespace Theemin.Content;

public record ContentRenderResult(bool Found, DateTimeOffset? LastModified, string? Content);

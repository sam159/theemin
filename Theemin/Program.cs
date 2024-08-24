using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Theemin.Content;
using Theemin.Templating;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTemplateOptions(builder.Configuration)
    .AddContentOptions(builder.Configuration)
    .AddTemplateServices()
    .AddContentServices()
    .AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
}
app.MapHealthChecks("/healthz").RequireHost("localhost", "127.0.0.1");

const string fallbackErrorHtml = "<html><head><title>Server Error</title></head><body><h1>Server Error</h1></body></html>";
const string fallbackNotFoundHtml = "<html><head><title>Not Found</title></head><body><h1>Not Found</h1></body></html>";

app.MapGet("/", (ContentService content, HttpContext ctx) => GetResult(content.Render(true), ctx));
app.MapGet("/error/500", (ContentService content) => Results.Content(
    content.Render500() ?? fallbackErrorHtml,
    "text/html",
    Encoding.UTF8,
    500
));
app.MapFallback("/{*path}",
    async (string path, ContentService content, HttpContext ctx, IContentTypeProvider typeProvider) =>
    {
        var staticFile = app.Environment.WebRootFileProvider.GetFileInfo(path);
        if (!staticFile.Exists || staticFile.IsDirectory)
        {
            var result = content.Render(true, path.Split('/'));
            await GetResult(result, ctx).ExecuteAsync(ctx);
        }
        else if (staticFile.PhysicalPath != null)
        {
            if (typeProvider.TryGetContentType(staticFile.PhysicalPath, out var staticType))
            {
                await Results.File(staticFile.PhysicalPath, staticType, null, staticFile.LastModified, null, true)
                    .ExecuteAsync(ctx);
            }
            else
            {
                await Results.Content(content.Render404() ?? fallbackNotFoundHtml, "text/html", Encoding.UTF8, 404)
                    .ExecuteAsync(ctx);
            }
        }
    }
);

app.Run();
return;

IResult GetResult(ContentRenderResult result, HttpContext context)
{
    if (result.Found)
    {
        if (result.LastModified.HasValue)
        {
            context.Response.Headers.LastModified = HeaderUtilities.FormatDate(result.LastModified.Value);
        }

        return Results.Content(result.Content, "text/html", Encoding.UTF8, 200);
    }

    return Results.Content(result.Content ?? fallbackNotFoundHtml, "text/html", Encoding.UTF8, 404);
}
using System.Text;
using Markdig;
using Theemin.Templating;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Theemin.Content;

public class ContentReader
{
    public ContentPage ReadPage(string name, Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException("stream is not readable", nameof(stream));
        }

        using var sr = new StreamReader(stream, Encoding.UTF8);

        var state = ContentReaderState.Start;
        var frontMatterContent = new StringBuilder();
        var content = new StringBuilder();

        while (sr.ReadLine() is { } line)
        {
            switch (state)
            {
                case ContentReaderState.Start:
                    if (line.Trim() == "---")
                    {
                        state = ContentReaderState.FrontMatter;
                    }
                    else
                    {
                        state = ContentReaderState.Content;
                        content.AppendLine(line);
                    }
                    break;
                case ContentReaderState.FrontMatter:
                    if (line.Trim() == "---")
                    {
                        state = ContentReaderState.Content;
                    }
                    else
                    {
                        frontMatterContent.AppendLine(line);
                    }
                    break;
                case ContentReaderState.Content:
                    content.AppendLine(line);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        var frontMatter = ParseFrontMatter(frontMatterContent.ToString());

        var parser = new TemplateParser(frontMatter.Variables);

        var pageContent = content.ToString();
        // parse markdown if a name ends in .md
        if (name.EndsWith(".md"))
        {
            pageContent = Markdown.ToHtml(pageContent);
        }

        using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(pageContent));
        var contentTemplate = new Template(name, parser.Parse(contentStream));
        
        return new ContentPage
        {
            Name = name,
            Data = frontMatter,
            Content = contentTemplate
        };
    }

    private static PageData ParseFrontMatter(string content)
    {
        if (content.Length == 0)
        {
            return new PageData(null, new Dictionary<string, string>());
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<PageData>(content);
    }
}
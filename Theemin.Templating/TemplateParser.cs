using System.Text;

namespace Theemin.Templating;

public class TemplateParser
{
    private readonly Dictionary<string, string> _presetContent = new();

    public TemplateParser(IDictionary<string, string>? presetContent = null)
    {
        if (presetContent == null) return;
        foreach (var x in presetContent)
        {
            _presetContent.Add(x.Key, x.Value);
        }
    }

    public IList<Node> Parse(Stream reader)
    {
        if (!reader.CanRead)
        {
            throw new ArgumentException("stream is not readable", nameof(reader));
        }

        var nodes = new List<Node>();

        var line = 1;
        var position = 1;
        // decoder for streaming decode of utf8
        var decoder = Encoding.UTF8.GetDecoder();
        // next char that was decoded from the stream
        var nextChar = new char[1];
        // next byte/int from the stream
        int nextByte;
        // the previous char decoded, to detect escape chars
        char? lastChar = null;

        var state = ParserState.Content;
        var currentContent = new StringBuilder();

        using var sr = new StreamReader(reader, Encoding.UTF8);

        while ((nextByte = sr.Read()) != -1)
        {
            // try and decode the next char with this byte
            var decodedCount = decoder.GetChars([(byte)nextByte], 0, 1, nextChar, 0);
            if (decodedCount < 1)
            {
                continue;
            }

            var c = nextChar[0];
            currentContent.Append(c);

            // maintain a record of where we are for error reporting
            if (c == '\n')
            {
                line++;
                position = 1;
            }
            else
            {
                position++;
            }

            switch (state)
            {
                case ParserState.Content:
                    if (c == '{')
                    {
                        if (lastChar == '\\')
                        {
                            // opening was escaped, delete escape char
                            currentContent.Length -= 2;
                            currentContent.Append('{');
                        }
                        else
                        {
                            state = ParserState.VariableStart;
                        }
                    }

                    break;
                case ParserState.VariableStart:
                    if (c == '{')
                    {
                        // remove opening {{
                        currentContent.Length -= 2;
                        nodes.Add(new Node(NodeType.Content, currentContent.ToString()));
                        currentContent.Clear();
                        state = ParserState.Variable;
                    }
                    else
                    {
                        // was just a singular {
                        state = ParserState.Content;
                    }

                    break;
                case ParserState.Variable:
                    if (c == '}')
                    {
                        state = ParserState.VariableEnd;
                    }

                    break;
                case ParserState.VariableEnd:
                    if (c == '}')
                    {
                        // remove }}
                        currentContent.Length -= 2;

                        // add as content if matching a preset
                        var content = currentContent.ToString().Trim();

                        if (content.StartsWith('>'))
                        {
                            // template inclusion
                            nodes.Add(new Node(NodeType.IncludeTemplate, content[1..]));
                        }
                        else
                        {
                            nodes.Add(
                                _presetContent.TryGetValue(content, out var preset)
                                    ? new Node(NodeType.Content, preset)
                                    : new Node(NodeType.Variable, currentContent.ToString())
                            );
                        }

                        currentContent.Clear();
                        state = ParserState.Content;
                    }
                    else
                    {
                        throw new ParseException("expected closing bracket", line, position);
                    }

                    break;
                default:
                    throw new ParseException("invalid state", line, position);
            }

            lastChar = c;
        }

        switch (state)
        {
            // add any remaining content
            case ParserState.Content:
            case ParserState.VariableStart:
                if (currentContent.Length > 0)
                {
                    nodes.Add(new Node(NodeType.Content, currentContent.ToString()));
                }
                break;

            // variables must be closed
            case ParserState.Variable:
                throw new ParseException("expected closing }}", line, position);
            case ParserState.VariableEnd:
                throw new ParseException("expected closing }}", line, position);
            default:
                throw new ParseException("invalid state", line, position);
        }

        return nodes;
    }
}
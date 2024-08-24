using System.Net.Mime;

namespace Theemin.Templating;

public enum ParserState
{
    Content,
    VariableStart,
    Variable,
    VariableEnd
}

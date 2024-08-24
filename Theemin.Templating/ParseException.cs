namespace Theemin.Templating;

public class ParseException : Exception
{
    public ParseException(string message, int line, int position) : base($"Parse error line {line} pos {position}: {message}")
    { }
}
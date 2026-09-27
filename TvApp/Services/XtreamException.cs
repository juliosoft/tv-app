namespace TvApp.Services;

public class XtreamException : Exception
{
    public XtreamException(string message) : base(message) { }
    public XtreamException(string message, Exception inner) : base(message, inner) { }
}

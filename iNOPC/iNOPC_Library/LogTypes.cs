namespace iNOPC.Library
{
    public delegate void LogEvent(string text, LogType type = LogType.REGULAR);

    public delegate void UpdateEvent();

    public delegate void WinLogEvent(string text);

    public enum LogType
    {
        REGULAR,
        DETAILED,
        WARNING,
        ERROR,
    }
}
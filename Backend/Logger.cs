using log4net;

public static class Logger
{
    private static readonly ILog _instance = LogManager.GetLogger("GlobalLogger");

    public static ILog Instance => _instance;
}

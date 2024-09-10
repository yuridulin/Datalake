using Microsoft.Extensions.Logging;

namespace Datalake.Database.Utilities;

public static class LogManager
{
	private static ILoggerFactory _loggerFactory;

	static LogManager()
	{
		_loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
		});
	}

	public static ILogger CreateLogger<T>()
	{
		return _loggerFactory.CreateLogger<T>();
	}
}

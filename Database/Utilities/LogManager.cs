using Microsoft.Extensions.Logging;

namespace Datalake.Database.Utilities;

public static class LogManager
{
	public static readonly ILoggerFactory MainLoggerFactory = LoggerFactory.Create(builder =>
	{
		builder
			.AddConsole()
#if DEBUG
			.AddDebug().AddFilter(null, LogLevel.Trace)
#elif RELEASE
			.AddFilter("LinqToDB.Data.DataConnection", LogLevel.Warning)
			.AddFilter("LinqToDB.Data.DataConnection", LogLevel.Warning)
#endif
			;
	});

	public static ILogger<T> CreateLogger<T>()
	{
		return MainLoggerFactory.CreateLogger<T>();
	}
}

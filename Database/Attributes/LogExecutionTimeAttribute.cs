using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Datalake.Database.Attributes;

/// <summary>
/// Класс замера затрачиваемого на выполнение времени
/// </summary>
public static class Measures
{
	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа
	/// </summary>
	public static T Measure<T>(Func<T> action, ILogger logger, string methodName)
	{
		var sw = Stopwatch.StartNew();
		T result = action();
		sw.Stop();

		logger.LogDebug("[Timing] {Method} took {Elapsed} ms", methodName, sw.ElapsedMilliseconds);
		return result;
	}

	/// <summary>
	/// Замер времени, которое тратится на вызов методов
	/// </summary>
	public static void Measure(Action action, ILogger logger, string methodName)
	{
		var sw = Stopwatch.StartNew();
		action.Invoke();
		sw.Stop();

		logger.LogDebug("[Timing] {Method} took {Elapsed} ms", methodName, sw.ElapsedMilliseconds);
	}
}

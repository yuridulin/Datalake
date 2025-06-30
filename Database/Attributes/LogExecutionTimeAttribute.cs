using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Datalake.Database.Attributes;

/// <summary>
/// 
/// </summary>
public static class Measures
{
	/// <summary>
	/// Замер времени, которое тратится на вызов методов
	/// </summary>
	public static T Measure<T>(Func<T> action, ILogger logger, string methodName)
	{
		var sw = Stopwatch.StartNew();
		T result = action();
		sw.Stop();

		logger.LogInformation("[Timing] {Method} took {Elapsed} ms", methodName, sw.ElapsedMilliseconds);
		return result;
	}
}


/*[PSerializable]
public class LogExecutionTimeAttribute : OnMethodBoundaryAspect
{
	[NonSerialized]
	private Stopwatch? _sw;

	[NonSerialized]
	private ILogger? _logger;

	/// <inheritdoc/>
	public override void RuntimeInitialize(System.Reflection.MethodBase method)
	{
		var factory = DatalakeContext.LoggerFactory;
		if (factory != null)
		{
			var loggerType = method.DeclaringType ?? typeof(object);
			_logger = factory.CreateLogger(loggerType);
		}
	}

	/// <inheritdoc/>
	public override void OnEntry(MethodExecutionArgs args)
	{
		_sw = Stopwatch.StartNew();
	}

	/// <inheritdoc/>
	public override void OnExit(MethodExecutionArgs args)
	{
		_sw?.Stop();
		if (_sw == null)
			return;

		if (_logger != null)
		{
			var method = $"{args.Method.DeclaringType?.Name}.{args.Method.Name}";
			_logger.LogInformation("[Timing] {Method} took {Elapsed} ms", method, _sw.ElapsedMilliseconds);
		}

		Console.WriteLine($"[Timing] {args.Method.DeclaringType?.Name}.{args.Method.Name} took {_sw.ElapsedMilliseconds} ms");
	}
}*/


using Microsoft.Extensions.Logging;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System.Diagnostics;

namespace Datalake.Database.Attributes;

/// <summary>
/// Замер времени, которое тратится на вызов методов
/// </summary>
[PSerializable]
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
		if (_logger != null && _sw != null)
		{
			var method = $"{args.Method.DeclaringType?.Name}.{args.Method.Name}";
			_logger.LogInformation("[Timing] {Method} took {Elapsed} ms", method, _sw.ElapsedMilliseconds);
		}
	}
}


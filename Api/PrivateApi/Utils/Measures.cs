using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Utils;

/// <summary>
/// Класс замера затрачиваемого на выполнение времени
/// </summary>
public static class Measures
{
	const string OperationName = "function";

	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа
	/// </summary>
	public static T Measure<T>(Func<T> action, ILogger logger, string methodName, string operation = OperationName, object? parameters = null)
	{
		var parent = SentrySdk.GetSpan();
		var span = parent is not null
			? parent.StartChild(operation, methodName)           // дочерний спан
			: SentrySdk.StartTransaction(methodName, operation); // корневая транзакция

		if (parameters != null)
			span.SetData($"parameters", parameters);

		try
		{
			T result = action();

			span.Finish(SpanStatus.Ok);
			logger.LogDebug("Выполнение метода {Method}: {Elapsed} мс", methodName, Duration(span));

			return result;
		}
		catch (Exception ex)
		{
			span.Finish(SpanStatus.InternalError);
			logger.LogError(ex, "Ошибка в методе {Method}: {Elapsed} мс", methodName, Duration(span));

			throw;
		}
	}

	/// <summary>
	/// Замер времени, которое тратится на вызов методов
	/// </summary>
	public static void Measure(Action action, ILogger logger, string methodName, string operation = OperationName, object? parameters = null) =>
		Measure<object?>(() =>
		{
			action();
			return null;
		}, logger, methodName, operation, parameters);

	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа, асинхронный
	/// </summary>
	public static async Task<T> MeasureAsync<T>(Func<Task<T>> action, ILogger logger, string methodName, string operation = OperationName, object? parameters = null)
	{
		var parent = SentrySdk.GetSpan();
		var span = parent is not null
			? parent.StartChild(operation, methodName)
			: SentrySdk.StartTransaction(methodName, operation);

		if (parameters != null)
			span.SetData($"parameters", parameters);

		try
		{
			var result = await action()/*.ConfigureAwait(false)*/;

			span.Finish(SpanStatus.Ok);
			logger.LogDebug("Выполнение метода {Method}: {Elapsed} мс", methodName, Duration(span));

			return result;
		}
		catch (Exception ex)
		{
			span.Finish(SpanStatus.InternalError);
			logger.LogError(ex, "Ошибка в методе {Method}: {Elapsed} мс", methodName, Duration(span));

			throw;
		}
	}

	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа, асинхронный
	/// </summary>
	public static Task MeasureAsync(Func<Task> action, ILogger logger, string methodName, string operation = OperationName, object? parameters = null)
		=> MeasureAsync<object?>(async () =>
		{
			await action().ConfigureAwait(false);
			return null;
		}, logger, methodName, operation, parameters);


	/// <summary>
	/// Получение времени выполнения спана
	/// </summary>
	/// <param name="span">Спан</param>
	/// <returns>Количество миллисекунд</returns>
	static double Duration(ISpan span)
	{
		var duration = (span.EndTimestamp ?? span.StartTimestamp) - span.StartTimestamp;
		return duration.TotalMilliseconds;
	}
}

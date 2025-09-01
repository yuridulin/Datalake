using Datalake.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.Functions;

/// <summary>
/// Класс замера затрачиваемого на выполнение времени
/// </summary>
public static class Measures
{
	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа
	/// </summary>
	public static T Measure<T>(Func<T> action, ILogger logger, string methodName, string operationName = "function")
	{
		var parent = SentrySdk.GetSpan();
		var span = parent is not null
			? parent.StartChild(operationName, methodName)           // дочерний спан
			: SentrySdk.StartTransaction(methodName, operationName); // корневая транзакция

		try
		{
			T result = action();

			span.Finish(SpanStatus.Ok);
			logger.LogDebug("Выполнение метода {Method}: {Elapsed} мс", methodName, span.Duration());

			return result;
		}
		catch (Exception ex)
		{
			span.Finish(SpanStatus.InternalError);
			logger.LogError(ex, "Ошибка в методе {Method}: {Elapsed} мс", methodName, span.Duration());

			throw;
		}
	}

	/// <summary>
	/// Замер времени, которое тратится на вызов методов
	/// </summary>
	public static void Measure(Action action, ILogger logger, string methodName, string operationName = "function") => 
		Measure<object?>(() =>
		{
			action();
			return null;
		}, logger, methodName, operationName);

	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа, асинхронный
	/// </summary>
	public static async Task<T> MeasureAsync<T>(Func<Task<T>> action, ILogger logger, string methodName, string operationName = "function")
	{
		var parent = SentrySdk.GetSpan();
		var span = parent is not null
			? parent.StartChild(operationName, methodName)
			: SentrySdk.StartTransaction(methodName, operationName);

		try
		{
			var result = await action().ConfigureAwait(false);

			span.Finish(SpanStatus.Ok);
			logger.LogDebug("Выполнение метода {Method}: {Elapsed} мс", methodName, span.Duration());

			return result;
		}
		catch (Exception ex)
		{
			span.Finish(SpanStatus.InternalError);
			logger.LogError(ex, "Ошибка в методе {Method}: {Elapsed} мс", methodName, span.Duration());

			throw;
		}
	}

	/// <summary>
	/// Замер времени, которое тратится на вызов методов, с возвратом ответа, асинхронный
	/// </summary>
	public static Task MeasureAsync(Func<Task> action, ILogger logger, string methodName, string operationName = "function")
		=> MeasureAsync<object?>(async () =>
		{
			await action().ConfigureAwait(false);
			return null;
		}, logger, methodName, operationName);
}

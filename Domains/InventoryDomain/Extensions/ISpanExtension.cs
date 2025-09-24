namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение работы со спанами Sentry
/// </summary>
public static class ISpanExtension
{
	/// <summary>
	/// Получение времени выполнения спана
	/// </summary>
	/// <param name="span">Спан</param>
	/// <returns>Количество миллисекунд</returns>
	public static double Duration(this ISpan span)
	{
		var duration = (span.EndTimestamp ?? span.StartTimestamp) - span.StartTimestamp;
		return duration.TotalMilliseconds;
	}
}

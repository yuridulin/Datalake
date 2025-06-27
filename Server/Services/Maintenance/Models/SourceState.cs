using System.ComponentModel.DataAnnotations;

namespace Datalake.Server.Services.Maintenance.Models;

/// <summary>
/// Объект состояния источника
/// </summary>
public class SourceState
{
	/// <summary>
	/// Идентификатор источника
	/// </summary>
	[Required]
	public required int SourceId { get; init; }

	/// <summary>
	/// Дата последней попытки подключиться
	/// </summary>
	[Required]
	public required DateTime? LastTry { get; init; }

	/// <summary>
	/// Дата последнего удачного подключения
	/// </summary>
	public required DateTime? LastConnection { get; init; }

	/// <summary>
	/// Было ли соединение при последнем подключении
	/// </summary>
	[Required]
	public required bool IsConnected { get; init; }

	/// <summary>
	/// Была ли попытка установить соединение
	/// </summary>
	[Required]
	public required bool IsTryConnected { get; init; }

	/// <summary>
	/// Список количества секунд с момента записи каждого тега
	/// </summary>
	[Required]
	public required double[] ValuesAfterWriteSeconds { get; init; }
}
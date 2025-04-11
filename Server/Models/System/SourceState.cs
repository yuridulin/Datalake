using System.ComponentModel.DataAnnotations;

namespace Datalake.Server.Models.System;

/// <summary>
/// Объект состояния источника
/// </summary>
public class SourceState
{
	/// <summary>
	/// Идентификатор источника
	/// </summary>
	[Required]
	public required int SourceId { get; set; }

	/// <summary>
	/// Дата последней попытки подключиться
	/// </summary>
	[Required]
	public DateTime LastTry { get; set; } = DateTime.MinValue;

	/// <summary>
	/// Дата последнего удачного подключения
	/// </summary>
	public DateTime? LastConnection { get; set; } = null;

	/// <summary>
	/// Было ли соединение при последнем подключении
	/// </summary>
	[Required]
	public bool IsConnected { get; set; } = false;

	/// <summary>
	/// Была ли попытка установить соединение
	/// </summary>
	[Required]
	public bool IsTryConnected { get; set; } = false;

	/// <summary>
	/// Список количества секунд с момента записи каждого тега
	/// </summary>
	[Required]
	public int[] ValuesAfterWriteSeconds { get; set; } = [];
}
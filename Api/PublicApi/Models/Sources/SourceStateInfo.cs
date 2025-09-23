using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Sources;

/// <summary>
/// Объект состояния источника
/// </summary>
public class SourceStateInfo
{
	/// <summary>
	/// Идентификатор источника
	/// </summary>
	[Required]
	public int SourceId { get; init; }

	/// <summary>
	/// Дата последней попытки подключиться
	/// </summary>
	[Required]
	public DateTime? LastTry { get; init; }

	/// <summary>
	/// Дата последнего удачного подключения
	/// </summary>
	public DateTime? LastConnection { get; init; }

	/// <summary>
	/// Было ли соединение при последнем подключении
	/// </summary>
	[Required]
	public bool IsConnected { get; init; }

	/// <summary>
	/// Была ли попытка установить соединение
	/// </summary>
	[Required]
	public bool IsTryConnected { get; init; }

	/// <summary>
	/// Список количества тегов этого источника
	/// </summary>
	[Required]
	public int ValuesAll { get; set; } = 0;

	/// <summary>
	/// Список количества тегов, которые обновлены за последние полчаса
	/// </summary>
	[Required]
	public int ValuesLastHalfHour { get; set; } = 0;

	/// <summary>
	/// Список количества тегов, которые обновлены за последние сутки
	/// </summary>
	[Required]
	public int ValuesLastDay { get; set; } = 0;
}
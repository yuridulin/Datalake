using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.States;
using System.Collections.Concurrent;

namespace Datalake.DataService.Services.Metrics;

/// <summary>
/// Хранение последних ошибок по получению значений
/// </summary>
[Singleton]
public class TagsReceiveStateService
{
	private ConcurrentDictionary<int, TagReceiveState> _state = [];

	/// <summary>
	/// Получение состояния по выбранным тегам
	/// </summary>
	/// <param name="identifiers">Идентификаторы тегов</param>
	/// <returns>Ошибки, сопоставленные с идентификаторами тегов</returns>
	public Dictionary<int, TagReceiveState?> Get(int[]? identifiers)
	{
		if (identifiers?.Length > 0)
			return identifiers
				.ToDictionary(x => x, x => _state.TryGetValue(x, out var value) ? value : null);

		return _state.ToDictionary()!;
	}

	/// <summary>
	/// Сохранение состояния расчета по выбранному тегу
	/// </summary>
	/// <param name="identifier">Идентификатор тега</param>
	/// <param name="value">Состояние ошибки</param>
	public void Set(int identifier, string? value)
	{
		_state.AddOrUpdate(
			identifier,
			(id) => new(DateFormats.GetCurrentDateTime(), value),
			(id, existing) => existing with {
				Date = existing.Message == value ? existing.Date : DateFormats.GetCurrentDateTime(),
				Message = value,
			});
	}
}

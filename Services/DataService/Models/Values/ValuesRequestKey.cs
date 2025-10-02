using Datalake.Contracts.Public.Enums;

namespace Datalake.PublicApi.Models.Values;

/// <summary>
/// Уникальная подпись для метрики запроса к данным
/// </summary>
/// <param name="RequestKey">Ключ запроса</param>
/// <param name="Tags">Список идентификаторов тегов</param>
/// <param name="TagsId">Список глобальных идентификаторов тегов</param>
/// <param name="Old">Дата начала диапазона</param>
/// <param name="Young">Дата конца диапазона</param>
/// <param name="Exact">Дата среза</param>
/// <param name="Resolution">Шаг</param>
/// <param name="Func">Агрегирующая функция</param>
public readonly record struct ValuesRequestKey(
	string RequestKey,
	Guid[] Tags,
	int[]? TagsId,
	DateTime? Old,
	DateTime? Young,
	DateTime? Exact,
	TagResolution Resolution,
	AggregationFunc Func);
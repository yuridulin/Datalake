using Datalake.DataService.Database.Entities;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Фабрика архивных значений
/// </summary>
public interface ITagHistoryFactory
{
	/// <summary>
	/// Создание архивного значения из общего запроса на запись
	/// </summary>
	/// <param name="tag">Информация о теге</param>
	/// <param name="request">Данные для записи</param>
	/// <returns>Объект значения</returns>
	TagHistory CreateFrom(TagCacheInfo tag, ValueWriteRequest request);

	/// <summary>
	/// Создание архивного значения из проверенного запроса на запись
	/// </summary>
	/// <param name="request">Данные для записи после проверки, содержащие информацию о теге</param>
	/// <returns>Объект значения</returns>
	TagHistory CreateFrom(ValueTrustedWriteRequest request);
}
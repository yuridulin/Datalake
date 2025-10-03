using Datalake.Data.Host.Database.Entities;
using Datalake.Data.Host.Models.Values;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.Data.Host.Abstractions;

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
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Хранилище информации о существующих внешних источниках данных
/// </summary>
public interface ISourcesStore
{
	/// <summary>
	/// Получение списка информации о источниках данных
	/// </summary>
	/// <returns></returns>
	IReadOnlyCollection<SourceInfo> GetAll();

	/// <summary>
	/// Получение информации о источнике данных
	/// </summary>
	/// <param name="id">Идентификатор источника данных</param>
	/// <returns>Информация о источнике данных</returns>
	SourceInfo? TryGet(int id);

	/// <summary>
	/// Обновление информации о источниках данных
	/// </summary>
	/// <param name="newSources">Новая информация</param>
	void Update(IEnumerable<SourceInfo> newSources);
}
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Получение нужного сборщика данных для выбранного источника
/// </summary>
public interface ICollectorFactory
{
	/// <summary>
	/// Получение сборщика для источника
	/// </summary>
	/// <param name="source">Выбранный источник данных</param>
	/// <returns>Новый экземпляр подходящего сборщика</returns>
	ICollector? GetCollector(SourceWithTagsInfo source);
}
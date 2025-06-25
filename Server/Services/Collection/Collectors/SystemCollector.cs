using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;

namespace Datalake.Server.Services.Collection.Collectors;

/// <summary>
/// Системный сборщик данных, собирающий информацию о работе сервера
/// </summary>
/// <param name="source">Источник данных</param>
/// <param name="logger">Служба сообщений</param>
internal class SystemCollector(
	SourceWithTagsInfo source,
	ILogger<SystemCollector> logger) : CollectorBase(source.Name, source, logger)
{
}

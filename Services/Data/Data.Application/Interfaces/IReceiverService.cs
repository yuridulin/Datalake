using Datalake.Data.Application.Models;
using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Interfaces;

/// <summary>
/// Служба получения данных из источников по сети
/// </summary>
public interface IReceiverService
{
	/// <summary>
	/// Запрос данных
	/// </summary>
	/// <param name="sourceType">Тип источника данных</param>
	/// <param name="items">Список запрашиваемых значений</param>
	/// <param name="address">Адрес сервера</param>
	/// <returns>Ответ с данными</returns>
	Task<RemoteResponseDto> AskSourceAsync(SourceType sourceType, string[]? items, string? address, int? port)
	{
		return sourceType switch
		{
			SourceType.Inopc => AskInopcAsync(items, address, port),

			_ => throw new ApplicationException("Тип источника данных не поддерживает получение удаленных значений"),
		};
	}

	/// <summary>
	/// Запрос данных из сервера INOPC
	/// </summary>
	/// <param name="items">Список запрашиваемых значений</param>
	/// <param name="address">Адрес сервера</param>
	/// <returns>Ответ с данными</returns>
	Task<RemoteResponseDto> AskInopcAsync(string[]? items, string? address, int? port);
}